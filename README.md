## TinyClicker

TinyClicker is an automated clicker for the Tiny Tower mobile game. 
The clicker utilizes computer vision to help you play Tiny Tower on LDPlayer Android OS emulator.

![TinyClicker](https://user-images.githubusercontent.com/51026900/140165371-999ee88c-6d1e-44ef-bd62-c5ede942049b.png)

<code>Platform: Windows</code>\
<code>Recommended version of LDPlayer: 4.x</code>

## Features

- 100% autonomous work
- Fully automated tower rebuilding at level 50 and completion of the obligatory tutorial
- TinyClicker will watch all advertisements to maximize coin and bux profits
- Automatic participation in hourly raffle
- Configure once, start and forget about it

## Libraries in use

shimat/opencvsharp  [Apache License 2.0](https://github.com/shimat/opencvsharp/blob/master/LICENSE)\
charlesw/tesseract  [Apache License 2.0](https://github.com/charlesw/tesseract/blob/master/LICENSE.txt)


## Before running

Make sure to run only one instance of LDPlayer. 
LDPlayer window can be covered with other windows, but do not minimize it to tray. 
Player interactions with the game are possible but not recommended.\
Avoid checking the tower on your mobile device while the clicker is running, in-game cloud sync may confuse the most recent version of the game and some progress may be lost.


## Setup

- Prior to launching the clicker, set the LDPlayer resolution setting to Customize, with <code>Width = 333</code>, <code>Height = 592</code>, <code>DPI = 150</code>. It is also recommended to set the "Fixed window size" to "Enable" at the "Other settings" tab in order to prevent accidental change of the resolution. This is important and TinyClicker won't work without the correct resolution.

- In case there is no VIP package, open the file Config.txt, located inside the TinyClicker folder with notepad. Locate the "VipPackage": setting and change it to "false" without quotes. If you have the VIP package leave the setting at true. You should also provide the number of the last built floor (e.g. 14 or 27 etc.) for automatic tower rebuilding and the elevator speed (e.g. 7.25 or 6 etc.). Only use the format provided by examples in brackets.\
The setup is done once unless some of the parameters such as elevator speed or VIP status change. In case the config was edited, close TinyClicker before and restart it after the new config was saved.

- When the setup is done, LDPlayer is launched first, then the Clicker. To start the clicker press the Start button, TinyClicker will find the game if it is on the screen and will start working on its own. 

## For iOS players

If you have a Windows PC it is possible to use TinyTower cloud save inside the Android version of Tiny Tower in LDPlayer, VIP package will be transformed as well. Avoid checking the tower on your mobile device while the clicker is running because in-game cloud sync may confuse the most recent version of the game and you may lose some progress. To avoid this wait at least 30 minutes before launching the game on your iOS device after TinyClicker is stopped.
