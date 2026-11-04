using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace HexTerrainHacks.Core
{
    internal static class FlatGroundRegistry
    {
        private const string PrefabName = "hex_flat_ground";
        private static GameObject _flatGroundPrefab;
        private static GameObject _prefabContainer;

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

                if (!_prefabContainer)
                {
                    _prefabContainer = new GameObject("HexTerrainHacks_Prefabs");
                    _prefabContainer.SetActive(false);
                    Object.DontDestroyOnLoad(_prefabContainer);
                }

                _flatGroundPrefab = Object.Instantiate(vanillaPrefab, _prefabContainer.transform, false);
                _flatGroundPrefab.name = PrefabName;

                var piece = _flatGroundPrefab.GetComponent<Piece>();
                piece.m_name = "Flat Ground";

                var settings = _flatGroundPrefab.GetComponent<TerrainOp>().m_settings;
                settings.m_level = true;
                settings.m_levelRadius = Plugin.LevelRadius.Value;
                settings.m_square = true;
                settings.m_smooth = false;
                settings.m_paintCleared = true;
                settings.m_paintRadius = Plugin.LevelRadius.Value;
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
            var pieceTable = hoePrefab.GetComponent<ItemDrop>().m_itemData.m_shared.m_buildPieces;

            if (!pieceTable.m_pieces.Contains(_flatGroundPrefab))
            {
                pieceTable.m_pieces.Add(_flatGroundPrefab);
            }
        }
    }
}