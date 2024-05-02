using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System.Collections.Generic;
using Object = StardewValley.Object;

namespace ParticleFramework.Framework.Patches
{
    internal class CodePatches : PatchTemplate
    {
        internal CodePatches(Harmony harmony) : base(harmony) { }
        internal void Apply()
        {
            Patch(typeof(Farmer), nameof(Farmer.draw), nameof(FarmerDrawPostfix), [typeof(SpriteBatch)]);
            Patch(typeof(Object), nameof(Object.draw), nameof(ObjectDrawPostfix), [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]);
            Patch(typeof(NPC), nameof(NPC.draw), nameof(NPCDrawPostfix), [typeof(SpriteBatch), typeof(float)]);
        }

        public static void FarmerDrawPostfix(Farmer __instance, SpriteBatch b)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
            if (ModEntry.farmerDict.ContainsKey(__instance.UniqueMultiplayerID))
            {
                foreach (var effect in ModEntry.farmerDict[__instance.UniqueMultiplayerID])
                {
                    if (ModEntry.effectDict.ContainsKey(effect))
                    {
                        ModEntry.ShowFarmerParticleEffect(b, __instance, effect, ModEntry.effectDict[effect]);
                    }
                }
            }
            foreach (var kvp in ModEntry.effectDict)
            {
                switch (kvp.Value.type.ToLower())
                {
                    case "hat":
                        if (__instance.hat.Value != null && __instance.hat.Value.Name == kvp.Value.name)
                            ModEntry.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "shirt":
                        if (__instance.shirt.Value.ToString() == kvp.Value.name)
                            ModEntry.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "pants":
                        if (__instance.pants.Value.ToString() == kvp.Value.name)
                            ModEntry.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "boots":
                        if (__instance.boots.Value != null && __instance.boots.Value.Name == kvp.Value.name)
                            ModEntry.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "tool":
                        if (__instance.CurrentItem is Tool && __instance.CurrentItem.Name == kvp.Value.name)
                            ModEntry.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "ring":
                        if (__instance.leftRing.Value != null && __instance.leftRing.Value.Name == kvp.Value.name)
                            ModEntry.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        else if (__instance.rightRing.Value != null && __instance.rightRing.Value.Name == kvp.Value.name)
                            ModEntry.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                }
            }
        }
        public static void ObjectDrawPostfix(Object __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
            foreach (var kvp in ModEntry.effectDict)
            {
                if (kvp.Value.type.ToLower() == "object" && kvp.Value.name == __instance.Name)
                {
                    ModEntry.ShowObjectParticleEffect(spriteBatch, __instance, x, y, kvp.Key, kvp.Value);
                }
            }
        }
        public static void NPCDrawPostfix(NPC __instance, SpriteBatch b)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
            if (ModEntry.NPCDict.ContainsKey(__instance.Name))
            {
                foreach (var effect in ModEntry.NPCDict[__instance.Name])
                {
                    if (ModEntry.effectDict.ContainsKey(effect))
                    {
                        ModEntry.ShowNPCParticleEffect(b, __instance, effect, ModEntry.effectDict[effect]);
                    }
                }
            }
            foreach (var kvp in ModEntry.effectDict)
            {
                if (kvp.Value.type.ToLower() == "npc" && kvp.Value.name == __instance.Name)
                {
                    ModEntry.ShowNPCParticleEffect(b, __instance, kvp.Key, kvp.Value);
                }
            }
        }
    }
}
