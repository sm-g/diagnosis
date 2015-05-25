call build.cmd client 1 0
call build.cmd server 1 0

set mstest="%programfiles(x86)%\Microsoft Visual Studio 12.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe"
%mstest% Diagnosis.Common.Tests\bin\Debug\Diagnosis.Common.Tests.dll  Diagnosis.Models.Tests\bin\Debug\Diagnosis.Models.Tests.dll  Diagnosis.Data.Tests\bin\Debug\Diagnosis.Data.Tests.dll Diagnosis.ViewModels.Tests\bin\Debug\Diagnosis.ViewModels.Tests.dll /logger:trx
