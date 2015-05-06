#include "constants.iss"

[Setup]

AppId={{CC9F6654-572C-4A94-94B3-CCEA7A446108}
AppName={#ServerAppName}
AppVerName={#ServerAppName} {#MyAppVersion}
AppMutex={#ServerMutex}  
OutputBaseFilename=diagnosis-server-{#MyAppVersion}   
UninstallDisplayIcon={app}\{#ServerAppExeName}
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
Source: "{#ServerBuildOutputFolder}\{#ServerAppExeName}"; DestDir: "{app}"
Source: "{#ServerBuildOutputFolder}\{#ServerAppExeName}.config"; DestDir: "{app}"

#include "common-files.iss"

[Icons]
Name: "{group}\{#ServerAppName}"; Filename: "{app}\{#ServerAppExeName}"
Name: "{group}\{cm:Logs}"; Filename: "{localappdata}\{#LogsSubFolder}"; Flags: uninsneveruninstall
Name: "{commondesktop}\{#ServerAppName}"; Filename: "{app}\{#ServerAppExeName}"; Tasks: desktopicon

[Run]
#include "run-distr.iss"

[Dirs]
Name: "{app}\bin"
Name: "{app}\bin\ru"

; [Components]
; Name: "serv"; Description: "{#ServerAppName}"; Types: server
; Name: "cli"; Description: "{#ClientAppName}"; Types: client custom

; [Types]
; Name: "client"; Description: {#ClientAppName}
; Name: "server"; Description: {#ServerAppName}
; Name: "custom"; Description: "Выборочная установка"; Flags: iscustom

#include "code.iss"

