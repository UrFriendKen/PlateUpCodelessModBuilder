using CodelessModBuilder.src;
using Kitchen;
using KitchenData;
using ModName.src.Utils;
using System;

namespace ModName.src.Customs
{
    public abstract class EffectTypeData<T> where T : IEffectType
    {
        public string Name;

        public abstract bool TryConvert(in GameData gameData, out T effectType);
    }

    public class CApplianceSpeedModifierEffectData : EffectTypeData<CApplianceSpeedModifier>
    {
        public bool AffectsAllProcesses;
        public string AffectedProcess;
        public float ProcessSpeed;
        public float BadProcessSpeed;

        public override bool TryConvert(in GameData gameData, out CApplianceSpeedModifier effectType)
        {
            effectType = default;
            if (!ValidationUtils.IsNonNegative(ProcessSpeed, "ProcessSpeed") ||
                !ValidationUtils.IsNonNegative(BadProcessSpeed, "BadProcessSpeed"))
            {
                return false;
            }
            if (!GameDataUtils.TryGetExistingGDOByName(gameData, AffectedProcess, out Process process))
            {
                Main.LogError($"Failed to find Process with name {AffectedProcess}");
                return false;
            }

            effectType = new CApplianceSpeedModifier()
            {
                AffectsAllProcesses = AffectsAllProcesses,
                Process = process.ID,
                Speed = ProcessSpeed,
                BadSpeed = BadProcessSpeed
            };
            return true;
        }
    }

    public class CAppliesStatusEffectData : EffectTypeData<CAppliesStatus>
    {
        public string BonusStatus;

        public override bool TryConvert(in GameData gameData, out CAppliesStatus effectType)
        {
            if (!Enum.TryParse(BonusStatus, out DecorationBonus decorationBonus))
            {
                Main.LogError("Failed to parse BonusStatus");
                effectType = default;
                return false;
            }
            effectType = new CAppliesStatus()
            {
                Bonus = decorationBonus
            };
            return true;
        }
    }

    public class CTableModifierEffectData : EffectTypeData<CTableModifier>
    {
        public float EatingTimeModifier;
        public float ThinkingTimeModifier;
        public float SeatingTimeModifier;
        public float ServiceTimeModifier;
        public float WaitForFoodTimeModifier;
        public float DeliveryTimeModifier;
        public float DrinkDeliveredRecovery;
        public float ItemDeliveredRecovery;
        public float FoodDeliveredRecovery;
        public bool SkipThinking;
        public bool InfinitePatienceInsideIfHasQueue;
        public bool DestroyTableWhenLeave;
        public bool BonusPatienceWhenNearby;
        public bool ResetPatienceOption; // Unknown
        public bool ProvidesQueuePatienceBoost; // Unknown

        public float StarterChanceModifier;
        public float SidesChanceModifier;
        public float DessertChanceModifier;
        public float ChangeMindModifier;
        public float RepeatCourseModifier;
        public bool GroupOrdersSame;
        public bool SidesOptional;
        public bool CustomerPaysFlatFee;
        public int BonusPerDelivery;
        public float ConsumableReuseChanceModifier;
        public float MessFactor;
        public bool PreventMess;
        public int GroupMinimumShare;
        public float PriceModifier;
        public int FlatFeeAmount;
        public bool SeatWithoutClear;

        public override bool TryConvert(in GameData gameData, out CTableModifier effectType)
        {
            if (!ValidationUtils.IsNonNegative(EatingTimeModifier, "EatingTimeModifier") ||
                !ValidationUtils.IsNonNegative(ThinkingTimeModifier, "ThinkingTimeModifier") ||
                !ValidationUtils.IsNonNegative(SeatingTimeModifier, "SeatingTimeModifier") ||
                !ValidationUtils.IsNonNegative(ServiceTimeModifier, "ServiceTimeModifier") ||
                !ValidationUtils.IsNonNegative(WaitForFoodTimeModifier, "WaitForFoodTimeModifier") ||
                !ValidationUtils.IsNonNegative(DeliveryTimeModifier, "DeliveryTimeModifier") ||
                !ValidationUtils.IsNonNegative(StarterChanceModifier, "StarterChanceModifier") ||
                !ValidationUtils.IsNonNegative(SidesChanceModifier, "SidesChanceModifier") ||
                !ValidationUtils.IsNonNegative(DessertChanceModifier, "DessertChanceModifier"))
            {
                effectType = default;
                return false;
            }

            effectType = new CTableModifier()
            {
                PatienceModifiers = new PatienceValues()
                {
                    Eating = EatingTimeModifier,
                    Thinking = ThinkingTimeModifier,
                    Seating = SeatingTimeModifier,
                    Service = ServiceTimeModifier,
                    WaitForFood = WaitForFoodTimeModifier,
                    GetFoodDelivered = DeliveryTimeModifier,
                    DrinkDeliverBonus = DrinkDeliveredRecovery,
                    ItemDeliverBonus = ItemDeliveredRecovery,
                    FoodDeliverBonus = FoodDeliveredRecovery,
                    SkipWaitPhase = SkipThinking,
                    InfinitePatienceIfQueue = InfinitePatienceInsideIfHasQueue,
                    DestroyTableIfLeave = DestroyTableWhenLeave,
                    BonusPatienceWhenNearby = BonusPatienceWhenNearby,
                    ResetPatienceOption = ResetPatienceOption,
                    ProvidesQueuePatienceBoost = ProvidesQueuePatienceBoost
                },

                OrderingModifiers = new OrderingValues()
                {
                    StarterModifier = StarterChanceModifier,
                    SidesModifier = SidesChanceModifier,
                    DessertModifier = DessertChanceModifier,
                    ChangeMindModifier = ChangeMindModifier,
                    RepeatCourseModifier = RepeatCourseModifier,
                    GroupOrdersSame = GroupOrdersSame,
                    SidesOptional = SidesOptional,
                    IsOnlyFlatFee = CustomerPaysFlatFee,
                    BonusPerDelivery = BonusPerDelivery,
                    ConsumableReuseChance = ConsumableReuseChanceModifier,
                    MessFactor = MessFactor,
                    PreventMess = PreventMess,
                    MinimumShare = GroupMinimumShare,
                    PriceModifier = PriceModifier,
                    FlatFee = FlatFeeAmount,
                    SeatWithoutClear = SeatWithoutClear
                }
            };
            return true;

        }
    }

    public class CQueueModifierEffectData : EffectTypeData<CQueueModifier>
    {
        public float QueueFactor;

        public override bool TryConvert(in GameData gameData, out CQueueModifier effectType)
        {
            if (!ValidationUtils.IsNonNegative(QueueFactor, "QueueFactor"))
            {
                effectType = default;
                return false;
            }
            effectType = new CQueueModifier()
            {
                PatienceFactor = QueueFactor
            };
            return true;
        }
    }
}
