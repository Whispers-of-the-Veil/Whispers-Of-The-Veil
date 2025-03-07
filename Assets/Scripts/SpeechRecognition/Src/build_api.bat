@echo off
if "%~1"=="" (
    echo Building API

    pyinstaller --onefile --add-data "models/ASR.keras;models" --add-data "Data/Process.py;Data" --add-data "Data/NLP.py;Data" --add-data "Model/ASRModel.py;Model" --add-data "Grab_Ini.py;." --hidden-import language_tool_python API/ASR_API.py

    copy config.ini dist\config.ini
) else if "%~1"=="clean" (
    echo Cleaning up build files

    rmdir /s /q dist
    rmdir /s /q build
    del ASR_API.spec
)

echo done