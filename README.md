
# Beatclone Install Tool for Android Devices

This tool runs on a Windows PC, using ADB to connect to your device. This tool Installs the APK, Assets and User file required to run the game.


![Image](https://dotsunlocks.com/bctoolgithub.png)


## FAQ

#### What assets do I need?

Use the link within the tool to download the assets. They vary slightly from the asset pack on the discord.

#### I have the game already installed, can I use this tool to install new assets?

Yes. Clear your app data from your phones settings then deploy assets using the tool. This wont affect your custom songs or user file.

#### Is this safe?

I made this tool completely open source. Feel free to browse everything and/or compile it yourself to check it out. It's as safe as can be, HOWEVER, I cannot be held responsible for data loss/corrupt files.

Windows SmartScreen will prompt you to confirm you are happy to run this app. This is due to the application being unsigned (this is a hobby project).

#### What commands does this program run?

This program runs the following ADB commands in order. 

You can run these commands manually from within the toolkit folder if you copy the apk, user file, streamedimages folder, unitycache folder and streamableemojis folder into the main tool directory.
Your directory should look like this if you plan on doing this manually for any reason!

<img width="381" height="177" alt="{1AF80617-016E-48DA-AD51-4E959617A83D}" src="https://github.com/user-attachments/assets/6ae697f6-bfea-416d-b4e0-e1573fddba5c" />


adb.exe install beatstar.apk

adb.exe shell cmd appops set --uid com.spaceapegames.beatstar MANAGE_EXTERNAL_STORAGE allow

adb.exe push -a -p streamableemojis /sdcard/Android/data/com.spaceapegames.beatstar/files

adb.exe push -a -p streamedimages /sdcard/Android/data/com.spaceapegames.beatstar/files

adb.exe push -a -p UnityCache /sdcard/unitycache

adb.exe shell mv /sdcard/unitycache/unitycache /sdcard/Android/data/com.spaceapegames.beatstar/files

adb.exe push -a -p "user" /sdcard/Beatstar


## Acknowledgements

 - [Beatclone Community Discord](https://discord.gg/9JPXFajuR2)

