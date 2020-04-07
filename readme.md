# LighthouseController

A utility for waking Valve's Lighthouse V2 units from sleep, or sending them back to it.

## Building

1. Check out source code
2. dotnet publish

## Manual running
Launch LighthouseControlCmd.exe with either an 'on' or 'off' argument

## Automated running
Once built, navigate to the LighthouseConstrolService publish directory and run install_service.bat as admin to install the service. It watches, every second, for a launch of SteamVR and wakes the lighthouses. Then it watches for SteamVR to exit, and puts the lighthouses to sleep

## Configuring Windows
Users will probably need to allow their PC to talk to unpaired devices. This can be done via:
```
Settings > Privacy > Other Devices (left column) > Communicate with other devices
```