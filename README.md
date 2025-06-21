# smb-healthcheck-widget

A simple program that checks your Samba shares and whether they're connected.

## Requirements

- Dotnet >=9.0
- Make

## Install

```
git clone https://github.com/Crozzers/smb-healthcheck-widget
cd smb-healthcheck-widget
make install
```

These commands will install the app and configure it to run on startup.

On Windows the app is installed to `%LOCALAPPDATA\smb-healthcheck-widget`.

On Linux the app is installed to `~/.local/share` and the startup service is configured via systemd.

> Note: On Gnome you will need to enable the AppIndicator and KStatusNotifierIcon extension for the app to function properly.

## Uninstall

```
make uninstall
```