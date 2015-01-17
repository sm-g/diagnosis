set msbuild=%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe
REM set targets=%*
REM if /i "%targets%" == "" set targets=Start
%msbuild% build.msbuild /p:BuildRelease="%1" /p:ZipRelease="%2" /p:CopyWorking="%3" /verbosity:minimal
@pause
