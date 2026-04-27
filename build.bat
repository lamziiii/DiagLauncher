@echo off
setlocal
title DiagLauncher — Build + Installer

echo.
echo  ========================================
echo   DiagLauncher  —  Build ^& Package
echo  ========================================
echo.

:: ── 1. Publish self-contained win-x64 ──────────────────────────────────────
echo [1/3] Publication (self-contained win-x64)...
dotnet publish "DiagLauncher\DiagLauncher.csproj" ^
    -c Release ^
    -r win-x64 ^
    --self-contained true ^
    -o "publish"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo  ERREUR : dotnet publish a echoue.
    pause & exit /b 1
)
echo  OK — dossier publish\ pret.
echo.

:: ── 2. Crée le dossier de sortie installer ─────────────────────────────────
if not exist "installer\output" mkdir "installer\output"

:: ── 3. Compile l'installeur Inno Setup ─────────────────────────────────────
echo [2/3] Recherche d'Inno Setup...

set ISCC=
if exist "%PROGRAMFILES(X86)%\Inno Setup 6\ISCC.exe" set ISCC="%PROGRAMFILES(X86)%\Inno Setup 6\ISCC.exe"
if exist "%PROGRAMFILES(X86)%\Inno Setup 5\ISCC.exe" set ISCC="%PROGRAMFILES(X86)%\Inno Setup 5\ISCC.exe"
if exist "%PROGRAMFILES%\Inno Setup 6\ISCC.exe"      set ISCC="%PROGRAMFILES%\Inno Setup 6\ISCC.exe"

if "%ISCC%"=="" (
    echo.
    echo  INFO : Inno Setup non trouve.
    echo  Telechargez-le sur : https://jrsoftware.org/isdownload.php
    echo.
    echo  Le dossier publish\ est pret — vous pouvez lancer
    echo  manuellement installer\DiagLauncher.iss avec Inno Setup.
    echo.
    echo [3/3] Ouverture du dossier publish\...
    explorer "publish"
    pause & exit /b 0
)

echo  Inno Setup trouve : %ISCC%
echo [3/3] Compilation du setup...
%ISCC% "installer\DiagLauncher.iss"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo  ERREUR : Inno Setup a echoue.
    pause & exit /b 1
)

echo.
echo  ========================================
echo   Setup genere : installer\output\DiagLauncher_Setup.exe
echo  ========================================
echo.
explorer "installer\output"
pause
