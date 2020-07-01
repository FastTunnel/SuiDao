@echo off

for /d %%p in (SuiDao.Client,SuiDao.Server) do (
  CD ./src/%%p 
 
  for  %%I in (win-x64,osx-x64,linux-x64) do (
	dotnet publish -o=../../publish/%%p.%%I -c=release -r=%%I /p:PublishSingleFile=true /p:PublishTrimmed=true & echo f |xcopy %~dp0\src\%%p\appsettings.json %~dp0publish\%%p.%%I\appsettings.json & 7z a -tzip ../../publish/%%p.%%I.zip ../../publish/%%p.%%I
  )
  cd ../../
)

pause