[Code]
var 
  syncCoreInstalled: boolean;
  
procedure InitializeWizard(); 
begin 
    syncCoreInstalled := false;
end;

// http://kynosarges.org/DotNetVersion.html
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//    'v4.5'          .NET Framework 4.5
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, release, serviceCount: cardinal;
    check45, success: boolean;
var reqNetVer : string;
begin
    // .NET 4.5 installs as update to .NET 4.0 Full
    if version = 'v4.5' then begin
        version := 'v4\Full';
        check45 := true;
    end else
        check45 := false;

    // installation key group for all .NET versions
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;

    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;

    // .NET 4.0/4.5 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;

    // .NET 4.5 uses additional value Release
    if check45 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Release', release);
        success := success and (release >= 378389);
    end;

    result := success and (install = 1) and (serviceCount >= service);
end;

function IsRequiredDotNetDetected(): Boolean;  
begin
    result := IsDotNetDetected('v4\Full', 0);
end;

function IsRequiredSyncFrDetected(): Boolean; 
var
    key: string;
    install: cardinal;
    success: boolean; 
begin
    key:= 'SOFTWARE\Microsoft\Microsoft Sync Framework\v2.1\Setup\SynchronizationX86';
    success := RegQueryDWordValue(HKLM, key, 'Install', install);
    result := success and (install = 1);
end;

procedure SetSyncCoreInstalled();
begin
    syncCoreInstalled := true;
end;

function IsNeedToInstallDatabaseProviders(): Boolean; 
begin
    result := syncCoreInstalled;
end;

function IsRequiredDotNetLPDetected(): Boolean; 
var
    key: string;
    install: cardinal;
    success: boolean; 
begin
    key:= 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\1049';
    success := RegQueryDWordValue(HKLM, key, 'Install', install);
    result := success and (install = 1);
end;

function IsRequiredDotNetUpdateDetected(): Boolean; 
var
    key: string;
    install: string;
    success: boolean; 
begin
    key:= 'SOFTWARE\Microsoft\Updates\Microsoft .NET Framework 4 Extended\KB2468871';
    success := RegQueryStringValue(HKLM, key, 'ThisVersionInstalled', install);
    result := success and (install = 'Y');
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var 
	allRemoved: boolean;
	item: String;
begin
	if CurUninstallStep = usPostUninstall then
		begin
		allRemoved := not FileExists(ExpandConstant('{app}\{#ServerAppExeName}')) and not FileExists(ExpandConstant('{app}\{#ClientAppExeName}'))
		if allRemoved then 
			begin
			item := ExpandConstant('{group}\{cm:Logs}.lnk');
			DeleteFile(item);
			RemoveDir(ExpandConstant('{group}'));
          end;
		end;
end;