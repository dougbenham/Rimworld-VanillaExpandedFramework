﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VFECore
{
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;


    [HarmonyPatch(typeof(PawnRenderer), nameof(PawnRenderer.RenderPawnAt))]
    public static class Patch_RenderPawnAt
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo humanlikeInfo = AccessTools.PropertyGetter(typeof(RaceProperties), nameof(RaceProperties.Humanlike));

            foreach (CodeInstruction instruction in instructions)
            {
                yield return instruction;
                if (instruction.Calls(humanlikeInfo))
                {
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(VFEGlobal),         nameof(VFEGlobal.settings)));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(VFEGlobalSettings), nameof(VFEGlobalSettings.disableCaching)));
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    yield return new CodeInstruction(OpCodes.Ceq);
                    yield return new CodeInstruction(OpCodes.And);

                    yield return new CodeInstruction(OpCodes.Ldloc_1);
                    yield return CodeInstruction.Call(typeof(Patch_RenderPawnAt), nameof(ChangeFlags));
                    yield return new CodeInstruction(OpCodes.Stloc_1);
                }
            }
        }

        public static PawnRenderFlags ChangeFlags(PawnRenderFlags pawnRenderFlags)
        {
            pawnRenderFlags |= PawnRenderFlags.Headgear;
            pawnRenderFlags |= PawnRenderFlags.Clothes;

            return pawnRenderFlags;
        }
    }

    

    [HarmonyPatch(typeof(GlobalTextureAtlasManager), nameof(GlobalTextureAtlasManager.TryGetPawnFrameSet))]
    [HarmonyPatch(typeof(GlobalTextureAtlasManager), nameof(GlobalTextureAtlasManager.TryMarkPawnFrameSetDirty))]
    public static class Patch_DisableAtlasCaching
    {
        [HarmonyPrefix]
        public static bool Prefix(ref bool __result)
        {
            __result = !VFEGlobal.settings.disableCaching;
            return __result;
        }
    }
}
