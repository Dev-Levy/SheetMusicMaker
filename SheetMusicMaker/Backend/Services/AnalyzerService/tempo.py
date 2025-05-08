import librosa
import json
import sys

def get_tempo(y, sr):
    tempo, _ = librosa.beat.beat_track(y=y, sr=sr)
    return tempo

if __name__ == "__main__":
    audio_path = sys.argv[1]
    y, sr = librosa.load(audio_path, sr=None)

    tempo = get_tempo(y, sr)
    duration = librosa.get_duration(y=y, sr=sr)

    result = {
        "tempo": float(tempo),
        "duration": float(duration)
    }

    print(json.dumps(result, indent=4))