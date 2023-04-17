# PlateUpCodelessModBuilder

## v0.1.1

* Updated to Yariazen.PlateUp.ModBuildUtilities v1.8.3
* Changed current Material to KLMaterial
* Changed KLMaterial to be referenced by given name instead of filename
* Fixed issue when trying to add the same material multiple times causing exception to be thrown
* Added PatienceValues
* Added OrderingValues
* Added UnlockEffect (Including EffectTypes to support GlobalEffect)
* Added UnlockInfo
* Added UnlockCard
* Moved most object caching to CodelessModInterop (Allows for other mods to access converted resources)
* Various bug fixes
