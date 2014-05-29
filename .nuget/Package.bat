msbuild.exe "..\HangFire.Windsor\HangFire.Windsor.csproj" /p:Configuration=Release
nuget.exe pack "..\HangFire.Windsor\HangFire.Windsor.csproj" -Prop Configuration=Release

