; dist
Source: "{#MyDistFolder}\DotNetFX40KB2468871\dotNetFx40_Full_x86_x64.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsRequiredDotNetDetected
Source: "{#MyDistFolder}\DotNetFX40KB2468871\NDP40-KB2468871-v2-x86.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsRequiredDotNetUpdateDetected
Source: "{#MyDistFolder}\DotNetFX40KB2468871\ru\dotNetFx40LP_Full_x86_x64ru.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsRequiredDotNetLPDetected
Source: "{#MyDistFolder}\SyncFramework21\Synchronization-v2.1-x86-ENU.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsRequiredSyncFrDetected
Source: "{#MyDistFolder}\SyncFramework21\DatabaseProviders-v3.1-x86-ENU.msi"; DestDir: "{tmp}"; Flags: deleteafterinstall; Check: not IsRequiredSyncFrDetected
; common dlls
Source: "{#ClientBuildOutputFolder}\bin\Diagnosis.Common.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\Diagnosis.Common.Presentation.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\Diagnosis.Data.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\Diagnosis.Models.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\Diagnosis.ViewModels.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\EventAggregator.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\FluentMigrator.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\FluentMigrator.Runner.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\FluentValidation.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\FluentValidation.resources.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\GongSolutions.Wpf.DragDrop.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\Iesi.Collections.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\log4net.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\NHibernate.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\PowerCollections.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\sqlceca40.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\sqlcecompact40.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\sqlceer40EN.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\sqlceme40.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\sqlceqp40.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\sqlcese40.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\SQLite.Interop.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\System.Data.SQLite.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\System.Data.SqlServerCe.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\Microsoft.VC90.CRT\msvcr90.dll"; DestDir: "{app}\bin"; Flags: sharedfile
Source: "{#ClientBuildOutputFolder}\bin\Microsoft.VC90.CRT\Microsoft.VC90.CRT.manifest"; DestDir: "{app}\bin"; Flags: sharedfile
