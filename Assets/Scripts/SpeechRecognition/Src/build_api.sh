#!/bin/bash
if [ -z "$1" ]; then
    echo "Building API"

    pyinstaller --onefile --add-data "models/ASR.keras:models" --add-data "Data/Process.py:Data" --add-data "Model/ASRModel.py:Model" --add-data "Grab_Ini.py:." --add-data "config.ini:." API/ASR_API.py
elif [ "$1" == "clean" ]; then
    echo "Cleaning up build files"

    rm -r dist
    rm -r build
    rm ASR_API.spec
fi

echo "done"