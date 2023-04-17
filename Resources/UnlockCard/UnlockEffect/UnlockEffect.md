# UnlockEffect

Effects that the unlock card will apply when the card is active.

Folder Name: `UnlockEffect`

## General Properties

### Name (string)

* Used to generate GameDataObject (GDO) ID
* Must NOT overlap with other GDOs defined in your mod

## Effect Properties

### CustomerSpawnEffect

Affects number of expected customers/groups by changing the day scaling curve

* CustomerSpawnBaseMultiplier (float)
  * Additive percentage modifier for number of customers on first day

* CustomerSpawnPerDayMultiplier (float)
  * Additive percentage modifier for number of customers scaling per day

### EnableGroupEffect

Adds customer types that visit the restaurant

* CustomerTypeProbability (float)
  * Chance of customer type (Currently unused in game - Default: 0.25)
  * Value between 0 and 1.0
* EnableCustomerTypeName (string)
  * Name of customer type
  * Uses internal name of GDO, not display name

### FranchiseEffect

Allows choosing additional base dishes when starting runs with future franchise tiers (Variety effect)

* FranchiseIncreasedBaseDishCount (int)
  * Number of additional base dishes to allow
  * Value greater than or equal to 0

### GlobalEffect

Applies a variety of more complicated effects

* GlobalEffectCondition (string)
  * "Always"
  * "Day"
  * "Night"
* GlobalEffectTypeName (string)
  * Parameters for effect to be applied
  * You must define these objects separately (See `GlobalEffectTypes` folder)
  * Supported EffectType include:
    * CApplianceSpeedModifier
    * CAppliesEffect
    * CTableModifier
    * CQueueModifier

### ParameterEffect

Affects number of expected customers/groups by changing effectiveness of card customer reduction, and group size

* CustomersPerHour (float)
  * Additive percentage modifier for base number of customers without cards
* CustomersPerHourCardEffect (float)
  * Additive exponential percentage modifier for card customer reduction
* MinGroupSizeChange (int)
  * Number of customers added to minimum group size
* MaxGroupSizeChange (int)
  * Number of customers added to maximum group size

### ShopEffect

Modifies shop costs, appliance chance and other blueprint related mechanics

* AddStapleApplianceName (string)
  * Name of appliance
  * Uses internal name of GDO, not display name

* ExtraBlueprintDeskSpawns (int)
  * Additional copies of blueprints that are locked in for each blueprint desk

* ExtraShopBlueprints (int)
  * Number of blueprints that spawn in the shop at the start of each day

* ShopCostDecrease (float)
  * Global percentage discount of blueprint cost
  * Value less that or equal to 1.0
  
* RandomiseShopPrices (bool)
  * Setting to `True` assigns random cost to blueprints in shop

* ExtraRandomStartingBlueprints (int)
  * Number of free appliances to provide at the start of future franchise tiers

* BlueprintRebuyableChance (float)
  * Iterative additive percentage modifier for appliance blueprint not to be consumed when purchased
  * Value greater than or equal to 0

* BlueprintRefreshChance (float)
  * Iterative additive percentage modifier for random appliance blueprint to spawn when one is purchased
  * Value greater than or equal to 0

* UpgradedShopChance (float)
  * Iterative additive percentage modifier for blueprints to spawn as upgraded blueprints in the shop
  * Value greater than or equal to 0

### StartBonusEffect

Provide free appliance and/or starting money at the start of future franchise tiers

* StartingApplianceName (string)
  * Name of appliance
  * Uses internal name of GDO, not display name
  
* StartingMoney (int)
  * Number of coins
  * Value greater than 0

### StatusEffect

Applies `RestaurantStatus`

* RestaurantStatusToApply
  * See `RestaurantStatusValues.txt`

### ThemeAddEffect

Adds decoration theme

* AddsTheme (string)
  * "Exclusive"
  * "Affordable"
  * "Charming"
  * "Formal"
  * "Kitchen"
