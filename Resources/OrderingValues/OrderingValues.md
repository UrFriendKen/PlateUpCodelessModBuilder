# OrderingValues

Determining course chances, effects of cards that modify ordering, dish related values and some other miscellaneous ordering related modifiers

Folder Name: `OrderingValues`

## General Properties

### Name (string)

* Used to generate GameDataObject (GDO) ID
* Must NOT overlap with other GDOs defined in your mod

## Effect Properties

### Course Order Chance

Multiplicative percentage modifier for chance of ordering respective course. Value must be greater than or equal to 0

* StarterChanceModifier (float)
* SidesChanceModifier (float)
* DessertChanceModifier (float)

### Card Effects

* ChangeMindModifier (float)
  * Affected base game card: `Personalised Waiting`
  * Additive modifier for chance of customer changing order
  * Will affect modded cards that cause customers to randomly change orders
* RepeatCourseModifier (float)
  * Affected base game card: `All You Can Eat`
  * Additive modifier for chance of customer ordering the same course again
  * May not affect modded cards that introduce a 3rd or 4th order, depending on implementation

### Ordering Effects

* GroupOrdersSame (bool)
  * Setting to `True` will cause all customers in group to order the same dish
* SidesOptional (bool)
  * Setting to `True` makes sides optional

### Money

* CustomerPaysFlatFee (bool)
  * Customers pay only `FlatFeeAmount`, ignoring dish price and `BonusPerDelivery`
* FlatFeeAmount (int)
  * Base coins paid, excluding dishes served
* BonusPerDelivery (int)
  * Additional coins per dish served
* PriceModifier (float)
  * Additive modifier for price of dish

### Customers

* GroupMinimumShare (int)
  * Minimum number of customers that share a single dish
* MessFactor (float)
  * Multiplicative modifier for chance that customer creates mess
  * Value must be greater than or equal to 0
* PreventMess (bool)
  * Set to `True` to prevent customers creating any mess
* SeatWithoutClear (bool)
  * Set to `True` to allow customers to seat at tables even if they are occupied

### Others

* ConsumableReuseChanceModifier (float)
  * Iterative additive modifier for chance that consumables will not be consumed
  * Value between 0 and 1.0
