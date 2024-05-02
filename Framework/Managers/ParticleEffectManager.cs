using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ParticleFramework.Framework.Data;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Object = StardewValley.Object;

namespace ParticleFramework.Framework.Managers
{
    internal class ParticleEffectManager
    {
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

        public static void ShowParticleEffect(SpriteBatch spriteBatch, List<ParticleData> particles, ParticleEffectData effectData, Vector2 center, float drawDepth, bool isScreenSpace = false)
        {
            foreach (var particle in particles.ToArray())
            {
                UpdateParticle(particles, particle, effectData, center);

                if (IsOutOfBounds(particle, effectData, center))
                {
                    particles.Remove(particle);
                    continue;
                }

                DrawParticle(spriteBatch, particle, effectData, drawDepth, isScreenSpace);
            }

            AddNewParticles(particles, effectData, center);
        }

        private static void UpdateParticle(List<ParticleData> particles, ParticleData particle, ParticleEffectData effectData, Vector2 center)
        {
            particle.age++;

            if (particle.age > particle.lifespan)
            {
                particles.Remove(particle);
                return;
            }

            if (particle.direction == Vector2.Zero)
            {
                SetParticleDirection(particle, effectData, center);
            }

            particle.position += particle.direction * (effectData.movementSpeed + effectData.acceleration * particle.age);
            particle.rotation += particle.rotationRate;
        }

        private static void SetParticleDirection(ParticleData particle, ParticleEffectData effectData, Vector2 center)
        {
            Vector2 direction = Vector2.Zero;

            switch (effectData.movementType)
            {
                case "away":
                    direction = (center - particle.position) * new Vector2(-1, -1);
                    break;
                case "towards":
                    direction = center - particle.position;
                    break;
                case "up":
                    direction = -Vector2.UnitY;
                    break;
                case "down":
                    direction = Vector2.UnitY;
                    break;
                case "left":
                    direction = -Vector2.UnitX;
                    break;
                case "right":
                    direction = Vector2.UnitX;
                    break;
                case "random":
                    direction = new Vector2((float)Game1.random.NextDouble() - 0.5f, (float)Game1.random.NextDouble() - 0.5f);
                    break;
            }

            direction.Normalize();
            particle.direction = direction;
        }

        private static void DrawParticle(SpriteBatch spriteBatch, ParticleData particle, ParticleEffectData effectData, float drawDepth, bool isScreenSpace)
        {
            int frame = (int)Math.Round(particle.age * effectData.frameSpeed) % (effectData.spriteSheet.Width / effectData.particleWidth);

            float depthOffset = GetDepthOffset(effectData);

            spriteBatch.Draw(
                effectData.spriteSheet,
                new Rectangle(
                    Utility.Vector2ToPoint(isScreenSpace ? particle.position : Game1.GlobalToLocal(particle.position)),
                    new Point((int)(effectData.particleWidth * particle.scale), (int)(effectData.particleHeight * particle.scale))
                ),
                new Rectangle(
                    frame * effectData.particleWidth,
                    particle.option * effectData.particleHeight,
                    effectData.particleWidth,
                    effectData.particleHeight
                ),
                Color.White * particle.alpha,
                particle.rotation,
                new Vector2(effectData.particleWidth / 2, effectData.particleHeight / 2),
                SpriteEffects.None,
                drawDepth + depthOffset
            );
        }

        private static float GetDepthOffset(ParticleEffectData effectData)
        {
            float depthOffset = effectData.belowOffset >= 0 ? (effectData.aboveOffset >= 0 ? (Game1.random.NextDouble() < 0.5 ? effectData.aboveOffset : effectData.belowOffset) : effectData.belowOffset) : effectData.aboveOffset;
            return depthOffset;
        }

        private static void AddNewParticles(List<ParticleData> particles, ParticleEffectData effectData, Vector2 center)
        {
            if (particles.Count < effectData.maxParticles && Game1.random.NextDouble() < effectData.particleChance)
            {
                var newParticle = GenerateParticle(effectData, center);
                particles.Add(newParticle);
            }
        }

        private static ParticleData GenerateParticle(ParticleEffectData effectData, Vector2 center)
        {
            var newParticle = new ParticleData();
            newParticle.lifespan = Game1.random.Next(effectData.minLifespan, effectData.maxLifespan + 1);
            newParticle.scale = effectData.minParticleScale + (float)Game1.random.NextDouble() * (effectData.maxParticleScale - effectData.minParticleScale);
            newParticle.alpha = effectData.minAlpha + (float)Game1.random.NextDouble() * (effectData.maxAlpha - effectData.minAlpha);
            newParticle.rotationRate = effectData.minRotationRate + (float)Game1.random.NextDouble() * (effectData.maxRotationRate - effectData.minRotationRate);
            newParticle.option = Game1.random.Next(effectData.spriteSheet.Height / effectData.particleHeight);

            if (effectData.fieldOuterRadius <= 0)
            {
                newParticle.position = center - new Vector2(effectData.fieldOuterWidth, effectData.fieldOuterHeight) / 2 + GetRandomOffset(effectData);
            }
            else
            {
                newParticle.position = center + GetCirclePos(effectData);
            }

            return newParticle;
        }

        private static Vector2 GetRandomOffset(ParticleEffectData effectData)
        {
            double x;
            double y;

            if (effectData.fieldInnerHeight > 0)
            {
                var innerTop = (effectData.fieldOuterHeight - effectData.fieldInnerHeight) / 2;
                var innerBottom = effectData.fieldOuterHeight - innerTop;
                var innerLeft = (effectData.fieldOuterWidth - effectData.fieldInnerWidth) / 2;
                var innerRight = effectData.fieldOuterWidth - innerLeft;

                // Calculate a random point inside the inner field
                int pixel = (int)((effectData.fieldOuterWidth * innerTop * 2 + effectData.fieldInnerHeight * innerLeft * 2) * Game1.random.NextDouble());

                if (pixel >= effectData.fieldOuterWidth * innerTop + effectData.fieldInnerHeight * innerLeft * 2) // bottom
                {
                    pixel = pixel - effectData.fieldOuterWidth * innerTop - effectData.fieldInnerHeight * innerLeft * 2;
                    x = pixel % effectData.fieldOuterWidth;
                    y = innerBottom + pixel / effectData.fieldOuterWidth;
                }
                else if (pixel >= effectData.fieldOuterWidth * innerTop + effectData.fieldInnerHeight * innerLeft) // right
                {
                    pixel = pixel - effectData.fieldOuterWidth * innerTop - effectData.fieldInnerHeight * innerLeft;
                    x = innerRight + pixel % innerLeft;
                    y = innerTop + pixel / innerLeft;
                }
                else if (pixel >= effectData.fieldOuterWidth * innerTop) // left
                {
                    pixel = pixel - effectData.fieldOuterWidth * innerTop;
                    x = pixel % innerLeft;
                    y = innerTop + pixel / innerLeft;
                }
                else // top
                {
                    x = pixel % effectData.fieldOuterWidth;
                    y = pixel / effectData.fieldOuterWidth;
                }
            }
            else
            {
                x = effectData.fieldOuterWidth * Game1.random.NextDouble();
                y = effectData.fieldOuterHeight * Game1.random.NextDouble();
            }

            return new Vector2((float)x, (float)y);
        }

        private static Vector2 GetCirclePos(ParticleEffectData ped)
        {
            var angle = (float)Game1.random.NextDouble() * 2 * Math.PI;
            var distance = (float)Math.Sqrt(ped.fieldInnerRadius / ped.fieldOuterRadius + (float)Game1.random.NextDouble() * (1 - ped.fieldInnerRadius / ped.fieldOuterRadius)) * ped.fieldOuterRadius;
            return new Vector2(distance * (float)Math.Cos(angle), distance * (float)Math.Sin(angle));
        }

        private static bool IsOutOfBounds(ParticleData particle, ParticleEffectData ped, Vector2 center)
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

        public void LoadEffects()
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
    }
}
