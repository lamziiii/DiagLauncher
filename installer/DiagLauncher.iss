; ============================================================
;  DiagLauncher — Script Inno Setup
;  Génère : DiagLauncher_Setup.exe
; ============================================================

#define AppName       "DiagLauncher"
#define AppVersion    "1.0.0"
#define AppPublisher  "DiagLauncher"
#define AppExeName    "DiagLauncher.exe"
#define SourceDir     "..\publish"
#define OutputDir     "..\installer\output"

[Setup]
AppId={{A3F2B1C4-7D8E-4F2A-9B3C-1E5D6F7A8B9C}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir={#OutputDir}
OutputBaseFilename=DiagLauncher_Setup
SetupIconFile=
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
WizardResizable=no
; Pas besoin de droits admin car on écrit aussi dans HKCU
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
; Architecture
ArchitecturesInstallIn64BitMode=x64
ArchitecturesAllowed=x64

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "Créer un raccourci sur le Bureau"; GroupDescription: "Raccourcis :"; Flags: unchecked

[Files]
; --- Exécutable principal et tous les fichiers runtime .NET ---
Source: "{#SourceDir}\*"; DestDir: "{app}"; \
    Flags: ignoreversion recursesubdirs createallsubdirs; \
    Excludes: "assets\*"

; --- Apps portables bundlées (assets\) ---
Source: "{#SourceDir}\assets\*"; DestDir: "{app}\assets"; \
    Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
; Menu Démarrer
Name: "{group}\{#AppName}";    Filename: "{app}\{#AppExeName}"
Name: "{group}\Désinstaller {#AppName}"; Filename: "{uninstallexe}"
; Bureau (optionnel)
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; \
    Tasks: desktopicon

[Run]
; Proposer de lancer l'app à la fin de l'installation
Filename: "{app}\{#AppExeName}"; \
    Description: "Lancer {#AppName}"; \
    Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Supprime le dossier de config utilisateur à la désinstallation (optionnel)
; Type: filesandordirs; Name: "{localappdata}\DiagLauncher"

[Code]
// Supprime la clé de démarrage automatique lors de la désinstallation
procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
    RegDeleteValue(HKCU,
      'SOFTWARE\Microsoft\Windows\CurrentVersion\Run',
      'DiagLauncher');
end;
