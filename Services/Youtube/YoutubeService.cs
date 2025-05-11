using System.Diagnostics;
using System.Threading.Tasks;
using ai_it_wiki.Services.TelegramBot;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Telegram.Bot;
using VideoLibrary;
using Hangfire;

namespace ai_it_wiki.Services.Youtube
{
    public class YoutubeService
    {
        public YoutubeService() { }

        /// <summary>
        /// Метод объединения видео и аудио в один файл с помощью FFmpeg
        /// </summary>
        /// <param name="videoPath">Путь к видео</param>
        /// <param name="audioPath">Путь к аудио</param>
        /// <param name="outputPath">Путь к конечному файлу</param>
        private bool MergeVideoAndAudio(string videoPath, string audioPath, string outputPath)
        {
            try
            {
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                string ffmpegPath = Path.Combine(AppContext.BaseDirectory, "ffmpeg", "bin", "ffmpeg.exe");

                string arguments = $"-i \"{videoPath}\" -i \"{audioPath}\" -c:v copy -c:a aac -strict experimental \"{outputPath}\"";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = ffmpegPath,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();

                    // Читаем оба потока в фоне, чтобы не блокировать процесс
                    Task.Run(() => process.StandardOutput.ReadToEnd());
                    Task.Run(() => process.StandardError.ReadToEnd());

                    // Ждем завершения процесса
                    process.WaitForExit();

                    process.Close();
                }
                if (!File.Exists(outputPath))
                {
                    throw new Exception("Ошибка при объединении видео и аудио");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка при объединении видео и аудио: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Метод скачивания видео и аудио с YouTube через VideoLibrary
        /// </summary>
        /// <param name="url">Ссылка на видео</param>
        /// <param name="videoPath">Путь сохранения видео</param>
        /// <param name="audioPath">Путь сохранения аудио</param>
        private async Task<DownloadedVideoData> DownloadYouTubeVideoWithAudio(string url, string baseUserPath, TelegramBotService botClient, long userId)
        {
            try
            {
                var youtube = YouTube.Default;

                var allVideos = await youtube.GetAllVideosAsync(url);

                // Загружаем видео (без звука)
                var video = allVideos
                    .Where(v => v.Format == VideoFormat.Mp4)
                    .OrderByDescending(v => v.Resolution)
                    .Where(v => ((v.ContentLength / 1024) / 1024) < 200)
                    .FirstOrDefault();

                botClient.SendTextMessageAsync(userId, JsonConvert.SerializeObject(video, Formatting.Indented));

                if (video == null)
                {
                    throw new NullReferenceException("Ошибка: Не удалось получить видеопоток");
                }

                // Загружаем аудио (MP4 формат)
                var audio = allVideos
                    .Where(a => a.AudioFormat == AudioFormat.Aac)
                    .OrderByDescending(a => a.AudioBitrate)
                    .FirstOrDefault();

                var videoPath = Path.Combine(baseUserPath, "video_" + video.FullName);
                var audioPath = Path.Combine(baseUserPath, "audio_" + video.FullName);

                // Получаем байты видео и аудио ПАРАЛЛЕЛЬНО
                var videoBytesTask = video.GetBytesAsync();
                var audioBytesTask = audio.GetBytesAsync();

                var videoBytes = await videoBytesTask;
                var audioBytes = await audioBytesTask;

                // Записываем файлы ПАРАЛЛЕЛЬНО
                var videoWriteTask = System.IO.File.WriteAllBytesAsync(videoPath, videoBytes);
                var audioWriteTask = System.IO.File.WriteAllBytesAsync(audioPath, audioBytes);

                await Task.WhenAll(videoWriteTask, audioWriteTask);

                return new DownloadedVideoData
                {
                    AudioPath = audioPath,
                    VideoPath = videoPath,
                    FinalOutputPath = Path.Combine(baseUserPath, video.FullName)
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Метод получения информации о видео по ссылке
        public async Task<IEnumerable<YouTubeVideo>> GetVideoInfo(string videoUrl)
        {
            try
            {
                var youtube = YouTube.Default;
                return await youtube.GetAllVideosAsync(videoUrl);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DownloadedVideo> DownloadVideo(string videoUrl, long userId, TelegramBotService _botClient)
        {
            //отправлякм сообщение о начале загрузки
            await _botClient.SendTextMessageAsync(userId, "Загрузка видео началась. Пожалуйста, подождите...");

            var baseUserPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "Youtube", userId.ToString());

            //проверяем наличие папки
            if (!Directory.Exists(baseUserPath))
            {
                Directory.CreateDirectory(baseUserPath);
            }

            //отправляем статус активности "записывает видео" каждые 4 секунды, пока идет загрузка
            new Timer((e) =>
            {
                _botClient.SendChatActionAsync(userId, Telegram.Bot.Types.Enums.ChatAction.RecordVideo);
            }, null, 0, period: 4 * 1000);

            try
            {
                // Скачиваем видео и аудио
                DownloadedVideoData downloadedVideoData = await DownloadYouTubeVideoWithAudio(videoUrl, baseUserPath, _botClient, userId);

                // сообщаем о завершении загрузки
                if (downloadedVideoData == null)
                {
                    await _botClient.SendTextMessageAsync(userId, "Ошибка при загрузке видео. Попробуйте позже.");
                    return null;
                }

                // Объединяем видео и аудио
                bool mergeSuccess = MergeVideoAndAudio(downloadedVideoData.VideoPath, downloadedVideoData.AudioPath, downloadedVideoData.FinalOutputPath);

                if (mergeSuccess)
                {
                    File.Delete(downloadedVideoData.VideoPath);
                    File.Delete(downloadedVideoData.AudioPath);

                    // _botClient.SendTextMessageAsync(userId, ((new FileInfo(downloadedVideoData.FinalOutputPath).Length / 1024) / 1024).ToString() + "mb");

                    //если размер файла больше 50 мб, то отправляем ссылку на файл
                    if (new FileInfo(downloadedVideoData.FinalOutputPath).Length > (50 * 1024 * 1024))
                    {
                        //получаем ссылку на файл
                        var staticFileUrl = Path.Combine("https://it-wiki.site", "Youtube", userId.ToString(), Uri.EscapeDataString(Path.GetFileName(downloadedVideoData.FinalOutputPath)) );
                        staticFileUrl = staticFileUrl.Replace("\\", "/");
                        //кодируем ссылку для отправки в сообщении
                      
                        await _botClient.SendTextMessageAsync(userId, staticFileUrl);
                        //запускаем таймер на удаление файла через 20 минут
                      
                      
                         BackgroundJob.Schedule(() => File.Delete(downloadedVideoData.FinalOutputPath), TimeSpan.FromMinutes(20));

                    }
                    else
                    {
                        _botClient.SendChatActionAsync(userId, Telegram.Bot.Types.Enums.ChatAction.UploadVideo);

                        using (var fs = new FileStream(downloadedVideoData.FinalOutputPath, System.IO.FileMode.Open))
                        {
                            var inputFile = Telegram.Bot.Types.InputFile.FromStream(fs);
                            try
                            {
                                await _botClient.SendVideoAsync(userId, inputFile, caption: Path.GetFileNameWithoutExtension(downloadedVideoData.FinalOutputPath));
                            }
                            catch (Exception ex)
                            {
                                await _botClient.SendTextMessageAsync(userId, ex.Message);
                            }
                        }
                        File.Delete(downloadedVideoData.FinalOutputPath);
                    }
                    GC.KeepAlive(_botClient);
                    return new DownloadedVideo { Path = downloadedVideoData.FinalOutputPath, Name = downloadedVideoData.FinalOutputPath };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }

          

        }




     

    }
    public class DownloadedVideo
    {
        public string Path { get; set; }
        public string Name { get; set; }
    }
    public class DownloadedVideoData
    {
        public string AudioPath { get; set; }
        public string VideoPath { get; set; }
        public string FinalOutputPath { get; set; }
    }
}