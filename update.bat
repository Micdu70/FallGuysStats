@echo off
title Fall Guys Stats "FE" Update Script
echo Checking...
if exist "%~dp0\update\" (
  move "%~dp0\update\"*.* .
  rmdir /S /Q "%~dp0\update"
  start FallGuysStats.exe
  exit
)
echo No update data found...
pause
exit