# PLATEUP CODELESS MOD BUILDER

The goal of this project is to allow creating and registering standard PlateUp Objects without writing any C# code.

## Requirements

* Subscribe to [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2898033283) in the workshop
* Subscribe to [KitchenLib](https://steamcommunity.com/sharedfiles/filedetails/?id=2898069883) in the workshop
* Subscribe to [CodelessModInterop](https://steamcommunity.com/sharedfiles/filedetails/?id=2962181953) in the workshop

## Getting Started

Download the repo as a zip file. Whenever you want to create a new mod, extract the contents of the zip to a new folder.

In it you will find these folders/files of interest:

* Resources Folder<br>- Contains files for your mod (See 'Resources' section for more infomation)

* References Folder<br>- Contains templates and useful information

* `ModName.csproj`<br>- Rename this to your mod name (without spaces or dashes, and only using letters)<br>- You are NOT required to open or modify this file<br>- When renaming .csproj, if you have previously ran `build.bat`, you must delete the out of date folder in PlateUp install directory (It can be found in the Mods folder, if there is one)

* `build.bat`<br>- Run this to compile your mod

## Resources
This directory contains all the files that make up your mod. Currently, all files are in required to be in '`.json`' format.
> JSON is a human-readable data interchange format. There is a strict syntax you are expected to follow for the data to be correctly read.

The files are to be placed in subfolders with names describing the type of object they are defining.

Here are two equally valid example folder structures:

```text
Resources
    
    --- Materials
        - Material1.json
        - Material2.json
        - Material3.json

    --- Decors
        - Wallpaper1.json
        - Floor1.json
        - Floor2.json

    - metadata.json
```

```text
Resources
    
    --- Materials
        - Material1.json
        - Material2.json

    --- Wallpaper
        --- Decors
            - Wallpaper1.json
        --- Materials
            - Material3.json

    --- Flooring
        --- Decors
            - Floor1.json
            - Floor2.json
    
    - metadata.json
```

You must also modify the details in `metadata.json` to include your author name and mod version. (as well as a mod name override if desired. Otherwise leave empty)

## Defining Objects

For more information about the various types, see the wiki.

Currently supported Game Data Object (GDO) type(s) are:

* Decors

Currently supported non-GDO type(s) are:

* Materials (In the format retrieved from KitchenLib's in-game material exporter)
