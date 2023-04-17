# Decor

Includes wallpapers and flooring you get during decoration days. This is not the same as the theme appliances you can purchase.

Folder Name: `Decor`

## Properties

### Name (string)

* Used to generate GameDataObject (GDO) ID
* Must NOT overlap with other GDOs defined in your mod

### UseCustomMaterial (bool)

Set to `True` if using a custom material (Material must be defined either in this mod, or some other mod that registers Material with CodelessModInterop)

* Otherwise `False`

### MaterialName (string)

* Can be a custom material name (`UseCustomMaterial` must be set to `True`)

* Can also use base game material name

* See `References\BaseGameMaterials.md` for available base game materials (May not be up to date)

### DecorType (string)

* Allowed values are: "Wallpaper", "Floor"

### IsAvailable (bool)

* Setting to `True` allows decor to show up during decoration days
