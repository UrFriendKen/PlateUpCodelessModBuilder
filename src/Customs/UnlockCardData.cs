using System.Collections.Generic;

namespace ModName.src.Customs
{
    public class UnlockCardData
    {
        public string Name;
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
    }
}
