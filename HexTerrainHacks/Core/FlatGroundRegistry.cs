using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace HexTerrainHacks.Core
{
    internal static class FlatGroundRegistry
    {
        private const string PrefabName = "hex_flat_ground";
        private static GameObject _flatGroundPrefab;

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
                    Plugin.Log.LogError("[Terrain] mud_road_v2 not found in Hoe PieceTable.");
                    return;
                }

                _flatGroundPrefab = Object.Instantiate(vanillaPrefab);
                _flatGroundPrefab.name = PrefabName;
                _flatGroundPrefab.SetActive(false);

                var piece = _flatGroundPrefab.GetComponent<Piece>();
                piece.m_name = "Flat Ground";

                var settings = _flatGroundPrefab.GetComponent<TerrainOp>().m_settings;
                settings.m_level = true;
                settings.m_levelRadius = Plugin.LevelRadius.Value;
                settings.m_square = true;
                settings.m_smooth = false;

                Plugin.Log.LogInfo("[Terrain] Flat Ground prefab initialized.");
            }

            RegisterTerrainOp(database);
            RegisterPiece(database);

            if (ZNetScene.instance)
            {
                RegisterPrefab(ZNetScene.instance);
            }
        }

        internal static void RegisterPrefab(ZNetScene scene)
        {
            if (!_flatGroundPrefab)
            {
                return;
            }

            var prefabs = AccessTools.Field(typeof(ZNetScene), "m_prefabs").GetValue(scene) as List<GameObject>;
            var namedPrefabs = AccessTools.Field(typeof(ZNetScene), "m_namedPrefabs").GetValue(scene) as Dictionary<int, GameObject>;
            var hash = PrefabName.GetStableHashCode();

            if (namedPrefabs.TryGetValue(hash, out var existingPrefab))
            {
                if (existingPrefab != _flatGroundPrefab)
                {
                    Plugin.Log.LogError($"[Terrain] Prefab hash conflict: {PrefabName}");
                }

                return;
            }

            prefabs.Add(_flatGroundPrefab);
            namedPrefabs.Add(hash, _flatGroundPrefab);
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
        }

        internal static void RegisterPiece(ObjectDB database)
        {
            if (!_flatGroundPrefab)
            {
                return;
            }

            var hoePrefab = database.GetItemPrefab("Hoe");
            var pieceTable = hoePrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces;

            if (!pieceTable.m_pieces.Contains(_flatGroundPrefab))
            {
                pieceTable.m_pieces.Add(_flatGroundPrefab);
            }
        }
    }
}