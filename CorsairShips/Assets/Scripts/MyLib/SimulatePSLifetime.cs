using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class SimulatePSLifetime : MonoBehaviour {
    public float Seconds;
    void Start() {
        var ps = GetComponent<ParticleSystem>();
        ps.Simulate(Seconds);
        ps.Play();
    }
}
