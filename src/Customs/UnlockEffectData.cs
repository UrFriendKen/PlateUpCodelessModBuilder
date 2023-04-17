using Kitchen;
using KitchenData;
using KitchenLib.Utils;
using ModName.src.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModName.src.Customs
{
    public class UnlockEffectData
    {
        public enum EffectCondition
        {
            Always,
            Day,
            Night
        }

        public bool IsCustomerSpawnEffect => CustomerSpawnBaseMultiplier != 0f || CustomerSpawnPerDayMultiplier != 0f;
        public bool IsEnableGroupEffect => CustomerTypeProbability != 0f || !EnableCustomerTypeName.IsNullOrEmpty();
        public bool IsFranchiseEffect => FranchiseIncreasedBaseDishCount != 0;
        public bool IsGlobalEffect => !GlobalEffectCondition.IsNullOrEmpty() || !GlobalEffectTypeName.IsNullOrEmpty();
        public bool IsParameterEffect => CustomersPerHour != 0f || CustomersPerHourCardEffect != 0f || MinGroupSizeChange != 0 || MaxGroupSizeChange != 0;
        public bool IsShopEffect =>
            !AddStapleApplianceName.IsNullOrEmpty() || ExtraBlueprintDeskSpawns != 0 || ExtraShopBlueprints != 0 || ShopCostDecrease != 0f || RandomiseShopPrices ||
            ExtraRandomStartingBlueprints != 0 || BlueprintRebuyableChance != 0f || BlueprintRefreshChance != 0f || UpgradedShopChance != 0f;
        public bool IsStartBonusEffect => !StartingApplianceName.IsNullOrEmpty() || StartingMoney != 0;
        public bool IsStatusEffect => !RestaurantStatusToApply.IsNullOrEmpty();
        public bool IsThemeAddEffect => !AddsTheme.IsNullOrEmpty();

        public string Name;

        #region CustomerSpawnEffect
        public float CustomerSpawnBaseMultiplier;
        public float CustomerSpawnPerDayMultiplier;
        #endregion

        #region EnableGroupEffect
        public float CustomerTypeProbability;
        public string EnableCustomerTypeName;
        #endregion

        #region FranchiseEffect
        public int FranchiseIncreasedBaseDishCount;
        #endregion

        #region GlobalEffect
        public string GlobalEffectCondition;
        public string GlobalEffectTypeName;
        #endregion

        #region ParameterEffect
        public float CustomersPerHour;
        public float CustomersPerHourCardEffect;
        public int MinGroupSizeChange;
        public int MaxGroupSizeChange;
        #endregion

        #region ShopEffect
        public string AddStapleApplianceName;
        public int ExtraBlueprintDeskSpawns;
        public int ExtraShopBlueprints;
        public float ShopCostDecrease;
        public bool RandomiseShopPrices;
        public int ExtraRandomStartingBlueprints;
        public float BlueprintRebuyableChance;
        public float BlueprintRefreshChance;
        public float UpgradedShopChance;
        #endregion

        #region StartBonusEffect
        public string StartingApplianceName;
        public int StartingMoney;
        #endregion

        #region StatusEffect
        public string RestaurantStatusToApply;
        #endregion

        #region ThemeAddEffect
        public string AddsTheme;
        #endregion

        public IEnumerable<UnlockEffect> GetAllUnlockEffects(in GameData gameData, in ResourceDirectory resourceDirectory)
        {
            HashSet<UnlockEffect> unlockEffectsSet = new HashSet<UnlockEffect>();
            if (GetUnlockEffect(gameData, resourceDirectory, out CustomerSpawnEffect customerSpawnEffect))
                unlockEffectsSet.Add(customerSpawnEffect);
            if (GetUnlockEffect(gameData, resourceDirectory, out EnableGroupEffect enableGroupEffect))
                unlockEffectsSet.Add(enableGroupEffect);
            if (GetUnlockEffect(gameData, resourceDirectory, out FranchiseEffect franchiseEffect))
                unlockEffectsSet.Add(franchiseEffect);
            if (GetUnlockEffect(gameData, resourceDirectory, out GlobalEffect globalEffect))
                unlockEffectsSet.Add(globalEffect);
            if (GetUnlockEffect(gameData, resourceDirectory, out ParameterEffect parameterEffect))
                unlockEffectsSet.Add(parameterEffect);
            if (GetUnlockEffect(gameData, resourceDirectory, out ShopEffect shopEffect))
                unlockEffectsSet.Add(shopEffect);
            if (GetUnlockEffect(gameData, resourceDirectory, out StartBonusEffect startBonusEffect))
                unlockEffectsSet.Add(startBonusEffect);
            if (GetUnlockEffect(gameData, resourceDirectory, out StatusEffect statusEffect))
                unlockEffectsSet.Add(statusEffect);
            if (GetUnlockEffect(gameData, resourceDirectory, out ThemeAddEffect themeAddEffect))
                unlockEffectsSet.Add(themeAddEffect);
            return unlockEffectsSet.AsEnumerable();
        }

        public bool GetUnlockEffect<T>(in GameData gameData, in ResourceDirectory resourceDirectory, out T unlockEffect) where T : UnlockEffect
        {
            UnlockEffect result = null;
            if (typeof(T) == typeof(CustomerSpawnEffect) && IsCustomerSpawnEffect)
                result = GetCustomerSpawnEffect();
            else if (typeof(T) == typeof(EnableGroupEffect) && IsEnableGroupEffect)
                result = GetEnableGroupEffect(gameData);
            else if (typeof(T) == typeof(FranchiseEffect) && IsFranchiseEffect)
                result = GetFranchiseEffect();
            else if (typeof(T) == typeof(GlobalEffect) && IsGlobalEffect)
                result = GetGlobalEffect(resourceDirectory);
            else if (typeof(T) == typeof(ParameterEffect) && IsParameterEffect)
                result = GetParameterEffect();
            else if (typeof(T) == typeof(ShopEffect) && IsShopEffect)
                result = GetShopEffect(gameData);
            else if (typeof(T) == typeof(StartBonusEffect) && IsStartBonusEffect)
                result = GetStartBonusEffect(gameData);
            else if (typeof(T) == typeof(StatusEffect) && IsStatusEffect)
                result = GetStatusEffect();
            else if (typeof(T) == typeof(ThemeAddEffect) && IsThemeAddEffect)
                result = GetThemeAddEffect();
            unlockEffect = result != null? result as T : null;
            return unlockEffect != null;
        }

        private CustomerSpawnEffect GetCustomerSpawnEffect()
        {
            return new CustomerSpawnEffect()
            {
                Base = CustomerSpawnBaseMultiplier,
                PerDay = CustomerSpawnPerDayMultiplier
            };
        }

        private EnableGroupEffect GetEnableGroupEffect(GameData gameData)
        {
            if (EnableCustomerTypeName.IsNullOrEmpty())
                return null;

            if (!GameDataUtils.TryGetExistingGDOByName(gameData, EnableCustomerTypeName, out CustomerType customerType))
            {
                
                Main.LogError($"Failed to find CustomerType with name {EnableCustomerTypeName}");
                return null;
            }

            if (CustomerTypeProbability < 0f || CustomerTypeProbability > 1f)
            {
                Main.LogError("CustomerTypeProbability must be between 0 and 1.0");
                return null;
            }

            return new EnableGroupEffect()
            {
                Probability = CustomerTypeProbability,
                EnableType = customerType
            };
        }

        private FranchiseEffect GetFranchiseEffect()
        {
            if (!ValidationUtils.IsNonNegative(FranchiseIncreasedBaseDishCount, "FranchiseIncreasedBaseDishCount"))
            {
                return null;
            }

            return new FranchiseEffect()
            {
                IncreasedBaseDishCount = FranchiseIncreasedBaseDishCount
            };
        }

        private GlobalEffect GetGlobalEffect(ResourceDirectory resourceDirectory)
        {
            if (GlobalEffectCondition.IsNullOrEmpty())
            {
                Main.LogError("GlobalEffectCondition must not be empty!");
                return null;
            }

            if (GlobalEffectTypeName.IsNullOrEmpty())
            {
                Main.LogError("GlobalEffectTypeName must not be empty!");
                return null;
            }

            if (!resourceDirectory.TryGetEffectType(GlobalEffectTypeName, out IEffectType effectType))
            {
                return null;
            }

            if (!Enum.TryParse(GlobalEffectCondition, ignoreCase: true, out EffectCondition effectConditionEnum))
            {
                Main.LogError("Failed to parse EffectCondition!");
                return null;
            }

            IEffectCondition effectCondition;
            switch (effectConditionEnum)
            {
                case EffectCondition.Always:
                    effectCondition = new CEffectAlways();
                    break;
                case EffectCondition.Day:
                    effectCondition = new CEffectAtNight()
                    {
                        DaytimeOnly = true
                    };
                    break;
                case EffectCondition.Night:
                    effectCondition = new CEffectAtNight()
                    {
                        DaytimeOnly = false
                    };
                    break;
                default:
                    Main.LogError("Unknown EffectCondition!");
                    return null;
            }

            return new GlobalEffect()
            {
                EffectType = effectType,
                EffectCondition = effectCondition
            };
        }

        private ParameterEffect GetParameterEffect()
        {
            return new ParameterEffect()
            {
                Parameters = new KitchenParameters
                {
                    CustomersPerHour = CustomersPerHour,
                    CustomersPerHourReduction = CustomersPerHourCardEffect,
                    MinimumGroupSize = MinGroupSizeChange,
                    MaximumGroupSize = MaxGroupSizeChange
                }
            };
        }

        private ShopEffect GetShopEffect(GameData gameData)
        {
            Appliance addStaple;
            if (AddStapleApplianceName.IsNullOrEmpty())
            {
                addStaple = null;
            }
            else if (!GameDataUtils.TryGetExistingGDOByName(gameData, AddStapleApplianceName, out addStaple))
            {
                return null;
            }
            
            if (ShopCostDecrease > 1f)
            {
                Main.LogError($"ShopCostDecrease cannot be greater than 1.0");
                return null;
            }

            if (!ValidationUtils.IsNonNegative(BlueprintRebuyableChance, "BlueprintRebuyableChance") ||
                !ValidationUtils.IsNonNegative(BlueprintRefreshChance, "BlueprintRefreshChance") ||
                !ValidationUtils.IsNonNegative(UpgradedShopChance, "UpgradedShopChance"))
            {
                return null;
            }

            return new ShopEffect()
            {
                AddStaple = addStaple,
                BlueprintDeskAddition = ExtraBlueprintDeskSpawns,
                ExtraShopBlueprints = ExtraShopBlueprints,
                ShopCostDecrease = ShopCostDecrease,
                RandomiseShopPrices = RandomiseShopPrices,
                ExtraStartingBlueprints = ExtraRandomStartingBlueprints,
                BlueprintRebuyableChance = BlueprintRebuyableChance,
                BlueprintRefreshChance = BlueprintRefreshChance,
                UpgradedShopChance = UpgradedShopChance
            };
        }

        private StartBonusEffect GetStartBonusEffect(GameData gameData)
        {
            Appliance startingAppliance;
            if (StartingApplianceName.IsNullOrEmpty())
            {
                startingAppliance = null;
            }
            else if (!GameDataUtils.TryGetExistingGDOByName(gameData, StartingApplianceName, out startingAppliance))
            {
                Main.LogError($"Failed to find CustomerType with name {EnableCustomerTypeName}");
                startingAppliance = null;
            }

            if (!ValidationUtils.IsNonNegative(StartingMoney, "StartingMoney"))
            {
                return null;
            }

            return new StartBonusEffect()
            {
                Appliance = startingAppliance,
                Money = StartingMoney
            };
        }

        private StatusEffect GetStatusEffect()
        {
            if (!Enum.TryParse(RestaurantStatusToApply, out RestaurantStatus restaurantStatus))
            {
                Main.LogError("Failed to parse RestaurantStatus");
                return null;
            }
            return new StatusEffect()
            {
                Status = restaurantStatus
            };
        }

        private ThemeAddEffect GetThemeAddEffect()
        {
            if (!Enum.TryParse(AddsTheme, out DecorationType decorationType))
            {
                Main.LogError("Failed to parse AddsTheme");
                return null;
            }
            return new ThemeAddEffect()
            {
                AddsTheme = decorationType
            };
        }
    }
}
