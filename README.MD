# PLATEUP CODELESS MOD BUILDER

The goal of this project is to allow creating and registering standard PlateUp Objects without writing any C# code.

## Requirements

* Subscribe to [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2898033283) in the workshop
* Subscribe to [KitchenLib](https://steamcommunity.com/sharedfiles/filedetails/?id=2898069883) in the workshop
* Subscribe to [CodelessModInterop](https://steamcommunity.com/sharedfiles/filedetails/?id=2962181953) in the workshop

## Getting Started

Download the latest source code zip file from [releases](https://github.com/UrFriendKen/PlateUpCodelessModBuilder/releases). Whenever you want to create a new mod, extract the contents of the zip to a new folder.

In it you will find these folders/files of interest:

* Resources Folder
  * Contains files for your mod (See 'Resources' section for more infomation)
  * Starts with templates you can use (Remove any files you do not need before distributing your mod on the workshop)

* `ModName.csproj`
  * Rename this to your mod name (without spaces or dashes, and only using letters)<br>- You are NOT required to open or modify this file<br>- When renaming .csproj, if you have previously ran `build.bat`, you must delete the out of date folder in PlateUp install directory (It can be found in the Mods folder, if there is one)

* `build.bat`
  * Run this to compile your mod

## Resources

This directory contains all the files that make up your mod. Currently, all files are in required to be in '`.json`' format.
> JSON is a human-readable data interchange format. There is a strict syntax you are expected to follow for the data to be correctly read.

The files are to be placed in subfolders with names describing the type of object they are defining.

Here are two equally valid example folder structures:

```text
Resources
    
    --- KLMaterial
        - Material1.json
        - Material2.json
        - Material3.json

    --- Decor
        - Wallpaper1.json
        - Floor1.json
        - Floor2.json

    - metadata.json
```

```text
Resources
    
    --- KLMaterial
        - Material1.json
        - Material2.json

    --- Wallpaper
        --- Decor
            - Wallpaper1.json
        --- KLMaterial
            - Material3.json

    --- Flooring
        --- Decor
            - Floor1.json
            - Floor2.json
    
    - metadata.json
```

You must also modify the details in `metadata.json` to include your author name and mod version. (as well as a mod name override if desired. Otherwise leave empty)

## Defining Objects

For more information about the various types, see the [wiki](https://github.com/UrFriendKen/PlateUpCodelessModBuilder/wiki).

Currently supported Game Data Object (GDO) types are:

* [Decor](https://github.com/UrFriendKen/PlateUpCodelessModBuilder/wiki/Decor)
* [UnlockCard](https://github.com/UrFriendKen/PlateUpCodelessModBuilder/wiki/UnlockCard)

Currently supported non-GDO types are:

* KLMaterial (Materials generated from KitchenLib's in-game material exporter)
* [UnlockInfo](https://github.com/UrFriendKen/PlateUpCodelessModBuilder/wiki/UnlockInfo)
* [UnlockEffect](https://github.com/UrFriendKen/PlateUpCodelessModBuilder/wiki/UnlockEffect)
* [CApplianceSpeedModifier](https://github.com/UrFriendKen/PlateUpCodelessModBuilder/wiki/CApplianceSpeedModifier)
* [CAppliesStatus](https://github.com/UrFriendKen/PlateUpCodelessModBuilder/wiki/CAppliesStatus)
* [CQueueModifier](https://github.com/UrFriendKen/PlateUpCodelessModBuilder/wiki/CQueueModifier)
* [CTableModifier](https://github.com/UrFriendKen/PlateUpCodelessModBuilder/wiki/CTableModifier)
