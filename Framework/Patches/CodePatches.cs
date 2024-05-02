﻿using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleFramework.Framework.Managers;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
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
            Patch(typeof(Furniture), nameof(Furniture.draw), nameof(FurnitureDrawPostfix), [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]);
            Patch(typeof(BedFurniture), nameof(BedFurniture.draw), nameof(BedFurnitureDrawPostfix), [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]);
            Patch(typeof(FishTankFurniture), nameof(FishTankFurniture.draw), nameof(FishTankFurnitureDrawPostfix), [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]);
        }

        public static void FarmerDrawPostfix(Farmer __instance, SpriteBatch b)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
            if (ParticleEffectManager.farmerDict.ContainsKey(__instance.UniqueMultiplayerID))
            {
                foreach (var effect in ParticleEffectManager.farmerDict[__instance.UniqueMultiplayerID])
                {
                    if (ParticleEffectManager.effectDict.ContainsKey(effect))
                    {
                        ParticleEffectManager.ShowFarmerParticleEffect(b, __instance, effect, ParticleEffectManager.effectDict[effect]);
                    }
                }
            }
            foreach (var kvp in ParticleEffectManager.effectDict)
            {
                switch (kvp.Value.type.ToLower())
                {
                    case "hat":
                        if (__instance.hat.Value != null && __instance.hat.Value.QualifiedItemId == kvp.Value.name)
                            ParticleEffectManager.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "shirt":
                        if (__instance.shirtItem.Value != null && __instance.shirtItem.Value.QualifiedItemId == kvp.Value.name)
                            ParticleEffectManager.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "pants":
                        if (__instance.pantsItem.Value != null && __instance.pantsItem.Value.QualifiedItemId == kvp.Value.name)
                            ParticleEffectManager.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "boots":
                        if (__instance.boots.Value != null && __instance.boots.Value.QualifiedItemId == kvp.Value.name)
                            ParticleEffectManager.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "tool":
                        if (__instance.CurrentItem is Tool && __instance.CurrentItem.QualifiedItemId == kvp.Value.name)
                            ParticleEffectManager.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                    case "ring":
                        if (__instance.leftRing.Value != null && __instance.leftRing.Value.QualifiedItemId == kvp.Value.name)
                            ParticleEffectManager.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        else if (__instance.rightRing.Value != null && __instance.rightRing.Value.QualifiedItemId == kvp.Value.name)
                            ParticleEffectManager.ShowFarmerParticleEffect(b, __instance, kvp.Key, kvp.Value);
                        break;
                }
            }
        }
        public static void ObjectDrawPostfix(Object __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
            if (ParticleEffectManager.objectDict.ContainsKey(__instance.QualifiedItemId))
            {
                foreach (var effect in ParticleEffectManager.objectDict[__instance.QualifiedItemId])
                {
                    if (ParticleEffectManager.effectDict.ContainsKey(effect))
                    {
                        ParticleEffectManager.ShowObjectParticleEffect(spriteBatch, __instance, x, y, effect, ParticleEffectManager.effectDict[effect]);
                    }
                }
            }
            foreach (var kvp in ParticleEffectManager.effectDict)
            {
                if (kvp.Value.type.ToLower() == "object" && kvp.Value.name == __instance.QualifiedItemId)
                {
                    ParticleEffectManager.ShowObjectParticleEffect(spriteBatch, __instance, x, y, kvp.Key, kvp.Value);
                }
            }
        }
        public static void NPCDrawPostfix(NPC __instance, SpriteBatch b)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
            if (ParticleEffectManager.NPCDict.ContainsKey(__instance.Name))
            {
                foreach (var effect in ParticleEffectManager.NPCDict[__instance.Name])
                {
                    if (ParticleEffectManager.effectDict.ContainsKey(effect))
                    {
                        ParticleEffectManager.ShowNPCParticleEffect(b, __instance, effect, ParticleEffectManager.effectDict[effect]);
                    }
                }
            }
            foreach (var kvp in ParticleEffectManager.effectDict)
            {
                if (kvp.Value.type.ToLower() == "npc" && kvp.Value.name == __instance.Name)
                {
                    ParticleEffectManager.ShowNPCParticleEffect(b, __instance, kvp.Key, kvp.Value);
                }
            }
        }

        public static void FurnitureDrawPostfix(Furniture __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
            if (ParticleEffectManager.furnitureDict.ContainsKey(__instance.QualifiedItemId))
            {
                foreach (var effect in ParticleEffectManager.furnitureDict[__instance.QualifiedItemId])
                {
                    if (ParticleEffectManager.effectDict.ContainsKey(effect))
                    {
                        ParticleEffectManager.ShowObjectParticleEffect(spriteBatch, __instance, x, y, effect, ParticleEffectManager.effectDict[effect]);
                    }
                }
            }
            foreach (var kvp in ParticleEffectManager.effectDict)
            {
                if (kvp.Value.type.ToLower() == "furniture" && kvp.Value.name == __instance.QualifiedItemId)
                {
                    ParticleEffectManager.ShowFurnitureParticleEffect(spriteBatch, __instance, x, y, kvp.Key, kvp.Value);
                }
            }
        }

        public static void BedFurnitureDrawPostfix(BedFurniture __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
            if (ParticleEffectManager.furnitureDict.ContainsKey(__instance.QualifiedItemId))
            {
                foreach (var effect in ParticleEffectManager.furnitureDict[__instance.QualifiedItemId])
                {
                    if (ParticleEffectManager.effectDict.ContainsKey(effect))
                    {
                        ParticleEffectManager.ShowObjectParticleEffect(spriteBatch, __instance, x, y, effect, ParticleEffectManager.effectDict[effect]);
                    }
                }
            }
            foreach (var kvp in ParticleEffectManager.effectDict)
            {
                if (kvp.Value.type.ToLower() == "furniture" && kvp.Value.name == __instance.QualifiedItemId)
                {
                    ParticleEffectManager.ShowFurnitureParticleEffect(spriteBatch, __instance, x, y, kvp.Key, kvp.Value);
                }
            }
        }

        // Fix later
        public static void FishTankFurnitureDrawPostfix(FishTankFurniture __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
            if (ParticleEffectManager.furnitureDict.ContainsKey(__instance.QualifiedItemId))
            {
                foreach (var effect in ParticleEffectManager.furnitureDict[__instance.QualifiedItemId])
                {
                    if (ParticleEffectManager.effectDict.ContainsKey(effect))
                    {
                        ParticleEffectManager.ShowObjectParticleEffect(spriteBatch, __instance, x, y, effect, ParticleEffectManager.effectDict[effect]);
                    }
                }
            }
            foreach (var kvp in ParticleEffectManager.effectDict)
            {
                if (kvp.Value.type.ToLower() == "furniture" && kvp.Value.name == __instance.QualifiedItemId)
                {
                    ParticleEffectManager.ShowFurnitureParticleEffect(spriteBatch, __instance, x, y, kvp.Key, kvp.Value);
                }
            }
        }
    }
}
