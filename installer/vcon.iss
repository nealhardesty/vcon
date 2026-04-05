; vcon — Inno Setup installer script
; Compiles to a single setup EXE for end-user installation.
;
; Build from repo root:
;   iscc /DAppVersion=0.1.0 /DPublishDir=..\src\Vcon.Overlay\bin\Release\net8.0-windows\win-x64\publish /DDistDir=..\dist installer\vcon.iss
;
; Or use the Makefile:
;   make installer-exe

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif

#ifndef PublishDir
  #define PublishDir "..\src\Vcon.Overlay\bin\Release\net8.0-windows\win-x64\publish"
#endif

#ifndef DistDir
  #define DistDir "..\dist"
#endif

[Setup]
AppId={{E8A3F2B1-7C4D-4E5A-9B6F-1D2E3F4A5B6C}
AppName=vcon
AppVersion={#AppVersion}
AppVerName=vcon {#AppVersion}
AppPublisher=neal
AppPublisherURL=https://github.com/nealhardesty/vcon
AppSupportURL=https://github.com/nealhardesty/vcon/issues
AppUpdatesURL=https://github.com/nealhardesty/vcon/releases
DefaultDirName={autopf}\vcon
DefaultGroupName=vcon
UninstallDisplayIcon={app}\vcon.exe
OutputDir={#DistDir}
OutputBaseFilename=vcon-{#AppVersion}-setup
Compression=lzma2/ultra64
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
WizardStyle=modern
PrivilegesRequired=admin
SetupIconFile=..\src\Vcon.Overlay\vcon.ico
LicenseFile=..\LICENSE
VersionInfoVersion={#AppVersion}.0
VersionInfoProductName=vcon
VersionInfoCompany=neal
VersionInfoCopyright=Copyright © 2026 neal

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startupentry"; Description: "Launch vcon at Windows startup"; GroupDescription: "Other:"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\vcon"; Filename: "{app}\vcon.exe"
Name: "{group}\{cm:UninstallProgram,vcon}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\vcon"; Filename: "{app}\vcon.exe"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "vcon"; ValueData: """{app}\vcon.exe"""; Flags: uninsdeletevalue; Tasks: startupentry

[Run]
Filename: "{app}\vcon.exe"; Description: "{cm:LaunchProgram,vcon}"; Flags: nowait postinstall skipifsilent
