using UnityEngine;
using System.Collections.Generic;

namespace PestelLib.Utils
{
    public class PartcilesCacheMapEntry
    {
        public ParticleSystem System;
        public ParticleSystem.Particle[] Particles;
        public List<ParticlesCacheReference> References = new List<ParticlesCacheReference>();
    }
}
