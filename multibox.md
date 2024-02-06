If you want to use more than one instance of the game at the same time or if you want to use AutoRetainer on different accounts on the same machine, run first instance normally and for any subsequent instances create one bat file for each instance with this text:
```
start "" /d "%USERPROFILE%\AppData\Local\XIVLauncher" "%USERPROFILE%\AppData\Local\XIVLauncher\XIVLauncher.exe" --roamingPath="c:\XivLauncher2"
```
Replace `c:\XivLauncher2` with path to would be second Xivlauncher configuration folder. Each instance/account needs to use it's own bat file and it's own folder (don't forget to actually create these folder). 
