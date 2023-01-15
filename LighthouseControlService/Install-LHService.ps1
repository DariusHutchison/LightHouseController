#Requires -RunAsAdministrator
New-Service -Name LighthouseControl -BinaryPathName "$PSScriptRoot\LighthouseControlService.exe" -StartupType Automatic -DisplayName "Lighthouse Control" -Description "This is a service that will monitor to start or stop lighthouses on SteamVR starting or exiting."
Start-Service -Name LighthouseControl
Read-Host "Service Installed"

#sc create LighthouseControl BinPath="%~dp0\LighthouseControlService.exe"
#sc config LighthouseControl start=auto
#sc start LighthouseControl