@echo off
set retryNumber=0
set maxRetries=5

:RESTORE

nuget restore CanvasDiagram.sln

IF NOT ERRORLEVEL 1 GOTO :EOF

@echo Nuget restore exited with code %ERRORLEVEL%!
set /a retryNumber=%retryNumber%+1
IF %reTryNumber% LSS %maxRetries% (GOTO :RESTORE)

@echo Restoring nuget packages %maxRetries% times was unsuccessful!

EXIT /B 1
