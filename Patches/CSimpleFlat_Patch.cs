using HarmonyLib;
using KitchenLib.Customs;
using UnityEngine;

namespace ModName.Patches
{
    [HarmonyPatch]
    static class CSimpleFlat_Patch
    {
        [HarmonyPatch(typeof(CSimpleFlat), nameof(CSimpleFlat.ConvertMaterial))]
        [HarmonyPostfix]
        static void ConvertMaterial_Postfix(ref CSimpleFlat __instance, Material material)
        {
            if (!__instance._HasTextureOverlay)
                material.DisableKeyword("_HASTEXTUREOVERLAY_ON");
            else
                material.EnableKeyword("_HASTEXTUREOVERLAY_ON");
        }
    }
}
