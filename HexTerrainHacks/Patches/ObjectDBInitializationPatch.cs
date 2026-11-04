using HarmonyLib;
using HexTerrainHacks.Core;

namespace HexTerrainHacks.Patches
{
    [HarmonyPatch]
    internal static class ObjectDBInitializationPatch
    {
        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
        [HarmonyPostfix]
        private static void AwakePostfix(ObjectDB __instance)
        {
            FlatGroundRegistry.Initialize(__instance);
        }

        [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.CopyOtherDB))]
        [HarmonyPostfix]
        private static void CopyOtherDBPostfix(ObjectDB __instance)
        {
            FlatGroundRegistry.Initialize(__instance);
        }
    }
}