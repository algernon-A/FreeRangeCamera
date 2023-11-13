// <copyright file="CameraControllerPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace FreeRangeCamera
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using Game;
    using Game.Simulation;
    using HarmonyLib;

    /// <summary>
    /// Harmony patches for <see cref="CameraController"/> to implement per-tile cost limits.
    /// </summary>
    [HarmonyPatch(typeof(CameraController))]
    internal static class CameraControllerPatches
    {
        /// <summary>
        /// Harmony transpiler for <see cref="CameraController.UpdateCamera"/> to override map edge bounds check.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateCamera")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateStatusTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Patcher.Instance.Log.Info("transpiling " + original.DeclaringType + '.' + original.Name);

            // Look for call to this.
            MethodInfo getBounds = AccessTools.Method(typeof(TerrainUtils), nameof(TerrainUtils.GetBounds));


            // Parse instructions.
            IEnumerator<CodeInstruction> instructionEnumerator = instructions.GetEnumerator();
            while (instructionEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionEnumerator.Current;

                // Look for call to TerrainUtils.GetBounds.
                if (instruction.Calls(getBounds))
                {
                    Patcher.Instance.Log.Debug("found call to TerrainUtils.GetBounds");

                    // Drop instruction argument and duplicate previous value on stack (m_Pivot).
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Dup);

                    continue;
                }

                yield return instruction;
            }
        }
    }
}
