using ParticleFramework.Framework.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ParticleFramework.Framework.Managers;

namespace ParticleFramework.Framework.Api
{
    public interface IParticleEffectAPI
    {
        public void BeginFarmerParticleEffect(long farmerID, string key);
        public void EndFarmerParticleEffect(long farmerID, string key);
        public void BeginNPCParticleEffect(string npc, string key);
        public void EndNPCParticleEffect(string npc, string key);
        public void BeginLocationParticleEffect(string location, int x, int y, string key);
        public void EndLocationParticleEffect(string location, int x, int y, string key);
        public List<string> GetEffectNames();
    }
    public class ParticleEffectsAPI
    {

        public void BeginFarmerParticleEffect(long farmerID, string key)
        {
            if (!ParticleEffectManager.farmerDict.ContainsKey(farmerID))
                ParticleEffectManager.farmerDict.Add(farmerID, new List<string>());
            if (!ParticleEffectManager.farmerDict[farmerID].Contains(key))
                ParticleEffectManager.farmerDict[farmerID].Add(key);
        }
        public void EndFarmerParticleEffect(long farmerID, string key)
        {
            if (ParticleEffectManager.farmerDict.ContainsKey(farmerID))
                ParticleEffectManager.farmerDict[farmerID].Remove(key);
        }
        public void BeginNPCParticleEffect(string npc, string key)
        {
            if (!ParticleEffectManager.NPCDict.ContainsKey(npc))
                ParticleEffectManager.NPCDict.Add(npc, new List<string>());
            if (!ParticleEffectManager.NPCDict[npc].Contains(key))
                ParticleEffectManager.NPCDict[npc].Add(key);
        }
        public void EndNPCParticleEffect(string npc, string key)
        {
            if (ParticleEffectManager.NPCDict.ContainsKey(npc))
                ParticleEffectManager.NPCDict[npc].Remove(key);
        }
        public void BeginScreenParticleEffect(string name, int x, int y, string key)
        {
            if (!ParticleEffectManager.effectDict.TryGetValue(key, out ParticleEffectData template))
                return;
            Point position = new Point(x, y);
            if (!ParticleEffectManager.screenDict.ContainsKey(position))
                ParticleEffectManager.screenDict[position] = new List<ParticleEffectData>();
            if (!ParticleEffectManager.screenDict[position].Exists(d => d.key == key))
            {
                ParticleEffectData ped = ParticleEffectManager.CloneParticleEffect(key, "screen", name, x, y, template);
                ParticleEffectManager.screenDict[position].Add(ped);

            }
        }
        public void EndScreenParticleEffect(string location, int x, int y, string key)
        {
            Point position = new Point(x, y);
            if (ParticleEffectManager.screenDict.ContainsKey(position))
            {
                for (int i = ParticleEffectManager.screenDict[position].Count - 1; i >= 0; i--)
                {
                    if (ParticleEffectManager.screenDict[position][i].key == key)
                        ParticleEffectManager.screenDict[position].RemoveAt(i);
                }
            }
        }
        public void BeginLocationParticleEffect(string location, int x, int y, string key)
        {
            if (!ParticleEffectManager.effectDict.TryGetValue(key, out ParticleEffectData template))
                return;
            if (!ParticleEffectManager.locationDict.ContainsKey(location))
                ParticleEffectManager.locationDict.Add(location, new Dictionary<Point, List<ParticleEffectData>>());
            Point position = new Point(x, y);
            if (!ParticleEffectManager.locationDict[location].ContainsKey(position))
                ParticleEffectManager.locationDict[location][position] = new List<ParticleEffectData>();
            if (!ParticleEffectManager.locationDict[location][position].Exists(d => d.key == key))
            {
                ParticleEffectData ped = ParticleEffectManager.CloneParticleEffect(key, "location", location, x, y, template);
                ParticleEffectManager.locationDict[location][position].Add(ped);

            }
        }
        public void EndLocationParticleEffect(string location, int x, int y, string key)
        {
            Point position = new Point(x, y);
            if (ParticleEffectManager.locationDict.ContainsKey(location) && ParticleEffectManager.locationDict[location].ContainsKey(position))
            {
                for (int i = ParticleEffectManager.locationDict[location][position].Count - 1; i >= 0; i--)
                {
                    if (ParticleEffectManager.locationDict[location][position][i].key == key)
                        ParticleEffectManager.locationDict[location][position].RemoveAt(i);
                }
            }
        }
        public List<string> GetEffectNames()
        {
            return ParticleEffectManager.effectDict.Keys.ToList();
        }
    }
}
