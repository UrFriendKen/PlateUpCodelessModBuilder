# CApplianceSpeedModifier

Affects appliance processes

Folder Name: `CApplianceSpeedModifier`

## Properties

### Name (string)

* Used to generate GameDataObject (GDO) ID
* Must NOT overlap with other GDOs defined in your mod

### AffectsAllProcesses (bool)

* Set to `True` to apply speed modifiers for all processes

### AffectedProcess (string)

* Can be left empty if `AffectsAllProcesses` is set to `True`
* Name of process
* Uses internal name of GDO

### ProcessSpeed (float)

* Muliplicative modifier for process speed
* Must be greater than -1.0, where negative values mean reducing speed

### BadProcessSpeed (float)

* Muliplicative modifier for bad process speed
* Applies if appliance process is defined to be bad (Eg. burning food)
* Must be greater than -1.0, where negative values mean reducing speed
