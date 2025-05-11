import sys
import torch
import ffmpeg
import librosa
import numpy as np
from pyannote.audio.pipelines import SpeakerDiarization
from pyannote.audio import Pipeline
from pyannote.audio.core.io import Audio
from pyannote.core import Segment

import sys
sys.stdout.reconfigure(encoding='utf-8')

def check_import(module_name):
    try:
        __import__(module_name)
    except ModuleNotFoundError:
        print(f"❌ Ошибка: библиотека '{module_name}' не установлена. Установите её через pip.")
        sys.exit(1)

# Проверяем установку всех модулей
check_import("torch")
check_import("librosa")
check_import("pyannote.audio")
check_import("ffmpeg")
check_import("numpy")
check_import("soundfile")

print("✅ Все модули загружены успешно")


# Авторизация с токеном
AUTH_TOKEN = "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"

# Загружаем модель с авторизацией
pipeline = Pipeline.from_pretrained("pyannote/speaker-diarization-3.0", use_auth_token=AUTH_TOKEN)

# Файл аудио (передаётся через аргумент)
#input_file = sys.argv[1]

input_file = 'C:\\Users\\pycek\\source\\repos\\ai_it_wiki\\Services\\VoskTranscription\\test.wav'

# Конвертация в mono WAV, если формат не поддерживается
audio, sr = librosa.load(input_file, sr=16000, mono=True)
temp_wav = input_file.replace(".wav", "_mono.wav")
import soundfile as sf
sf.write(temp_wav, audio, sr)

# Обрабатываем аудиофайл
diarization = pipeline({"uri": "file", "audio": temp_wav})

# Формируем JSON-результат
result = []
for turn, _, speaker in diarization.itertracks(yield_label=True):
    result.append({
        "start": round(turn.start, 2),
        "end": round(turn.end, 2),
        "speaker": speaker
    })

import json



print(json.dumps(result))
