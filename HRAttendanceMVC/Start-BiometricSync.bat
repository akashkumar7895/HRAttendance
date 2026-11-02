@echo off
title Realtime Biometric Sync Tool - HR Attendance
cls
echo ==================================================================
echo         REALTIME BIOMETRIC IP SYNC TOOL (100%% SAFE READ-ONLY)
echo ==================================================================
echo.
echo Connecting to Realtime Biometric Machine at IP 192.168.29.158...
echo Pushing new punches to https://hrattendance.tryasp.net...
echo.

if exist "%~dp0..\RealtimeSyncTool\publish\RealtimeSyncTool.exe" (
    cd /d "%~dp0..\RealtimeSyncTool\publish"
    RealtimeSyncTool.exe
) else (
    cd /d "%~dp0..\RealtimeSyncTool"
    dotnet run
)

echo.
pause
