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
}