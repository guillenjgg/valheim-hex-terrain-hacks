using HarmonyLib;
using System.Collections.Generic;
using HexTerrainHacks.Assets;
using UnityEngine;

namespace HexTerrainHacks.Core
{
    internal static class FlatGroundRegistry
    {
        private const string PrefabName = "hex_flat_ground";
        private static GameObject _flatGroundPrefab;
        private static GameObject _prefabContainer;

        internal static TerrainOp.Settings FlatGroundSettings { get; private set; }

        internal static void Initialize(ObjectDB database)
        {
            if (!_flatGroundPrefab)
            {
                var hoePrefab = database.GetItemPrefab("Hoe");

                if (!hoePrefab)
                {
                    return;
                }

                var pieceTable = hoePrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces;
                var vanillaPrefab = pieceTable.m_pieces.Find(p => p && p.name == "mud_road_v2");

                if (!vanillaPrefab)
                {
                    if (Plugin.EnableDebugLogging)
                    {
                        Plugin.Log?.LogError("[Terrain] mud_road_v2 not found in Hoe PieceTable.");
                    }

                    return;
                }

                if (!_prefabContainer)
                {
                    _prefabContainer = new GameObject("HexTerrainHacks_Prefabs");
                    _prefabContainer.SetActive(false);
                    Object.DontDestroyOnLoad(_prefabContainer);
                }

                _flatGroundPrefab = Object.Instantiate(vanillaPrefab, _prefabContainer.transform, false);
                _flatGroundPrefab.name = PrefabName;

                var icon = AssetHelper.LoadSprite("HexTerrainHacks.Assets.flat-ground.png");

                if (icon)
                {
                    _flatGroundPrefab.GetComponent<Piece>().m_icon = icon;
                }

                var settings = _flatGroundPrefab.GetComponent<TerrainOp>().m_settings;
                settings.m_level = true;
                settings.m_levelRadius = Plugin.LevelRadius.Value;
                settings.m_square = true;
                settings.m_smooth = false;
                settings.m_paintCleared = true;
                settings.m_paintRadius = Mathf.Ceil(Plugin.LevelRadius.Value) + 0.5f;
                settings.m_paintExp = 0.01f;
                settings.m_halfOffset = false;

                FlatGroundSettings = settings;

                if (Plugin.EnableDebugLogging)
                {
                    Plugin.Log?.LogDebug("[Terrain] Created Flat Ground prefab.");
                }

                var ghost = _flatGroundPrefab.transform.Find("_GhostOnly");

                if (ghost && ghost.GetComponent<ParticleSystem>() && ghost.GetComponent<ParticleSystemRenderer>())
                {
                    ghost.gameObject.AddComponent<FlatGroundPreviewComponent>();
                }
                else
                {
                    if (Plugin.EnableDebugLogging)
                    {
                        Plugin.Log?.LogWarning("[Terrain] Flat Ground preview particle system not found.");
                    }
                }
            }

            RegisterTerrainOp(database);
            RegisterPiece(database);
        }

        internal static void RegisterTerrainOp(ObjectDB database)
        {
            if (!_flatGroundPrefab)
            {
                return;
            }

            var terrainOp = _flatGroundPrefab.GetComponent<TerrainOp>();

            if (!database.m_terrainOps.Contains(terrainOp))
            {
                database.m_terrainOps.Add(terrainOp);
            }

            var terrainOpsByHash = AccessTools.Field(typeof(ObjectDB), "m_terrainOpsByHash").GetValue(database) as Dictionary<int, TerrainOp>;
            terrainOpsByHash[PrefabName.GetStableHashCode()] = terrainOp;
        }

        internal static void RegisterPiece(ObjectDB database)
        {
            if (!_flatGroundPrefab)
            {
                return;
            }

            var hoePrefab = database.GetItemPrefab("Hoe");

            if (!hoePrefab)
            {
                return;
            }

            var pieceTable = hoePrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces;

            _flatGroundPrefab.GetComponent<Piece>().m_enabled = Plugin.IsModEnabled;

            if (!Plugin.IsModEnabled)
            {
                pieceTable.m_pieces.Remove(_flatGroundPrefab);
                if (Plugin.EnableDebugLogging)
                {
                    Plugin.Log?.LogDebug("[Terrain] Flat Ground disabled.");
                }
                return;
            }

            if (!pieceTable.m_pieces.Contains(_flatGroundPrefab))
            {
                pieceTable.m_pieces.Add(_flatGroundPrefab);
                if (Plugin.EnableDebugLogging)
                {
                    Plugin.Log?.LogDebug("[Terrain] Added Flat Ground to the hoe menu.");
                }
            }
        }
    }
}
