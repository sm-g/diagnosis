#define MyAppName "Diagnosis"
#define MyAppVersion "1.8.2"
#define MyAppPublisher "NBI KemSU"
#define ClientAppName "Diagnosis"
#define ServerAppName "Diagnosis on server"
#define MyDistFolder "dist" 
#define ServerAppExeName "Diagnosis.Server.App.exe"
#define ClientAppExeName "Diagnosis.Client.App.exe"
#define ServerBuildOutputFolder "..\Diagnosis.Server.App\bin\Release"  
#define ClientBuildOutputFolder "..\Diagnosis.Client.App\bin\Release" 
#define MyOutputFolder "..\Release" 
#define LogsSubFolder "NBI_KemSU\Diagnosis\Logs"  
#define ClientDbSubFolder "NBI_KemSU\Diagnosis\Client"  
#define ClientMutex "ac2ee38e-31c5-45f5-8fde-4a9a126df451"
#define ServerMutex "2c2ee38e-31c5-45f5-8fde-4a9a126df452"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
