# PatienceValues

Modifiers for patience time and some patience modifiers

Folder Name: `PatienceValues`

## General Properties

### Name (string)

* Used to generate GameDataObject (GDO) ID
* Must NOT overlap with other GDOs defined in your mod

## Modifiers

### Patience Time

Multiplicative percentage modifier for patience time. Value must be greater than or equal to 0

* EatingTimeModifier (float)
* ThinkingTimeModifier (float)
* SeatingTimeModifier (float)
* ServiceTimeModifier (float)
* WaitForFoodTimeModifier (float)
* DeliveryTimeModifier (float)

* SkipThinking (bool)
  * Set to `True` to skip thinking phase
* InfinitePatienceInsideIfHasQueue (bool)
  * Set to `True` to prevent table patience from decreasing when there is a queue
* BonusPatienceWhenNearby (bool)
  * Set to `True` to make table patience decrease slower when player is witin from decreasing when there is a queue

### Patience Recovery

Time in seconds recovered when respective item is delivered

* DrinkDeliveredRecovery (float)
* ItemDeliveredRecovery (float)
* FoodDeliveredRecovery (float)
