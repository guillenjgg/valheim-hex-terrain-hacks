using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;

namespace HexTerrainHacks
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.hex.terrain.hacks";
        private const string PluginName = "HexTerrainHacks";
        private const string PluginVersion = "1.0.0";

        private Harmony _harmonyInstance;

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        internal static ConfigEntry<bool> EnableDebugLogging;
        internal static ConfigEntry<float> LevelRadius;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            EnableDebugLogging = Config.Bind("General", "EnableDebugLogging", false,
                new ConfigDescription("Enable debug logging for the plugin."));

            LevelRadius = Config.Bind("Terrain", "LevelRadius", 2f,
                new ConfigDescription("The radius of the level ground operation.",
                new AcceptableValueRange<float>(1f, 10f)));

            LevelRadius.SettingChanged += OnLevelRadiusChanged;

            _harmonyInstance = new Harmony(PluginGuid);
            _harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void OnDestroy()
        {
            Log.LogInfo($"{PluginName} v{PluginVersion} unloaded.");

            LevelRadius.SettingChanged -= OnLevelRadiusChanged;

            _harmonyInstance?.UnpatchSelf();
            _harmonyInstance = null;
            Instance = null;
            Log = null;
        }

        private void OnLevelRadiusChanged(object sender, EventArgs e)
        {
            if (ObjectDB.instance != null && ObjectDB.instance.TryGetTerrainOp("hex_flat_ground", out var terrainOp))
            {
                terrainOp.m_settings.m_levelRadius = LevelRadius.Value;
                terrainOp.m_settings.m_paintRadius = LevelRadius.Value;
            }
        }
    }
}
