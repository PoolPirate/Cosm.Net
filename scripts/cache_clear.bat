for /d %%G in ("C:\Users\elias\.nuget\packages\cosm.net.*") do rd /s /q "%%~G"
for /d %%G in ("C:\Users\elias\Desktop\Package Source\cosm.net.*") do del "%%~G"

copy scripts\out\* "C:\Users\elias\Desktop\Package Source" /Y
