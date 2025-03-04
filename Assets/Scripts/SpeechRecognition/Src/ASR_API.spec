# -*- mode: python ; coding: utf-8 -*-


a = Analysis(
    ['API/ASR_API.py'],
    pathex=[],
    binaries=[],
    datas=[('models/ASR.keras', 'models'), ('Data/Process.py', 'Data'), ('Data/NLP.py', 'Data'), ('Model/ASRModel.py', 'Model'), ('Grab_Ini.py', '.')],
    hiddenimports=['language_tool_python'],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[],
    noarchive=False,
    optimize=0,
)
pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.datas,
    [],
    name='ASR_API',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=True,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
)
