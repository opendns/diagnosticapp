@call vc9.bat
@IF ERRORLEVEL 1 goto NO_VC9

@set VERSION=%1
@IF NOT DEFINED VERSION GOTO VERSION_NEEDED

python checkver.py %VERSION%
@IF ERRORLEVEL 1 goto BAD_VERSION

devenv OpenDnsDiagnostic.sln /Project OpenDnsDiagnostic.csproj /ProjectConfig Release /Rebuild
@IF ERRORLEVEL 1 goto BUILD_FAILED
echo Compilation ok!

copy bin\Release\OpenDnsDiagnostic.exe OpenDNSDiagnostic-%VERSION%.exe
goto END

:NO_VC9
@echo vc9.bat failed
@goto END

:BUILD_FAILED
@echo Build failed!
@goto END

:VERSION_NEEDED
@echo Need to provide version number e.g.:
@echo build-release.bat 1.0
@goto END

:BAD_VERSION
@goto END

:END
