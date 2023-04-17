using CodelessModInterop;
using KitchenData;
using KitchenLib.References;
using KitchenLib.Utils;
using ModName.src.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace ModName.src.Customs
{
    public abstract class GDOData<T> where T : GameDataObject
    {
        public string Name;

        public int GenerateID(string modID)
        {
            return StringUtils.GetInt32HashCode($"{modID}:{Name}");
        }

        public bool TryConvert(in GameData gameData, in ResourceDirectory resourceDirectory, out T gdo)
        {
            if (Name.ToLower().StartsWith("cmb_"))
                Main.LogWarning("Please rename your resources so it is uniquely identifiable to your mod to prevent conflicts!");
            return OnTryConvert(gameData, resourceDirectory, out gdo);
        }

        protected abstract bool OnTryConvert(in GameData gameData, in ResourceDirectory resourceDirectory, out T gdo);
    }

    public class DecorData : GDOData<Decor>
    {
        public bool UseCustomMaterial;
        public string MaterialName;
        public string DecorType;
        public bool IsAvailable = true;

        protected override bool OnTryConvert(in GameData gameData, in ResourceDirectory resourceDirectory, out Decor decor)
        {
            Material material = null;
            decor = null;
            if (UseCustomMaterial && !ModdedResourceRegistry.TryGetModdedMaterial(MaterialName, out material))
            {
                Main.LogWarning($"Failed to get Custom Material - {MaterialName}.");
                return false;
            }

            if (!UseCustomMaterial)
            {
                material = MaterialUtils.GetExistingMaterial(MaterialName);
                if (material == null)
                {
                    Main.LogWarning($"Failed to get Material - {MaterialName}.");
                    return false;
                }
            }

            if (!ValidationUtils.EnumTryParse(DecorType, out LayoutMaterialType layoutMaterialType, warnIfFail: false))
            {
                Main.LogWarning("Failed to parse DecorType.");
                return false;
            }

            int applicatorApplianceId;
            switch (layoutMaterialType)
            {
                case LayoutMaterialType.Wallpaper:
                    applicatorApplianceId = ApplianceReferences.WallpaperApplicator;
                    break;
                case LayoutMaterialType.Floor:
                    applicatorApplianceId = ApplianceReferences.FlooringApplicator;
                    break;
                default:
                    Main.LogWarning($"Invalid DecorType - {DecorType}.");
                    return false;
            }

            if (!GameDataUtils.TryGetExistingGDOByID(gameData, applicatorApplianceId, out Appliance applicatorAppliance, warnIfFail: false))
            {
                Main.LogWarning($"Failed to get applicator appliance.");
                return false;
            }

            string decorName = $"{DecorType} - {Name}";
            int id = GenerateID(resourceDirectory.ModGuid);

            decor = ScriptableObject.CreateInstance<Decor>();
            decor.name = decorName;
            decor.ID = id;
            decor.Material = material;
            decor.Type = layoutMaterialType;
            decor.ApplicatorAppliance = applicatorAppliance;
            decor.IsAvailable = IsAvailable;

            return true;
        }
    }

    public class UnlockCardData : GDOData<UnlockCard>
    {
        public bool IsUnlockable;
        public string UnlockGroup;
        public string CardType;
        public int MinimumFranchiseTier;
        public bool IsExactFranchiseTier;
        public string ExpRewardLevel;
        public string CustomerMultiplierLevel;
        public float SelectionBias;
        public List<string> RequiredCards;
        public List<string> BlockedByCards;
        public List<string> UnlockEffectNames;
        public List<string> UnlockInfoNames;

        protected override bool OnTryConvert(in GameData gameData, in ResourceDirectory resourceDirectory, out UnlockCard unlockCard)
        {
            unlockCard = null;
            if (!ValidationUtils.EnumTryParse(UnlockGroup, out UnlockGroup unlockGroup))
                return false;
            if (!ValidationUtils.EnumTryParse(CardType, out CardType cardType))
                return false;
            if (!ValidationUtils.EnumTryParse(ExpRewardLevel, out Unlock.RewardLevel expReward))
                return false;
            if (!ValidationUtils.EnumTryParse(CustomerMultiplierLevel, out DishCustomerChange customerChange))
                return false;
            if (SelectionBias < 0f || SelectionBias > 1f)
            {
                Main.LogError("SelectionBias must be between 0 and 1.0");
                return false;
            }

            LocalisationObject<UnlockInfo> info = new LocalisationObject<UnlockInfo>();
            foreach (string localisationName in UnlockInfoNames)
            {
                if (!resourceDirectory.TryGetLocalisationItem(localisationName, out UnlockInfo unlockInfo))
                {
                    continue;
                }
                info.Add(unlockInfo.Locale, unlockInfo);
            }

            int id = GenerateID(resourceDirectory.ModGuid);

            unlockCard = ScriptableObject.CreateInstance<UnlockCard>();
            unlockCard.ID = id;
            unlockCard.name = Name;
            unlockCard.ExpReward = expReward;
            unlockCard.IsUnlockable = IsUnlockable;
            unlockCard.UnlockGroup = unlockGroup;
            unlockCard.CardType = cardType;
            unlockCard.MinimumFranchiseTier = MinimumFranchiseTier;
            unlockCard.IsSpecificFranchiseTier = IsExactFranchiseTier;
            unlockCard.CustomerMultiplier = customerChange;
            unlockCard.SelectionBias = SelectionBias;
            unlockCard.Info = info;
            unlockCard.Localisation = info.Get(Localisation.CurrentLocale);
            unlockCard.Effects = new List<UnlockEffect>();
            unlockCard.Requires = new List<Unlock>();
            unlockCard.BlockedBy = new List<Unlock>();

            return true;
        }
    }
}
