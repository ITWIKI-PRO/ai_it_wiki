//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.Formats.Jpeg;
//using SixLabors.ImageSharp.Formats.Png;
//using SixLabors.ImageSharp.Formats.Webp;
//using SixLabors.ImageSharp.Formats;
//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing;

//using System;
//using System.Runtime.Versioning;

//public static class ImageProcessor
//{
//  static string inputFolder = Path.Combine("recipes", "images", "input");
//  static string outputFolder = Path.Combine("recipes", "images", "output");
//  static string logoPath = Path.Combine("recipes", "logo.png");

//  [SupportedOSPlatform("windows")]
//  public static void AddLogoToImages_Altron()
//  {


//    if (!Directory.Exists(outputFolder))
//      Directory.CreateDirectory(outputFolder);

//    string[] files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly);

//    Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 4 }, file =>
//    {
//      try
//      {
//        using (Image image = Image.Load(file)) // ✅ Формат теперь определяется автоматически
//        using (Image logo = Image.Load(logoPath))
//        {
//          int logoWidth = image.Width / 5;
//          int logoHeight = logo.Height * logoWidth / logo.Width;
//          int x = image.Width - logoWidth - 10;
//          int y = image.Height - logoHeight - 10;

//          image.Mutate(ctx => ctx.DrawImage(logo, new Point(x, y), 1f));

//          // Определяем нужный формат файла
//          string extension = Path.GetExtension(file).ToLower();
//          IImageEncoder encoder = extension switch
//          {
//            ".png" => new PngEncoder(),
//            ".jpg" or ".jpeg" => new JpegEncoder(),
//            ".webp" => new WebpEncoder(),
//            _ => new JpegEncoder() // По умолчанию сохраняем в JPEG
//          };

//          string outputFile = Path.Combine(outputFolder, Path.GetFileName(file));
//          image.Save(outputFile, encoder); // ✅ Теперь формат корректно сохраняется
//          Console.WriteLine($"✅ Обработано: {outputFile}");
//        }
//      }
//      catch (Exception ex)
//      {
//        Console.WriteLine($"❌ Ошибка обработки {file}: {ex.Message}");
//      }
//    });
//  }

//  public static void FixImageFromAI(string inputPath, string outputPath)
//  {
//    using (Image<Rgba32> image = Image.Load<Rgba32>(inputPath))
//    {
//      // 1. Размытие по Гауссу для сглаживания аномалий
//      image.Mutate(x => x.GaussianBlur(1.5f));

//      // 2. Добавление легкого шума
//      Random rnd = new Random();
//      image.Mutate(ctx =>
//      {
//        for (int y = 0; y < image.Height; y++)
//        {
//          for (int x = 0; x < image.Width; x++)
//          {
//            Rgba32 pixel = image[x, y];
//            byte noise = (byte)rnd.Next(-5, 5);
//            pixel.R = (byte)Math.Clamp(pixel.R + noise, 0, 255);
//            pixel.G = (byte)Math.Clamp(pixel.G + noise, 0, 255);
//            pixel.B = (byte)Math.Clamp(pixel.B + noise, 0, 255);
//            image[x, y] = pixel;
//          }
//        }
//      });

//      // 3. Коррекция контраста, насыщенности и гаммы
//      image.Mutate(x =>
//      {
//        x.Contrast(1.05f);    // Контраст +5%
//        x.Saturate(1.1f);     // Насыщенность +10%
//      });

//      // 4. Повышение резкости
//      image.Mutate(x => x.GaussianSharpen(1.5f));

//      // 5. Виньетирование (градиентная тёмная рамка)
//      image.Mutate(ctx => ctx.Vignette());

//      // Сохранение обработанного изображения
//      image.Save(outputPath);
//    }
//  }
//}


using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Webp;
using System.Runtime.Versioning;

public static class ImageProcessor
{
  static string inputFolder = Path.Combine("recipes", "images", "input");
  static string outputFolder = Path.Combine("recipes", "images", "output");
  static string logoPath = Path.Combine("recipes", "logo.png");

  /// <summary>
  /// Массовая обработка изображений с наложением логотипа
  /// </summary>
  public static void ProcessImagesInBatch(string inputFolder, string outputFolder, string logoPath, string processingLevel = "medium")
  {
    if (!Directory.Exists(outputFolder))
      Directory.CreateDirectory(outputFolder);

    string[] files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly);

    Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 4 }, file =>
    {
      try
      {
        string outputFile = Path.Combine(outputFolder, Path.GetFileName(file));

        // Выбираем уровень обработки
        switch (processingLevel.ToLower())
        {
          case "light":
            LightProcessing(file, outputFile, logoPath);
            break;
          case "aggressive":
            AggressiveProcessing(file, outputFile, logoPath);
            break;
          default:
            MediumProcessing(file, outputFile, logoPath);
            break;
        }

        Console.WriteLine($"✅ Обработано: {outputFile}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Ошибка обработки {file}: {ex.Message}");
      }
    });
  }

  /// <summary>
  /// Легкая обработка изображения + добавление логотипа
  /// </summary>
  public static void LightProcessing(string inputPath, string outputPath, string logoPath)
  {
    ProcessImage(inputPath, outputPath, logoPath, 1.0f, -3, 3, 1.05f, 1.05f, 1.0f);
  }

  /// <summary>
  /// Средняя обработка изображения + добавление логотипа
  /// </summary>
  public static void MediumProcessing(string inputPath, string outputPath, string logoPath)
  {
    ProcessImage(inputPath, outputPath, logoPath, 1.5f, -5, 5, 1.10f, 1.10f, 1.5f);
  }

  /// <summary>
  /// Агрессивная обработка изображения + добавление логотипа
  /// </summary>
  public static void AggressiveProcessing(string inputPath, string outputPath, string logoPath)
  {
    ProcessImage(inputPath, outputPath, logoPath, 2.0f, -10, 10, 1.15f, 1.15f, 2.0f);
  }

  /// <summary>
  /// Основной метод обработки изображения + наложение логотипа
  /// </summary>
  private static void ProcessImage(string inputPath, string outputPath, string logoPath, float blurRadius, int noiseMin, int noiseMax, float contrast, float saturation, float sharpenRadius)
  {
    using (Image<Rgba32> image = Image.Load<Rgba32>(inputPath))
    {
      // Размытие по Гауссу
      image.Mutate(x => x.GaussianBlur(blurRadius));

      // Добавление шума
      Random rnd = new Random();
      image.Mutate(ctx =>
      {
        for (int y = 0; y < image.Height; y++)
        {
          for (int x = 0; x < image.Width; x++)
          {
            Rgba32 pixel = image[x, y];
            byte noise = (byte)rnd.Next(-5, 5);
            pixel.R = (byte)Math.Clamp(pixel.R + noise, 0, 255);
            pixel.G = (byte)Math.Clamp(pixel.G + noise, 0, 255);
            pixel.B = (byte)Math.Clamp(pixel.B + noise, 0, 255);
            image[x, y] = pixel;
          }
        }
      });

      // Коррекция контраста и насыщенности
      image.Mutate(x =>
      {
        x.Contrast(contrast);
        x.Saturate(saturation);
      });

      // Повышение резкости
      image.Mutate(x => x.GaussianSharpen(sharpenRadius));

      // Виньетирование
      image.Mutate(x => x.Vignette());

      //// Добавление логотипа, если он существует
      //if (File.Exists(logoPath))
      //{
      //  AddLogo(image, logoPath);
      //}

      // Определение формата сохранения
      string extension = Path.GetExtension(inputPath).ToLower();
      IImageEncoder encoder = extension switch
      {
        ".png" => new PngEncoder(),
        ".jpg" or ".jpeg" => new JpegEncoder(),
        ".webp" => new WebpEncoder(),
        _ => new JpegEncoder() // По умолчанию сохраняем в JPEG
      };

      // Сохранение результата
      image.Save(outputPath, encoder);
    }
  }

  [SupportedOSPlatform("windows")]
  public static void AddLogoToImages()
  {
    if (!Directory.Exists(outputFolder))
      Directory.CreateDirectory(outputFolder);

    string[] files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly);

    Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 4 }, file =>
    {
      try
      {
        using (Image image = Image.Load(file)) // ✅ Формат теперь определяется автоматически
        using (Image logo = Image.Load(logoPath))
        {
          int logoWidth = logo.Width * 2; // Увеличиваем в два раза
          int logoHeight = logo.Height * 2;

          using (Image resizedLogo = logo.Clone(ctx => ctx.Resize(logoWidth, logoHeight)))
          {
            int x = 0;
            int y = 0;

            image.Mutate(ctx => ctx.DrawImage(resizedLogo, new Point(x, y), 1));
          }

          // Определяем нужный формат файла
          string extension = Path.GetExtension(file).ToLower();
          IImageEncoder encoder = extension switch
          {
            ".png" => new PngEncoder(),
            ".jpg" or ".jpeg" => new JpegEncoder(),
            ".webp" => new WebpEncoder(),
            _ => new JpegEncoder() // По умолчанию сохраняем в JPEG
          };

          string outputFile = Path.Combine(outputFolder, Path.GetFileName(file));
          image.Save(outputFile, encoder); // ✅ Теперь формат корректно сохраняется
          Console.WriteLine($"✅ Обработано: {outputFile}");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Ошибка обработки {file}: {ex.Message}");
      }
    });
  }

  public static void FixImageFromAI(string inputPath, string outputPath)
  {
    using (Image<Rgba32> image = Image.Load<Rgba32>(inputPath))
    {
      // 1. Легкое размытие по Гауссу для устранения чрезмерной четкости
      image.Mutate(x => x.GaussianBlur(0.8f));

      // 2. Добавление текстурного шума (имитация структуры пленки)
      Random rnd = new Random();
      image.Mutate(ctx =>
      {
        for (int y = 0; y < image.Height; y++)
        {
          for (int x = 0; x < image.Width; x++)
          {
            Rgba32 pixel = image[x, y];
            int noise = rnd.Next(-4, 4); // Умеренный шум
            pixel.R = (byte)Math.Clamp(pixel.R + noise, 0, 255);
            pixel.G = (byte)Math.Clamp(pixel.G + noise, 0, 255);
            pixel.B = (byte)Math.Clamp(pixel.B + noise, 0, 255);
            image[x, y] = pixel;
          }
        }
      });

      // 3. Коррекция контраста, насыщенности и уменьшение блеска
      image.Mutate(x =>
      {
        x.Contrast(0.95f);    // Уменьшение контраста (-5%)
        x.Saturate(0.92f);    // Уменьшение насыщенности (-8%)
        x.Brightness(0.98f);  // Чуть меньше яркости (-2%)
      });

      // 4. Повышение текстурной резкости (без глянцевого эффекта)
      image.Mutate(x => x.GaussianSharpen(1.2f));

      // 5. Виньетирование (мягкий серый оттенок, плавный градиент)
      image.Mutate(ctx => ctx.Vignette(new GraphicsOptions
      {
        Antialias = true,
        ColorBlendingMode = PixelColorBlendingMode.Multiply,
        AlphaCompositionMode = PixelAlphaCompositionMode.SrcOver
      }, Color.LightGray));

      // Сохранение обработанного изображения
      image.Save(outputPath);
    }
  }

  public static void FixImagesFromAI()
  {
    if (!Directory.Exists(outputFolder))
      Directory.CreateDirectory(outputFolder);
    string[] files = Directory.GetFiles(inputFolder, "*.*", SearchOption.TopDirectoryOnly);
    Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 4 }, file =>
    {
      try
      {
        string outputFile = Path.Combine(outputFolder, Path.GetFileName(file));
        FixImageFromAI(file, outputFile);
        Console.WriteLine($"✅ Обработано: {outputFile}");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"❌ Ошибка обработки {file}: {ex.Message}");
      }
    });
  }
}
