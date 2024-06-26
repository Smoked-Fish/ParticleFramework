﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace ParticleFramework.Framework.Data
{
    public class EntityParticleData
    {
        public Dictionary<string, List<ParticleData>> particleDict = [];
    }

    public class ParticleData
    {
        public Vector2 direction;
        public int age;
        public float rotation;
        public float rotationRate;
        public int lifespan;
        public float scale;
        public float alpha;
        public int option = -1;
        public Vector2 position;
        public Vector2 originalDirection;
    }
}
