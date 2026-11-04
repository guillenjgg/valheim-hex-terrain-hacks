using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using HexTerrainHacks.Core;
using UnityEngine;

namespace HexTerrainHacks.Patches
{
    [HarmonyPatch(typeof(TerrainComp), "PaintCleared")]
    internal static class TerrainCompSquarePaintPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var originalMethod = AccessTools.Method(typeof(Vector2), nameof(Vector2.Distance), new[] { typeof(Vector2), typeof(Vector2) });
            var replacementMethod = AccessTools.Method(typeof(TerrainCompSquarePaintPatch), nameof(GetPaintDistance));
            var patchedInstructions = new List<CodeInstruction>(instructions);
            var replacements = 0;

            for (var i = 0; i < patchedInstructions.Count; i++)
            {
                var instruction = patchedInstructions[i];

                if ((instruction.opcode != OpCodes.Call && instruction.opcode != OpCodes.Callvirt) || !Equals(instruction.operand, originalMethod))
                {
                    continue;
                }

                patchedInstructions.Insert(i, new CodeInstruction(OpCodes.Ldarg_3));
                i++;
                patchedInstructions[i].opcode = OpCodes.Call;
                patchedInstructions[i].operand = replacementMethod;
                replacements++;
            }

            if (replacements != 1)
            {
                throw new InvalidOperationException($"Expected one paint distance calculation, found {replacements}.");
            }

            return patchedInstructions;
        }

        private static float GetPaintDistance(Vector2 center, Vector2 point, TerrainOp.Settings settings)
        {
            if (!ReferenceEquals(settings, FlatGroundRegistry.FlatGroundSettings))
            {
                return Vector2.Distance(center, point);
            }

            return Mathf.Max(Mathf.Abs(center.x - point.x), Mathf.Abs(center.y - point.y));
        }
    }
}