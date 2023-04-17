using KitchenData;
using ModName.src.Utils;
using System;
using System.Collections.Generic;

namespace ModName.src.Customs
{
    public abstract class BaseStructData
    {
        public string Name;

        public abstract Type StructType { get; }
    }

    public abstract class StructData<T> : BaseStructData where T : struct
    {
        public sealed override Type StructType => typeof(T);

        public abstract bool TryConvert(out T structData);
    }

    public class StructDataDictionary
    {
        private readonly Dictionary<string, BaseStructData> _dictionary = new Dictionary<string, BaseStructData>();
        
        public BaseStructData this[string key]
        {
            get
            {
                return _dictionary[key];
            }
            set
            {
                _dictionary[key] = value;
            }
        }

        public bool ContainsKey(string key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Add<T>(string key, T value) where T : BaseStructData
        {
            _dictionary.Add(key, value);
        }

        public bool TryGetValue(string key, out BaseStructData baseStructData)
        {
            return _dictionary.TryGetValue(key, out baseStructData);
        }

        public bool TryGetValue<T>(string key, out T baseStructData) where T : BaseStructData
        {
            if (!TryGetValue(key, out BaseStructData value))
            {
                Main.LogError($"Failed to get {typeof(T).Name} - {key}.");
                baseStructData = null;
                return false;
            }
            baseStructData = value as T;
            return true;
        }

        public bool TryGetConvertedValue<T>(string key, out T convertedStruct) where T : struct
        {
            convertedStruct = default;
            if (!TryGetValue(key, out BaseStructData baseStructData))
                return false;
            if (baseStructData.StructType != typeof(T))
            {
                Main.LogError($"{key} is not of type {typeof(T).Name}.");
                return false;
            }

            StructData<T> structData = baseStructData as StructData<T>;

            if (!structData.TryConvert(out convertedStruct))
                return false;

            return true;
        }
    }

    public class PatienceValuesData : StructData<PatienceValues>
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

        public override bool TryConvert(out PatienceValues patienceValues)
        {
            if (!ValidationUtils.IsNonNegative(EatingTimeModifier, "EatingTimeModifier") ||
                !ValidationUtils.IsNonNegative(ThinkingTimeModifier, "ThinkingTimeModifier") ||
                !ValidationUtils.IsNonNegative(SeatingTimeModifier, "SeatingTimeModifier") ||
                !ValidationUtils.IsNonNegative(ServiceTimeModifier, "ServiceTimeModifier") ||
                !ValidationUtils.IsNonNegative(WaitForFoodTimeModifier, "WaitForFoodTimeModifier") ||
                !ValidationUtils.IsNonNegative(DeliveryTimeModifier, "DeliveryTimeModifier"))
            {
                patienceValues = default;
                return false;
            }

            patienceValues = new PatienceValues()
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
            };
            return true;
        }
    }

    public class OrderingValuesData : StructData<OrderingValues>
    {
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

        public override bool TryConvert(out OrderingValues orderingValues)
        {
            orderingValues = default;
            if (!ValidationUtils.IsNonNegative(StarterChanceModifier, "StarterChanceModifier") ||
                !ValidationUtils.IsNonNegative(SidesChanceModifier, "SidesChanceModifier") ||
                !ValidationUtils.IsNonNegative(DessertChanceModifier, "DessertChanceModifier"))
            {
                return false;
            };

            if (ConsumableReuseChanceModifier < 0f || ConsumableReuseChanceModifier > 1f)
            {
                Main.LogError("ConsumableReuseChanceModifier must be between 0 and 1.0");
                return false;
            }

            orderingValues = new OrderingValues()
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
            };
            return true;
        }
    }
}
