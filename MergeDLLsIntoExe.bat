set IL_MERGE="D:\Program Files (x86)\Microsoft\ILMerge\ilmerge.exe"
set TARGET_PLATFORM=C:\Windows\Microsoft.NET\Framework64\v4.0.30319
set OUTPUT_EXE=Build\Gw2BuildHelper.exe
set INPUT_EXE=Source\bin\Release\Gw2BuildHelper.exe
set INPUT_DLL1=Source\bin\Release\Newtonsoft.Json.dll
set INPUT_DLL2=Source\bin\Release\MumbleLink-CSharp.dll
set INPUT_DLL3=Source\bin\Release\MumbleLink-CSharp-GW2.dll

%IL_MERGE% /targetplatform:v4,%TARGET_PLATFORM% /lib:%TARGET_PLATFORM%\WPF /out:%OUTPUT_EXE% %INPUT_EXE% %INPUT_DLL1% %INPUT_DLL2% %INPUT_DLL3%
pause