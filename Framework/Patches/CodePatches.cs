using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;

namespace ParticleFramework.Framework.Patches
{
    internal class CodePatches : PatchTemplate
    {
        internal CodePatches(Harmony harmony) : base(harmony) { }
        internal void Apply()
        {
            Patch(typeof(Farmer), nameof(Farmer.draw), nameof(FarmerDrawPostfix), [typeof(SpriteBatch)]);
            Patch(typeof(SObject), nameof(SObject.draw), nameof(ObjectDrawPostfix), [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]);
            Patch(typeof(NPC), nameof(NPC.draw), nameof(NPCDrawPostfix), [typeof(SpriteBatch), typeof(float)]);
            Patch(typeof(Furniture), nameof(Furniture.draw), nameof(FurnitureDrawPostfix), [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]);
            Patch(typeof(BedFurniture), nameof(BedFurniture.draw), nameof(BedFurnitureDrawPostfix), [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]);
            Patch(typeof(FishTankFurniture), nameof(FishTankFurniture.draw), nameof(FishTankFurnitureDrawPostfix), [typeof(SpriteBatch), typeof(int), typeof(int), typeof(float)]);
        }

        public static void FarmerDrawPostfix(Farmer __instance, SpriteBatch b)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
        }
        public static void ObjectDrawPostfix(SObject __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
        }
        public static void NPCDrawPostfix(NPC __instance, SpriteBatch b)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
        }

        public static void FurnitureDrawPostfix(Furniture __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
        }

        public static void BedFurnitureDrawPostfix(BedFurniture __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
        }

        // Fix later
        public static void FishTankFurnitureDrawPostfix(FishTankFurniture __instance, SpriteBatch spriteBatch, int x, int y)
        {
            if (!ModEntry.modConfig.EnableMod)
                return;
        }
    }
}
