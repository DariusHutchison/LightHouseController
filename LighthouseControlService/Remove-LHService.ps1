#Requires -RunAsAdministrator
Stop-Service -Name LighthouseControl -Force
Remove-Service -Name LighthouseControl
Read-Host "Service Removed"

#sc stop LighthouseControl
#sc delete LighthouseControl