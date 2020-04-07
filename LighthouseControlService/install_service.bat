
sc create LighthouseControl BinPath="%~dp0\LighthouseControlService.exe"
sc config LighthouseControl start=auto
sc start LighthouseControl