using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using HexValheimSdk.Configuration;
using HexTerrainHacks.Core;
using System;
using System.Reflection;
using UnityEngine;

namespace HexTerrainHacks
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.hex.terrain.hacks";
        private const string PluginName = "HexTerrainHacks";
        private const string PluginVersion = "1.0.0";

        private Harmony _harmonyInstance;
        private CommonConfigSettings _commonSettings;
        private ConfigFileWatcher _configWatcher;

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        internal static bool EnableDebugLogging => Instance?._commonSettings?.EnableDebugLogging.Value == true;
        internal static bool IsModEnabled => Instance?._commonSettings?.IsModEnabled.Value == true;
        internal static ConfigEntry<float> LevelRadius;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            var migrations = new ConfigMigrationPlan(
                new ConfigDefinition("Development", "ConfigurationVersion"),
                step => step.MoveSetting(
                    new ConfigDefinition("General", "EnableDebugLogging"),
                    new ConfigDefinition("Development", "EnableDebugLogging")));

            _commonSettings = ConfigInitializer.Initialize(Config, file =>
            {
                var terrain = new ConfigCategory(file, "Terrain");
                LevelRadius = terrain.Bind("LevelRadius", 2f,
                    "The radius of the level ground operation.",
                    new AcceptableValueRange<float>(1f, 10f));
            }, migrations);

            LevelRadius.SettingChanged += OnConfigChange;
            _commonSettings.IsModEnabled.SettingChanged += OnConfigChange;
            _configWatcher = new ConfigFileWatcher(Config, Logger, 500, migrations);

            _harmonyInstance = new Harmony(PluginGuid);
            _harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void OnDestroy()
        {
            Log?.LogInfo($"{PluginName} v{PluginVersion} unloaded.");
            _configWatcher?.Dispose();
            _configWatcher = null;

            if (LevelRadius != null)
                LevelRadius.SettingChanged -= OnConfigChange;
            if (_commonSettings != null)
                _commonSettings.IsModEnabled.SettingChanged -= OnConfigChange;

            _harmonyInstance?.UnpatchSelf();
            _harmonyInstance = null;
            Instance = null;
            Log = null;
        }

        private void OnConfigChange(object sender, EventArgs e)
        {
            if (ObjectDB.instance != null)
                FlatGroundRegistry.Initialize(ObjectDB.instance);

            if (ObjectDB.instance != null && ObjectDB.instance.TryGetTerrainOp("hex_flat_ground", out var terrainOp))
            {
                terrainOp.m_settings.m_levelRadius = LevelRadius.Value;
                terrainOp.m_settings.m_paintRadius = Mathf.Ceil(LevelRadius.Value) + 0.5f;
            }

            if (Player.m_localPlayer != null && ReferenceEquals(sender, _commonSettings.IsModEnabled))
            {
                Player.m_localPlayer.UpdateAvailablePiecesList();
                Player.m_localPlayer.SetupPlacementGhost();
            }
        }
    }
}
