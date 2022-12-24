# OpenHellion Client
![Image of the Hellion Project open in the Unity game engine.](https://user-images.githubusercontent.com/37084190/196989422-4079d0fe-c16a-416b-80f7-27df3077c366.png)

This is the client for the Hellion Revived game. It is the main part of the game, with the server always accompanying the client in every release.
In the basegame, the server is located in the HELLION_SP folder, and contains data such as world saves.
A [separate repository](https://github.com/Hellion-Revived/Hellion_SP) for the development of the single player server also exists.

## About Hellion
Hellion Revived is a multiplayer game set in space. You're a part of a mission-gone-wrong to colonise the nearest star. The goal is to survive in space, and to do such you need to follow some quests. The game is an open-world sandbox game, and have features that allow you to build and manage your own space stations and ships.

## Info
The game is developed with `Unity 2021.3 LTS` and `Wwise 2021.1 LTS`. The game should be cross-platform, but it has only been tested on Windows.

Any improvements you may have are gladly accepted in the form of a pull request. I probably won't bother reviewing pull requests with thousands of lines of code, so please keep them small. If enough people want to become part of the project, I might create a whole team around the development of this game.

## How to open
To open this project in the editor, you need to download and install the [Wwise Launcher](https://www.audiokinetic.com/download/). Adding the program to the Wwise Launcher is as simple as downloading this project and adding it to Unity Hub, but be careful not to open it. Once you have added the project to Unity Hub, it should be visible under the Unity tab.

Before you modify the project, you should download the Wwise SDK, of which I strongly recommend version 2021.1 with default settings, and maybe add some compiler targets for Linux or Mac if you want.

After this, you may modify the project. Choose the SDK you downloaded, and locate your Unity installation. You can apply the changes directly, since you can just revert with Git if it goes wrong.

Open the project in Wwise, set your platform in the dropdown in the upper-left corner. Afterwards, navigate to SoundBanks in the Project Explorer window, right click `SoundBanks`, and click on `Generate Soundbanks` for current platform.

Finally, open the editor and let it import. This will initialise all of the mandatory files, and will also increase the size of you project dramatically.
You will now have the whole project at your hands.

### Minimum requirements
* 30 GB of disk space
* 6 GB of RAM
* 16 GB of RAM to build

Processor and graphics card shouldn't matter much, but it has to at least run Unity; Settings can be modified to increase performance.

## License
All software written by members of the Hellion-Revived project and affiliates is licenced under GPL-3.0.

All remaining files by members of the Hellion-Revived project and affiliates, not falling under the definition of "software", is licensed under CC BY-SA 4.0.

Resources in the Assets/Plugins and Assets/Wwise folders are licensed under their own terms.
