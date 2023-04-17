using Kitchen;
using KitchenData;
using KitchenLib.Utils;
using ModName.src.Utils;
using System;

namespace ModName.src.Customs
{
    public abstract class EffectTypeData<T> where T : IEffectType
    {
        public string Name;

        public abstract bool TryConvert(in GameData gameData, in ResourceDirectory resourceDirectory, out T effectType);
    }

    public class CApplianceSpeedModifierEffectData : EffectTypeData<CApplianceSpeedModifier>
    {
        public bool AffectsAllProcesses;
        public string AffectedProcess;
        public float ProcessSpeed;
        public float BadProcessSpeed;

        public override bool TryConvert(in GameData gameData, in ResourceDirectory resourceDirectory, out CApplianceSpeedModifier effectType)
        {
            effectType = default;

            int processId = 0;
            if (!AffectedProcess.IsNullOrEmpty())
            {
                if (!GameDataUtils.TryGetExistingGDOByName(gameData, AffectedProcess, out Process process))
                {
                    Main.LogError($"Failed to find Process with name {AffectedProcess}");
                    return false;
                }
                processId = process.ID;
            }
            else if (!AffectsAllProcesses)
            {
                Main.LogError($"AffectedProcess cannot be empty, unless AffectsAllProcesses is set to True.");
                return false;
            }

            if (ProcessSpeed <= -1f)
            {
                Main.LogError("ProcessSpeed must be greater than -1.0");
                return false;
            }

            if (BadProcessSpeed <= -1f)
            {
                Main.LogError("ProcessSpeed must be greater than -1.0");
                return false;
            }

            effectType = new CApplianceSpeedModifier()
            {
                AffectsAllProcesses = AffectsAllProcesses,
                Process = processId,
                Speed = ProcessSpeed,
                BadSpeed = BadProcessSpeed
            };
            return true;
        }
    }

    public class CAppliesStatusEffectData : EffectTypeData<CAppliesStatus>
    {
        public string BonusStatus;

        public override bool TryConvert(in GameData gameData, in ResourceDirectory resourceDirectory, out CAppliesStatus effectType)
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
        public string PatienceValuesName;
        public string OrderingValuesName;

        public override bool TryConvert(in GameData gameData, in ResourceDirectory resourceDirectory, out CTableModifier effectType)
        {
            effectType = default;

            if (PatienceValuesName.IsNullOrEmpty())
            {
                Main.LogError("PatienceValuesName cannot be empty.");
                return false;
            }
            if (OrderingValuesName.IsNullOrEmpty())
            {
                Main.LogError("OrderingValuesName cannot be empty.");
                return false;
            }

            if (!resourceDirectory.TryGetConvertedStruct(PatienceValuesName, out PatienceValues patienceValues) ||
                !resourceDirectory.TryGetConvertedStruct(OrderingValuesName, out OrderingValues orderingValues))
            {
                return false;
            }

            effectType = new CTableModifier()
            {
                PatienceModifiers = patienceValues,
                OrderingModifiers = orderingValues
            };
            return true;

        }
    }

    public class CQueueModifierEffectData : EffectTypeData<CQueueModifier>
    {
        public float QueueFactor;

        public override bool TryConvert(in GameData gameData, in ResourceDirectory resourceDirectory, out CQueueModifier effectType)
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
