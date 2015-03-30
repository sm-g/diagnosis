set msbuild=%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
REM set targets=%*
REM if /i "%targets%" == "" set targets=Start
%msbuild% build.msbuild /p:App="%1" /p:BuildRelease="%2" /p:BuildAndZipSetup="%3"
rem /verbosity:minimal
@pause
