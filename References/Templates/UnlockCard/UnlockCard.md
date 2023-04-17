# UnlockCard

Refers to non-dish cards that applies effects, customers and group modifiers, as well as some other

Folder Name: `UnlockCard`

## Properties

### Name

* Used to generate GameDataObject (GDO) ID
* Must NOT overlap with other GDOs defined in your mod

### IsUnlockable

* Setting to `True` allows `UnlockCard` to show up

### UnlockGroup (string)

* Affects when the card appears
* Allowed values are:
  * "Generic"
  * "PrimaryTheme"
  * "SecondaryTheme"
  * "Special"

### CardType (string)

* Determines colour of the card
* Allowed values are:
  * "Default"
  * "FranchiseTier"
  * "ThemeUnlock"
  * "HalloweenTreat"
  * "HalloweenTrick"
  * "Setting"

### MinimumFranchiseTier (int)

* Lowest Franchise Tier this `UnlockCard` is allowed to appear in

### IsExactFranchiseTier (bool)

* Setting to `True` causes card to only appear when Franchise Tier matches `MinimumFranchiseTier` exactly

### ExpRewardLevel (string)

* Amount of EXP awarded by card at end of run

|   Value   |  EXP  |
| --------- | :---: |
| "None"    |   0   |
| "Small"   |  50   |
| "Medium"  |  100  |
| "Large"   |  200  |

### CustomerMultiplierLevel (string)

* Customer reduction percentage

|       Value       |  Percent  |
| ----------------- | :-------: |
| "None"            |     0     |
| "SmallIncrease"   |    +15    |
| "LargeIncrease"   |    +30    |
| "SmallDecrease"   |    -15    |
| "LargeDecrease"   |    -30    |
| "FranchiseTier"   |    +30    |

### SelectionBias (float)

* Higher values cause UnlockCard to have a higher chance of appearing
* Value between 0 and 1.0

### RequiredCards (string[])

* Names of cards that are required to be active before this UnlockCard can be selected
* Uses internal name of GDO, not display name

### BlockedByCards (string[])

* Names of cards that, when active, prevent this UnlockCard from appearing
* To use internal name of other GDOs, not display name

### UnlockEffectNames (string[])

* Names of `UnlockEffect` (you have created) that will be applied
* See `UnlockEffect` template

### UnlockInfoNames (string[])

* Names of `Unlock` (you have created) that will be used for each locale
* See `UnlockInfo` template
