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

cd /d "%~dp0RealtimeSyncTool\publish"
if exist "RealtimeSyncTool.exe" (
    RealtimeSyncTool.exe
) else (
    cd /d "%~dp0RealtimeSyncTool"
    dotnet run
)

echo.
pause
