using ParticleFramework.Framework.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParticleFramework.Framework.Managers;
using StardewModdingAPI;
using StardewValley;
using System.Threading;

namespace ParticleFramework.Framework.Api
{
    public interface IParticleFrameworkApi
    {
        public void AddCustomDictPath(string customDictPath);
        public void LoadEffect(ParticleEffectData effectData);
        public void UnloadEffect(string key);
        public List<string> GetEffectNames();
    }
    public class ParticleFrameworkApi
    {
        public void AddCustomDictPath(string customDictPath)
        {
            ParticleEffectManager.dictPaths.Add(customDictPath);
        }
        public void LoadEffect(ParticleEffectData effectData)
        {
            try
            {
                effectData.spriteSheet = ModEntry.modHelper.ModContent.Load<Texture2D>(effectData.spriteSheetPath);
                ParticleEffectManager.effectDict[effectData.key] = effectData;
            }
            catch (Exception e)
            {
                ModEntry.monitor.Log($"Error loading particle effect with key '{effectData.key}': {e}", LogLevel.Error);
                ParticleEffectManager.UnloadEffect(effectData.key);
            }
        }

        public void UnloadEffect(string key)
        {
            try
            {
                if (ParticleEffectManager.effectDict.ContainsKey(key))
                {
                    ParticleEffectManager.effectDict.Remove(key);
                    ModEntry.monitor.Log($"Successfully unloaded particle effect '{key}'.", LogLevel.Warn);
                }
                else
                {
                    ModEntry.monitor.Log($"Error unloading particle effect '{key}': Not found in the dictionary.", LogLevel.Warn);
                }
            }
            catch (Exception e)
            {
                ModEntry.monitor.Log($"Error unloading particle effect '{key}': {e}", LogLevel.Error);
            }
        }
        public List<string> GetEffectNames()
        {
            return ParticleEffectManager.effectDict.Keys.ToList();
        }
    }
}
