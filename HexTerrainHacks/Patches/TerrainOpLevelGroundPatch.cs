using HarmonyLib;

namespace HexTerrainHacks.Patches
{
    [HarmonyPatch(typeof(ObjectDB), "UpdateRegisters")]
    internal static class TerrainOpLevelGroundPatch
    {
        private static void Postfix(ObjectDB __instance)
        {
            if (!__instance.TryGetTerrainOp("mud_road_v2", out var terrainOp))
            {
                return;
            }

            var settings = terrainOp.m_settings;

            settings.m_level = true;
            settings.m_levelRadius = 2f;
            settings.m_square = true;
            settings.m_smooth = false;
        }
    }
}
