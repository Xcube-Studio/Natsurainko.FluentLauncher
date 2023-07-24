@echo off

SET LOCALIZER=..\FluentLauncher.LocalizationPoroject\build.bat

if not exist %LOCALIZER% (
    echo LOCALIZER NOT FOUND
)

cd ..\FluentLauncher.LocalizationPoroject
call build.bat < nul

SET SOURCE=%cd%\Strings
SET TARGET=%~dp0Natsurainko.FluentLauncher\Strings
xcopy %SOURCE% %TARGET%\ /q /e /r /S /Y

echo COPIED %SOURCE% TO %TARGET%
pause