@echo off
rem Lo ejecuta el Trigger de MSMQ. Deja la salida en un log porque corre sin consola.
echo ---- %DATE% %TIME% >> C:\AukanGym\bridge\bridge.log
echo args: %* >> C:\AukanGym\bridge\bridge.log
"C:\Program Files\Eclipse Adoptium\jdk-17.0.16.8-hotspot\bin\java.exe" -cp C:\AukanGym\bridge\aukan.jar cl.iplacex.aukan.Bridge %* >> C:\AukanGym\bridge\bridge.log 2>&1
echo salida: %ERRORLEVEL% >> C:\AukanGym\bridge\bridge.log
