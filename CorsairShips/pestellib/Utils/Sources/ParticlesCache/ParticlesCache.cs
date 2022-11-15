using UnityEngine;
using System.Collections.Generic;

namespace PestelLib.Utils
{
    public class ParticlesCache : MonoBehaviour
    {
        private Dictionary<string, PartcilesCacheMapEntry> _cache = new Dictionary<string, PartcilesCacheMapEntry>();

        void Awake()
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                var ps = child.GetComponent<ParticleSystem>();
                _cache[child.name] = new PartcilesCacheMapEntry
                {
                    System = ps,
                    Particles = new ParticleSystem.Particle[ps.main.maxParticles]
                };
            }
        }

        public ParticlesCacheReference AddParticle(string particleSystemName, Vector3 position, Vector3 rotation, bool applyRotation, Color32 color)
        {
            ParticleSystem.EmitParams e = new ParticleSystem.EmitParams {
                position = position,
                startColor = color
            };

            if (!_cache.ContainsKey(particleSystemName))
            {
                Debug.LogError("Cache doesn't contain " + particleSystemName);
                return null;
            }

            var system = _cache[particleSystemName].System;

            system.Emit(e, 1);
            
            var particleReference = new ParticlesCacheReference() {
                Position = position,
                Rotation = rotation,
                ApplyRotation = applyRotation
            };

            _cache[particleSystemName].References.Add(particleReference);
            
            return particleReference;
        }
        
        private void LateUpdate()
        {
            foreach (var each in _cache.Values)
            {
                UpdateEntry(each);
            }
        }

        private void UpdateEntry(PartcilesCacheMapEntry entry)
        {
            var system = entry.System;

            for (var i = 0; i < entry.References.Count; ++i)
            {
                if (entry.References[i].Killed)
                {
                    FastRemove(entry.References, i);
                }
            }
            
            var particlesToSpawn = entry.References.Count - system.particleCount;
            for (var i = 0; i < particlesToSpawn; ++i)
            {
                system.Emit(1);
            }
            
            var particles = entry.Particles;

            //update props
            // GetParticles is allocation free because we reuse the m_Particles buffer between updates
            var numParticlesAlive = system.GetParticles(particles);

            // Change only the particles that are alive
            for (var i = 0; i < numParticlesAlive; i++)
            {
                if (i >= entry.References.Count)
                {
                    particles[i].remainingLifetime = -1;
                    continue;
                }
                
                var reference = entry.References[i];

                particles[i].position = reference.Position;
                particles[i].startColor = reference.StartColor;

                if (reference.ApplyStartSize)
                {
                    particles[i].startSize = reference.StartSize;
                }

                if (reference.ApplyRotation)
                {
                    particles[i].rotation3D = reference.Rotation;
                }
            }
            
            // Apply the particle changes to the particle system
            system.SetParticles(particles, numParticlesAlive);
        }

        private void FastRemove<T>(List<T> list, int index)
        {
            var lastItem = list[list.Count - 1];
            list[index] = lastItem;
            list.RemoveAt(list.Count - 1);
        }
    }
}