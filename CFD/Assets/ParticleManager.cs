using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public HashSet<FluidParticle>  particleSet = new HashSet<FluidParticle>();



    public int particleCount;

    void Update()
    {
        particleCount = particleSet.Count;
    }
    
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Pass 1: densities
        foreach (var p in particleSet)
            p.FindDensity();

        // Pass 2: pressures
        foreach (var p in particleSet)
            p.ComputePressure();

        // Pass 3: forces + integrate
        foreach (var p in particleSet)
            p.Step(dt);
    }

}