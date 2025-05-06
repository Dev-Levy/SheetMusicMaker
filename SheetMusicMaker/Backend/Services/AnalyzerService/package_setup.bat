@echo off
echo Installing required Python packages...

python -m pip install --upgrade pip
python -m pip install numpy --no-input --disable-pip-version-check
python -m pip install librosa --no-input --disable-pip-version-check
python -m pip install matplotlib --no-input --disable-pip-version-check

echo Done!