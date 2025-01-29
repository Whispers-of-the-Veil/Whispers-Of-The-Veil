#!/bin/bash
if [ -z "$1" ]; then
    echo "Building API"

    pyinstaller --onefile --add-data "models/ASR.keras:models" --add-data "Data/Process.py:Data" --add-data "Model/ASRModel.py:Model" --add-data "Grab_Ini.py:." API/ASR_API.py

    cp config.ini dist/config.ini
elif [ "$1" == "clean" ]; then
    echo "Cleaning up build files"

    rm -r dist
    rm -r build
    rm ASR_API.spec
fi

echo "done"