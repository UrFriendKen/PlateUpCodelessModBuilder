using CodelessModBuilder.src;
using KitchenData;
using KitchenLib.Utils;
using ModName.src.Utils;
using System.Collections.Generic;
using System.Linq;

namespace ModName.src.Customs
{
    public class UnlockEffectData
    {
        public bool IsCustomerSpawnEffect => CustomerSpawnBaseMultiplier != 0f || CustomerSpawnPerDayMultiplier != 0f;
        public bool IsEnableGroupEffect => CustomerTypeProbability != 0f || !EnableCustomerTypeName.IsNullOrEmpty();
        public bool IsFranchiseEffect => FranchiseIncreasedBaseDishCount != 0;
        public bool IsGlobalEffect => false;
        public bool IsParameterEffect => CustomersPerHour != 0f || CustomersPerHourIncrease != 0f || MinGroupSizeChange != 0 || MaxGroupSizeChange != 0;
        public bool IsShopEffect =>
            !AddStapleApplianceName.IsNullOrEmpty() || ExtraBlueprintDeskSpawns != 0 || ExtraShopBlueprints != 0 || ShopCostDecrease != 0f || RandomiseShopPrices ||
            ExtraRandomStartingBlueprints != 0 || BlueprintRebuyableChance != 0f || BlueprintRefreshChance != 0f || UpgradedShopChance != 0f;
        public bool IsStartBonusEffect => !StartingApplianceName.IsNullOrEmpty() || StartingMoney != 0;
        public bool IsStatusEffect => false;
        public bool IsThemeAddEffect => false;

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
        #endregion

        #region ParameterEffect
        public float CustomersPerHour;
        public float CustomersPerHourIncrease;
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
        #endregion

        #region ThemeAddEffect
        #endregion

        public IEnumerable<UnlockEffect> GetAllUnlockEffects(in GameData gameData)
        {
            HashSet<UnlockEffect> unlockEffectsSet = new HashSet<UnlockEffect>();
            if (GetUnlockEffect(gameData, out CustomerSpawnEffect customerSpawnEffect))
                unlockEffectsSet.Add(customerSpawnEffect);
            if (GetUnlockEffect(gameData, out EnableGroupEffect enableGroupEffect))
                unlockEffectsSet.Add(enableGroupEffect);
            if (GetUnlockEffect(gameData, out FranchiseEffect franchiseEffect))
                unlockEffectsSet.Add(franchiseEffect);
            if (GetUnlockEffect(gameData, out GlobalEffect globalEffect))
                unlockEffectsSet.Add(globalEffect);
            if (GetUnlockEffect(gameData, out ParameterEffect parameterEffect))
                unlockEffectsSet.Add(parameterEffect);
            if (GetUnlockEffect(gameData, out ShopEffect shopEffect))
                unlockEffectsSet.Add(shopEffect);
            if (GetUnlockEffect(gameData, out StartBonusEffect startBonusEffect))
                unlockEffectsSet.Add(startBonusEffect);
            if (GetUnlockEffect(gameData, out StatusEffect statusEffect))
                unlockEffectsSet.Add(statusEffect);
            if (GetUnlockEffect(gameData, out ThemeAddEffect themeAddEffect))
                unlockEffectsSet.Add(themeAddEffect);
            return unlockEffectsSet.AsEnumerable();
        }

        public bool GetUnlockEffect<T>(in GameData gameData, out T unlockEffect) where T : UnlockEffect
        {
            UnlockEffect result = null;
            if (typeof(T) == typeof(CustomerSpawnEffect) && IsCustomerSpawnEffect)
                result = GetCustomerSpawnEffect();
            else if (typeof(T) == typeof(EnableGroupEffect) && IsEnableGroupEffect)
                result = GetEnableGroupEffect(gameData);
            else if (typeof(T) == typeof(FranchiseEffect) && IsFranchiseEffect)
                result = GetFranchiseEffect();
            else if (typeof(T) == typeof(GlobalEffect) && IsGlobalEffect)
                result = GetGlobalEffect();
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

            if (!gameData.TryGetExistingGDOByName(EnableCustomerTypeName, out CustomerType customerType))
            {
                
                Main.LogError($"Failed to find CustomerType with name {EnableCustomerTypeName}");
                return null;
            }

            if (CustomerTypeProbability < 0f)
            {
                Main.LogError("CustomerTypeProbability must not be negative!");
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
            if (FranchiseIncreasedBaseDishCount < 0)
            {
                Main.LogError("FranchiseBaseDishCount must not be negative!");
                return null;
            }

            return new FranchiseEffect()
            {
                IncreasedBaseDishCount = FranchiseIncreasedBaseDishCount
            };
        }

        private GlobalEffect GetGlobalEffect()
        {
            return null;
        }

        private ParameterEffect GetParameterEffect()
        {
            return new ParameterEffect()
            {
                Parameters = new KitchenParameters
                {
                    CustomersPerHour = CustomersPerHour,
                    CustomersPerHourReduction = -CustomersPerHourIncrease,
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
            else if (!gameData.TryGetExistingGDOByName(AddStapleApplianceName, out addStaple))
            {
                Main.LogError($"Failed to find CustomerType with name {EnableCustomerTypeName}");
                addStaple = null;
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
            else if (!gameData.TryGetExistingGDOByName(StartingApplianceName, out startingAppliance))
            {
                Main.LogError($"Failed to find CustomerType with name {EnableCustomerTypeName}");
                startingAppliance = null;
            }

            if (StartingMoney < 0)
            {
                Main.LogError("StartingMoney must not be negative!");
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
            return null;
        }

        private ThemeAddEffect GetThemeAddEffect()
        {
            return null;
        }
    }
}
