@ECHO OFF

SET EDITOR="C:\Program Files\Unity 2019.3.3\Editor"
SET METHOD=TrainingProject.Builder.BuildViaCommandLine

ECHO Konane's
ECHO Excute: %EDITOR%
ECHO Excute Method: %METHOD%

SET DIR=--buildingFolder %1
SET VERSION=--buildingVersion %2
IF "%~1"=="" ( SET DIR= )
IF "%~2"=="" ( SET VERSION= )
SET HOME=%PATH%
SET PATH=%PATH%;%EDITOR%
Unity -batchmode -projectPath "../" -executeMethod TrainingProject.Builder.BuildViaCommandLine %DIR% %VERSION%
SET PATH=%HOME%

IF NOT %ERRORLEVEL%==0 ( ECHO ERROR %ERRORLEVEL% )

PAUSE
