using ModName.src;
using KitchenData;
using System;
using UnityEngine;

namespace ModName.src.Customs
{
    public abstract class BaseLocalisationData
    {
        public string Name;
        public string Locale;
    }

    public abstract class LocalisationData<T> : BaseLocalisationData where T : Localisation
    {
        public abstract bool TryConvert(out T localisation);
    }

    public class UnlockInfoData : LocalisationData<UnlockInfo>
    {
        public string FlavourText;
        public string Description;

        public override bool TryConvert(out UnlockInfo unlockInfo)
        {
            unlockInfo = null;
            if (!Enum.TryParse(Locale, ignoreCase: true, out Locale locale))
            {
                Main.LogWarning($"Failed to parse Locale.");
                return false;
            }

            unlockInfo = ScriptableObject.CreateInstance<UnlockInfo>();
            unlockInfo.Name = Name;
            unlockInfo.Locale = locale;
            unlockInfo.FlavourText = FlavourText;
            unlockInfo.Description = Description;
            return true;
        }
    }
}
