import librosa
import numpy as np
import json
import sys

def get_pitch_per_frame(y, sr, frame_length=2048, hop_length=512, fmin=50, fmax=2000):
    
    pitches, voiced_flags, _ = librosa.pyin(
        y,
        fmin=fmin,
        fmax=fmax,
        frame_length=frame_length,
        hop_length=hop_length,
        sr=sr,
        fill_na=0
    )

    times = librosa.frames_to_time(
        np.arange(len(pitches)),
        hop_length=hop_length,
        sr=sr
    )

    return times, pitches

if __name__ == "__main__":
    audio_path = sys.argv[1]
    y, sr = librosa.load(audio_path, sr=None)
    
    times, pitches = get_pitch_per_frame(y, sr)
    
    result = []
    for time, pitch in zip(times, pitches):
        result.append({
            "time": float(time),
            "pitch": float(pitch)
        })
        
    print(json.dumps(result, indent= 4 ))