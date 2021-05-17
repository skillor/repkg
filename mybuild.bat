@ECHO OFF
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\Tools\VsDevCmd.bat"
@ECHO OFF

msbuild /p:OutputPath="Publish" /p:Configuration=Release RePKG