#Requires -RunAsAdministrator
Stop-Service -Name LighthouseControl -Force
Remove-Service -Name LighthouseControl

#sc stop LighthouseControl
#sc delete LighthouseControl