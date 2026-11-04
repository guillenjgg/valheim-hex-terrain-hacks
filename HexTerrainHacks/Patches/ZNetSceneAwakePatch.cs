using HarmonyLib;
using HexTerrainHacks.Core;

namespace HexTerrainHacks.Patches
{
    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    internal static class ZNetSceneAwakePatch
    {
        private static void Postfix(ZNetScene __instance)
        {
            FlatGroundRegistry.RegisterPrefab(__instance);
        }
    }
}