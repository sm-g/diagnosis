#include "constants.iss"

[Setup]
AppId={{081A185B-DDB7-45DF-B2F7-FE6E49513AEC}
AppName={#ClientAppName}
AppVerName={#ClientAppName} {#MyAppVersion}
AppMutex={#ClientMutex}  
OutputBaseFilename=diagnosis-client-{#MyAppVersion}   
UninstallDisplayIcon={app}\{#ClientAppExeName}
;common
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
VersionInfoVersion={#MyAppVersion}
MinVersion=0,5.01sp3
DisableProgramGroupPage=auto
DisableDirPage=auto
AllowNoIcons=yes
Compression=lzma2
OutputDir={#MyOutputFolder}

#include "messages.iss"

[Files]
; exe
Source: "{#ClientBuildOutputFolder}\Diagnosis.Client.App.exe"; DestDir: "{app}"
Source: "{#ClientBuildOutputFolder}\Diagnosis.Client.App.exe.config"; DestDir: "{app}"

#include "common-files.iss"

; client dlls
Source: "{#ClientBuildOutputFolder}\bin\Xceed.Wpf.AvalonDock.resources.dll"; DestDir: "{app}\bin\"
Source: "{#ClientBuildOutputFolder}\ru\Xceed.Wpf.AvalonDock.resources.dll"; DestDir: "{app}\bin\ru\"
Source: "{#ClientBuildOutputFolder}\bin\Xceed.Wpf.AvalonDock.Themes.Metro.dll"; DestDir: "{app}\bin\"
Source: "{#ClientBuildOutputFolder}\bin\Xceed.Wpf.DataGrid.dll"; DestDir: "{app}\bin\"
Source: "{#ClientBuildOutputFolder}\bin\EPPlus.dll"; DestDir: "{app}\bin"

; client help
Source: "{#ClientBuildOutputFolder}\Help\Hotkeys.html"; DestDir: "{app}\Help\"
Source: "{#ClientBuildOutputFolder}\Help\Index.html"; DestDir: "{app}\Help\"
Source: "{#ClientBuildOutputFolder}\Help\main.css"; DestDir: "{app}\Help\"

[Icons]
Name: "{group}\{#ClientAppName}"; Filename: "{app}\{#ClientAppExeName}"
Name: "{group}\{cm:Logs}"; Filename: "{localappdata}\{#LogsSubFolder}" ; Flags: uninsneveruninstall
Name: "{group}\{cm:Db}"; Filename: "{localappdata}\{#ClientDbSubFolder}"
Name: "{commondesktop}\{#ClientAppName}"; Filename: "{app}\{#ClientAppExeName}"; Tasks: desktopicon

[Run]
#include "run-distr.iss"
Filename: "{app}\{#ClientAppExeName}"; Flags: nowait postinstall skipifsilent unchecked; Description: "{cm:LaunchProgram,{#StringChange(ClientAppName, '&', '&&')}}"

[Dirs]
Name: "{app}\Help"
Name: "{app}\bin"
Name: "{app}\bin\ru"
Name: "{localappdata}\{#LogsSubFolder}"; Flags: uninsneveruninstall
Name: "{localappdata}\{#ClientDbSubFolder}"; Flags: uninsneveruninstall

#include "code.iss"
