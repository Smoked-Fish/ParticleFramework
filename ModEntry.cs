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
using System.Linq;
using System.Reflection;

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
            helper.Events.GameLoop.GameLaunched += OnGameLaunched;
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
            if (modConfig.EnableMod)
            {
                foreach (var path in ParticleEffectManager.dictPaths)
                {
                    if (e.NameWithoutLocale.IsEquivalentTo(path))
                    {
                        e.LoadFrom(() => new Dictionary<string, ParticleEffectData>(), AssetLoadPriority.Exclusive);
                    }
                }
            }
        }

        private void OnRenderedHud(object sender, RenderedHudEventArgs e)
        {
            if (!modConfig.EnableMod)
                return;
            foreach (var kvp in ParticleEffectManager.screenDict)
            {
                foreach (var effect in kvp.Value)
                {
                    ParticleEffectManager.ShowScreenParticleEffect(e.SpriteBatch, effect);
                }
            }
            foreach (var key in ParticleEffectManager.effectDict.Keys)
            {
                var ped = ParticleEffectManager.effectDict[key];
                switch (ped.type.ToLower())
                {
                    case "screen":
                        ParticleEffectManager.ShowScreenParticleEffect(e.SpriteBatch, ped);
                        break;
                }
            }
        }

        private void OnRenderedWorld(object sender, RenderedWorldEventArgs e)
        {
            if (!modConfig.EnableMod)
                return;
            if (ParticleEffectManager.locationDict.TryGetValue(Game1.currentLocation.Name, out Dictionary<Point, List<ParticleEffectData>> dict))
            {
                foreach (var kvp in dict)
                {
                    foreach (var effect in kvp.Value)
                    {
                        ParticleEffectManager.ShowLocationParticleEffect(e.SpriteBatch, Game1.currentLocation, effect);
                    }
                }
            }
            foreach (var key in ParticleEffectManager.effectDict.Keys)
            {
                var ped = ParticleEffectManager.effectDict[key];
                switch (ped.type.ToLower())
                {
                    case "location":
                        if (Game1.currentLocation.Name == ped.name)
                            ParticleEffectManager.ShowLocationParticleEffect(e.SpriteBatch, Game1.currentLocation, ped);
                        break;
                }
            }
        }

        public override object GetApi()
        {
            return new ParticleFrameworkApi();
        }

        private void OnTimeChanged(object sender, TimeChangedEventArgs e)
        {
            ParticleEffectManager.LoadEffects();
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            ParticleEffectManager.LoadEffects();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            ParticleEffectManager.LoadEffects();
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