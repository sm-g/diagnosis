Filename: "{tmp}\dotNetFx40_Full_x86_x64"; Parameters: "/q /norestart"; StatusMsg: "установка Microsoft .NET Framework 4.0 Full Profile..."; Check: not IsRequiredDotNetDetected
Filename: "{tmp}\NDP40-KB2468871-v2-x86"; Parameters: "/q /norestart"; StatusMsg: "установка Microsoft .NET Framework 4.0 Full Profile. KB2468871..."; Check: not IsRequiredDotNetUpdateDetected
Filename: "{tmp}\dotNetFx40LP_Full_x86_x64ru"; Parameters: "/q /norestart"; StatusMsg: "установка Microsoft .NET Framework 4.0 Full Profile. RU LP..."; Check: not IsRequiredDotNetLPDetected
Filename: "{tmp}\Synchronization-v2.1-x86-ENU.msi"; Parameters: "/quiet"; Flags: shellexec; StatusMsg: "установка Sync Framework 2.1..."; Check: not IsRequiredSyncFrDetected
