using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleFramework.Framework.Api;
using ParticleFramework.Framework.Data;
using ParticleFramework.Framework.Interfaces;
using ParticleFramework.Framework.Managers;
using ParticleFramework.Framework.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Object = StardewValley.Object;

namespace ParticleFramework
{
    public class ModEntry : Mod
    {
        // Shared static helpers
        internal static IMonitor monitor;
        internal static IModHelper modHelper;
        internal static Multiplayer multiplayer;
        internal static ModConfig modConfig;

        // Managers
        internal static ApiManager apiManager;

        public static readonly string dictPath = "Mods/Espy.ParticleFramework/dict";
        public static Dictionary<string, ParticleEffectData> effectDict = new Dictionary<string, ParticleEffectData>();

        public static Dictionary<long, EntityParticleData> farmerEffectDict = new Dictionary<long, EntityParticleData>();
        public static Dictionary<string, EntityParticleData> npcEffectDict = new Dictionary<string, EntityParticleData>();
        public static Dictionary<string, EntityParticleData> locationEffectDict = new Dictionary<string, EntityParticleData>();
        public static EntityParticleData screenEffectDict = new EntityParticleData();
        public static Dictionary<string, EntityParticleData> objectEffectDict = new Dictionary<string, EntityParticleData>();

        public static Dictionary<Point, List<ParticleEffectData>> screenDict = new Dictionary<Point, List<ParticleEffectData>>();
        public static Dictionary<string, List<string>> NPCDict = new Dictionary<string, List<string>>();
        public static Dictionary<long, List<string>> farmerDict = new Dictionary<long, List<string>>();
        public static Dictionary<string, Dictionary<Point, List<ParticleEffectData>>> locationDict = new Dictionary<string, Dictionary<Point, List<ParticleEffectData>>>();


        public override void Entry(IModHelper helper)
        {
            // Setup i18n
            I18n.Init(helper.Translation);

            // Setup the monitor, helper and multiplayer
            monitor = Monitor;
            modHelper = helper;
            multiplayer = helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer").GetValue();

            // Setup the config
            modConfig = Helper.ReadConfig<ModConfig>();

            // Setup the manager
            apiManager = new ApiManager(monitor);

            // Apply the patches
            var harmony = new Harmony(this.ModManifest.UniqueID);

            new CodePatches(harmony).Apply();



            // Hook into GameLoop events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;
            helper.Events.GameLoop.TimeChanged += OnTimeChanged;
            helper.Events.Display.RenderedWorld += OnRenderedWorld;
            helper.Events.Display.RenderedHud += OnRenderedHud;

            helper.Events.Content.AssetRequested += OnAssetRequested;
        }

        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            if (Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu") && apiManager.HookIntoGenericModConfigMenu(Helper))
            {
                var configApi = apiManager.GetGenericModConfigMenuApi();
                configApi.Register(ModManifest, () => modConfig = new ModConfig(), () => Helper.WriteConfig(modConfig));

                AddOption(configApi, nameof(modConfig.EnableMod));

            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (modConfig.EnableMod && e.NameWithoutLocale.IsEquivalentTo(dictPath))
            {
                e.LoadFrom(() => new Dictionary<string, ParticleEffectData>(), AssetLoadPriority.Exclusive);
            }
        }

        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (!modConfig.EnableMod)
                return;
            foreach (var kvp in screenDict)
            {
                foreach (var effect in kvp.Value)
                {
                    ShowScreenParticleEffect(e.SpriteBatch, effect);
                }
            }
            foreach (var key in effectDict.Keys)
            {
                var ped = effectDict[key];
                switch (ped.type.ToLower())
                {
                    case "screen":
                        ShowScreenParticleEffect(e.SpriteBatch, ped);
                        break;
                }
            }
        }

        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!modConfig.EnableMod)
                return;
            if (locationDict.TryGetValue(Game1.currentLocation.Name, out Dictionary<Point, List<ParticleEffectData>> dict))
            {
                foreach (var kvp in dict)
                {
                    foreach (var effect in kvp.Value)
                    {
                        ShowLocationParticleEffect(e.SpriteBatch, Game1.currentLocation, effect);
                    }
                }
            }
            foreach (var key in effectDict.Keys)
            {
                var ped = effectDict[key];
                switch (ped.type.ToLower())
                {
                    case "location":
                        if (Game1.currentLocation.Name == ped.name)
                            ShowLocationParticleEffect(e.SpriteBatch, Game1.currentLocation, ped);
                        break;
                }
            }
        }

        public override object GetApi()
        {
            return new ParticleEffectsAPI();
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            LoadEffects();
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            LoadEffects();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            LoadEffects();
        }

        public static void ShowFarmerParticleEffect(SpriteBatch b, Farmer instance, string key, ParticleEffectData ped)
        {
            if (!farmerEffectDict.TryGetValue(instance.UniqueMultiplayerID, out EntityParticleData entityParticleData))
            {
                entityParticleData = new EntityParticleData();
                farmerEffectDict[instance.UniqueMultiplayerID] = entityParticleData;
            }
            if (!entityParticleData.particleDict.TryGetValue(key, out var particleList))
            {
                particleList = new List<ParticleData>();
                farmerEffectDict[instance.UniqueMultiplayerID].particleDict[key] = particleList;
            }
            ShowParticleEffect(b, particleList, ped, instance.GetBoundingBox().Center.ToVector2() + new Vector2(ped.fieldOffsetX, ped.fieldOffsetY), instance.getDrawLayer());
            farmerEffectDict[instance.UniqueMultiplayerID] = entityParticleData;
        }
        public static void ShowNPCParticleEffect(SpriteBatch b, NPC instance, string key, ParticleEffectData ped)
        {
            if (!npcEffectDict.TryGetValue(instance.Name, out EntityParticleData entityParticleData))
            {
                entityParticleData = new EntityParticleData();
                npcEffectDict[instance.Name] = entityParticleData;
            }
            if (!entityParticleData.particleDict.TryGetValue(key, out var particleList))
            {
                particleList = new List<ParticleData>();
                npcEffectDict[instance.Name].particleDict[key] = particleList;
            }
            ShowParticleEffect(b, particleList, ped, instance.GetBoundingBox().Center.ToVector2() + new Vector2(ped.fieldOffsetX, ped.fieldOffsetY), 1f);
            npcEffectDict[instance.Name] = entityParticleData;
        }
        public static void ShowObjectParticleEffect(SpriteBatch b, Object instance, int x, int y, string key, ParticleEffectData ped)
        {
            var oKey = instance.Name + "|" + x + "," + y;
            if (!objectEffectDict.TryGetValue(oKey, out EntityParticleData entityParticleData))
            {
                entityParticleData = new EntityParticleData();
                objectEffectDict[oKey] = entityParticleData;
            }
            if (!entityParticleData.particleDict.TryGetValue(key, out var particleList))
            {
                particleList = new List<ParticleData>();
                objectEffectDict[oKey].particleDict[key] = particleList;
            }
            ShowParticleEffect(b, particleList, ped, instance.GetBoundingBoxAt(x, y).Center.ToVector2() + new Vector2(ped.fieldOffsetX, ped.fieldOffsetY), Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f);
            objectEffectDict[oKey] = entityParticleData;
        }
        public static void ShowLocationParticleEffect(SpriteBatch b, GameLocation instance, ParticleEffectData ped)
        {
            if (!locationEffectDict.TryGetValue(instance.Name, out EntityParticleData entityParticleData))
            {
                entityParticleData = new EntityParticleData();
                locationEffectDict[instance.Name] = entityParticleData;
            }
            List<ParticleData> particleList;
            if (!entityParticleData.particleDict.TryGetValue(ped.key, out particleList))
            {
                particleList = new List<ParticleData>();
                locationEffectDict[instance.Name].particleDict[ped.key] = particleList;
            }
            ShowParticleEffect(b, particleList, ped, new Vector2(ped.fieldOffsetX, ped.fieldOffsetY), 1f);
            locationEffectDict[instance.Name] = entityParticleData;
        }
        public static void ShowScreenParticleEffect(SpriteBatch b, ParticleEffectData ped)
        {
            List<ParticleData> particleList;
            if (!screenEffectDict.particleDict.TryGetValue(ped.key, out particleList))
            {
                particleList = new List<ParticleData>();
                screenEffectDict.particleDict[ped.key] = particleList;
            }
            ShowParticleEffect(b, particleList, ped, new Vector2(ped.fieldOffsetX, ped.fieldOffsetY), 1f, true);
        }
        public static void ShowParticleEffect(SpriteBatch b, List<ParticleData> particleList, ParticleEffectData ped, Vector2 center, float drawDepth, bool screen = false)
        {
            for (int i = particleList.Count - 1; i >= 0; i--)
            {
                particleList[i].age++;
                if (particleList[i].age > particleList[i].lifespan)
                {
                    particleList.RemoveAt(i);
                    continue;
                }
                Vector2 direction = particleList[i].direction;
                if (direction == Vector2.Zero)
                {
                    if (ped.movementType.Contains(" "))
                    {
                        var split = ped.movementType.Split(' ');
                        if (split.Length == 2
                            && float.TryParse(split[0], NumberStyles.Any, CultureInfo.InvariantCulture, out float x)
                            && float.TryParse(split[1], NumberStyles.Any, CultureInfo.InvariantCulture, out float y)
                        )
                        {
                            direction = new Vector2(x, y);
                            direction.Normalize();
                        }
                    }
                    else
                    {
                        switch (ped.movementType)
                        {
                            case "away":
                                direction = (center - particleList[i].position) * new Vector2(-1, -1);
                                direction.Normalize();
                                break;
                            case "towards":
                                direction = center - particleList[i].position;
                                direction.Normalize();
                                break;
                            case "up":
                                direction = new Vector2(0, -1);
                                break;
                            case "down":
                                direction = new Vector2(0, 1);
                                break;
                            case "left":
                                direction = new Vector2(-1, 0);
                                break;
                            case "right":
                                direction = new Vector2(1, 0);
                                break;
                            case "random":
                                direction = new Vector2((float)Game1.random.NextDouble() - 0.5f, (float)Game1.random.NextDouble() - 0.5f);
                                direction.Normalize();
                                break;
                        }
                    }
                    particleList[i].direction = direction;
                }
                particleList[i].position += particleList[i].direction * (ped.movementSpeed + ped.acceleration * particleList[i].age);
                if (IsOutOfBounds(particleList[i], ped, center))
                {
                    particleList.RemoveAt(i);
                    continue;
                }
                particleList[i].rotation += particleList[i].rotationRate;
            }
            if (particleList.Count < ped.maxParticles && Game1.random.NextDouble() < ped.particleChance)
            {
                var particle = new ParticleData();
                particle.lifespan = Game1.random.Next(ped.minLifespan, ped.maxLifespan + 1);
                particle.scale = ped.minParticleScale + (float)Game1.random.NextDouble() * (ped.maxParticleScale - ped.minParticleScale);
                particle.alpha = ped.minAlpha + (float)Game1.random.NextDouble() * (ped.maxAlpha - ped.minAlpha);
                particle.rotationRate = ped.minRotationRate + (float)Game1.random.NextDouble() * (ped.maxRotationRate - ped.minRotationRate);
                particle.option = Game1.random.Next(ped.spriteSheet.Height / ped.particleHeight);
                if (ped.fieldOuterRadius <= 0)
                {
                    double x;
                    double y;
                    if (screen)
                    {
                        if (ped.fieldOffsetX < 0)
                            ped.fieldOffsetX = Game1.viewport.Width / 2;
                        if (ped.fieldOffsetY < 0)
                            ped.fieldOffsetY = Game1.viewport.Height / 2;
                        if (ped.fieldOuterWidth < 0)
                            ped.fieldOuterWidth = Game1.viewport.Width;
                        if (ped.fieldOuterHeight < 0)
                            ped.fieldOuterHeight = Game1.viewport.Height;
                    }
                    else
                    {
                        if (ped.fieldOffsetX < 0)
                            ped.fieldOffsetX = Game1.currentLocation.map.DisplayWidth / 2;
                        if (ped.fieldOffsetY < 0)
                            ped.fieldOffsetY = Game1.currentLocation.map.DisplayHeight / 2;
                        if (ped.fieldOuterWidth < 0)
                            ped.fieldOuterWidth = Game1.currentLocation.map.DisplayWidth;
                        if (ped.fieldOuterHeight < 0)
                            ped.fieldOuterHeight = Game1.currentLocation.map.DisplayHeight;
                    }
                    if (ped.fieldInnerHeight > 0)
                    {
                        var innerTop = (ped.fieldOuterHeight - ped.fieldInnerHeight) / 2;
                        var innerBottom = ped.fieldOuterHeight - innerTop;
                        var innerLeft = (ped.fieldOuterWidth - ped.fieldInnerWidth) / 2;
                        var innerRight = ped.fieldOuterWidth - innerLeft;
                        var pixel = (int)((ped.fieldOuterWidth * innerTop * 2 + ped.fieldInnerHeight * innerLeft * 2) * Game1.random.NextDouble());
                        if (pixel >= ped.fieldOuterWidth * innerTop + ped.fieldInnerHeight * innerLeft * 2) // bottom
                        {
                            pixel = pixel - ped.fieldOuterWidth * innerTop - ped.fieldInnerHeight * innerLeft * 2;
                            x = pixel % ped.fieldOuterWidth;
                            y = innerBottom + pixel / ped.fieldOuterWidth;
                        }
                        else if (pixel >= ped.fieldOuterWidth * innerTop + ped.fieldInnerHeight * innerLeft) // right
                        {
                            pixel = pixel - ped.fieldOuterWidth * innerTop - ped.fieldInnerHeight * innerLeft;
                            x = innerRight + pixel % innerLeft;
                            y = innerTop + pixel / innerLeft;
                        }
                        else if (pixel >= ped.fieldOuterWidth * innerTop) // left
                        {
                            pixel = pixel - ped.fieldOuterWidth * innerTop;
                            x = pixel % innerLeft;
                            y = innerTop + pixel / innerLeft;
                        }
                        else // top
                        {
                            x = pixel % ped.fieldOuterWidth;
                            y = pixel / ped.fieldOuterWidth;
                        }
                    }
                    else
                    {
                        x = ped.fieldOuterWidth * Game1.random.NextDouble();
                        y = ped.fieldOuterHeight * Game1.random.NextDouble();
                    }
                    particle.position = center - new Vector2(ped.fieldOuterWidth, ped.fieldOuterHeight) / 2 + new Vector2((float)x, (float)y);
                }
                else
                {
                    particle.position = center + GetCirclePos(ped);
                }
                particleList.Add(particle);
            }
            int frames = ped.spriteSheet.Width / ped.particleWidth;
            foreach (var particle in particleList)
            {
                float depthOffset = ped.belowOffset >= 0 ? (ped.aboveOffset >= 0 ? (Game1.random.NextDouble() < 0.5 ? ped.aboveOffset : ped.belowOffset) : ped.belowOffset) : ped.aboveOffset;
                int frame = (int)Math.Round(particle.age * ped.frameSpeed) % frames;
                b.Draw(ped.spriteSheet, new Rectangle(Utility.Vector2ToPoint(screen ? particle.position : Game1.GlobalToLocal(particle.position)), new Point((int)(ped.particleWidth * particle.scale), (int)(ped.particleHeight * particle.scale))), new Rectangle(frame * ped.particleWidth, particle.option * ped.particleHeight, ped.particleWidth, ped.particleHeight), Color.White * particle.alpha, particle.rotation, new Vector2(ped.particleWidth / 2, ped.particleHeight / 2), SpriteEffects.None, drawDepth + depthOffset);
            }
        }

        public static Vector2 GetCirclePos(ParticleEffectData ped)
        {
            var angle = (float)Game1.random.NextDouble() * 2 * Math.PI;
            var distance = (float)Math.Sqrt(ped.fieldInnerRadius / ped.fieldOuterRadius + (float)Game1.random.NextDouble() * (1 - ped.fieldInnerRadius / ped.fieldOuterRadius)) * ped.fieldOuterRadius;
            return new Vector2(distance * (float)Math.Cos(angle), distance * (float)Math.Sin(angle));
        }

        public static bool IsOutOfBounds(ParticleData particle, ParticleEffectData ped, Vector2 center)
        {
            if (!ped.restrictOuter && !ped.restrictInner)
                return false;
            if (ped.fieldOuterRadius > 0)
            {
                return (ped.restrictOuter && Vector2.Distance(center, particle.position) > ped.fieldOuterRadius) || (ped.restrictInner && Vector2.Distance(center, particle.position) <= ped.fieldInnerRadius);
            }
            else
            {
                return (ped.restrictOuter && Math.Abs(particle.position.X - center.X) > ped.fieldOuterWidth / 2 || Math.Abs(particle.position.Y - center.Y) > ped.fieldOuterHeight / 2) || (ped.restrictInner && Math.Abs(particle.position.X - center.X) <= ped.fieldInnerWidth / 2 && Math.Abs(particle.position.Y - center.Y) <= ped.fieldInnerHeight / 2);
            }
        }

        private void LoadEffects()
        {
            effectDict = Game1.content.Load<Dictionary<string, ParticleEffectData>>(dictPath);
            foreach (var key in effectDict.Keys)
            {
                effectDict[key].key = key;
                effectDict[key].spriteSheet = Game1.content.Load<Texture2D>(effectDict[key].spriteSheetPath);
            }
        }


        public static ParticleEffectData CloneParticleEffect(string key, string type, string name, int x, int y, ParticleEffectData template)
        {
            return new ParticleEffectData()
            {
                key = key,
                type = type,
                name = name,
                movementType = template.movementType,
                movementSpeed = template.movementSpeed,
                frameSpeed = template.frameSpeed,
                acceleration = template.acceleration,
                restrictOuter = template.restrictOuter,
                restrictInner = template.restrictInner,
                minRotationRate = template.minRotationRate,
                maxRotationRate = template.maxRotationRate,
                particleWidth = template.particleWidth,
                particleHeight = template.particleHeight,
                fieldInnerWidth = template.fieldInnerWidth,
                fieldInnerHeight = template.fieldInnerHeight,
                fieldOuterWidth = template.fieldOuterWidth,
                fieldOuterHeight = template.fieldOuterHeight,
                minParticleScale = template.minParticleScale,
                maxParticleScale = template.maxParticleScale,
                maxParticles = template.maxParticles,
                minLifespan = template.minLifespan,
                maxLifespan = template.maxLifespan,
                spriteSheetPath = template.spriteSheetPath,
                spriteSheet = template.spriteSheet,
                fieldOffsetX = x,
                fieldOffsetY = y
            };
        }

        private void AddOption(IGenericModConfigMenuApi configApi, string name)
        {
            PropertyInfo propertyInfo = typeof(ModConfig).GetProperty(name);
            if (propertyInfo == null)
                return;

            Func<string> getName = () => I18n.GetByKey($"Config.{typeof(ModEntry).Namespace}.{name}.Name");
            Func<string> getDescription = () => I18n.GetByKey($"Config.{typeof(ModEntry).Namespace}.{name}.Description");

            if (getName == null || getDescription == null)
                return;

            if (propertyInfo.PropertyType == typeof(bool))
            {
                Func<bool> getter = () => (bool)propertyInfo.GetValue(modConfig);
                Action<bool> setter = value => propertyInfo.SetValue(modConfig, value);
                configApi.AddBoolOption(ModManifest, getter, setter, getName, getDescription);
            }
            else if (propertyInfo.PropertyType == typeof(int))
            {
                Func<int> getter = () => (int)propertyInfo.GetValue(modConfig);
                Action<int> setter = value => propertyInfo.SetValue(modConfig, value);
                configApi.AddNumberOption(ModManifest, getter, setter, getName, getDescription);
            }
            else if (propertyInfo.PropertyType == typeof(float))
            {
                Func<float> getter = () => (float)propertyInfo.GetValue(modConfig);
                Action<float> setter = value => propertyInfo.SetValue(modConfig, value);
                configApi.AddNumberOption(ModManifest, getter, setter, getName, getDescription);
            }
            else if (propertyInfo.PropertyType == typeof(string))
            {
                Func<string> getter = () => (string)propertyInfo.GetValue(modConfig);
                Action<string> setter = value => propertyInfo.SetValue(modConfig, value);
                configApi.AddTextOption(ModManifest, getter, setter, getName, getDescription);
            }
            else if (propertyInfo.PropertyType == typeof(SButton))
            {
                Func<SButton> getter = () => (SButton)propertyInfo.GetValue(modConfig);
                Action<SButton> setter = value => propertyInfo.SetValue(modConfig, value);
                configApi.AddKeybind(ModManifest, getter, setter, getName, getDescription);
            }
            else if (propertyInfo.PropertyType == typeof(KeybindList))
            {
                Func<KeybindList> getter = () => (KeybindList)propertyInfo.GetValue(modConfig);
                Action<KeybindList> setter = value => propertyInfo.SetValue(modConfig, value);
                configApi.AddKeybindList(ModManifest, getter, setter, getName, getDescription);
            }
        }
    }
}