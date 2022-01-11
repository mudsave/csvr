@echo off
set curpath=%~dp0

set KBE_ROOT=%curpath%
set KBE_RES_PATH=%KBE_ROOT%kbe/res/;%KBE_ROOT%/res/
set KBE_BIN_PATH=%KBE_ROOT%kbe/bin/Hybrid/

cd %curpath%

echo KBE_ROOT = %KBE_ROOT%
echo KBE_RES_PATH = %KBE_RES_PATH%
echo KBE_BIN_PATH = %KBE_BIN_PATH%

start %KBE_BIN_PATH%/bots.exe
