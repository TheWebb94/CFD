using System.Collections.Generic;
using UnityEngine;

public class FluidSimulation : MonoBehaviour
{
    [Header("Particle Settings")] 
    public float particleSize = 1f;
    public int numOfParticles;
    public float gravity = 9.81f;
    public float mass = 1f;
    public float collisionDamping = 0.9f;
    public float spacing = 0f;
    public float smoothingRadius;
  
   private HashSet<Particle> particles = new HashSet<Particle>();
    public  Transform particlesTransform;
    public GameObject particlePrefab;

    public Container container;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        int gridDimensions = Mathf.CeilToInt(Mathf.Sqrt(numOfParticles));
        int spawned = 0;

        float step = particleSize + spacing; //distance between each particle in spawn grid
        
        //spawn all particles in a uniform grid
       for (int x = 0; x < gridDimensions && spawned < numOfParticles; x++)
       {
           for (int y = 0; y < gridDimensions && spawned < numOfParticles; y++)
           {
               Vector2 pos = new Vector2(x * step, y * step);
                          
               GameObject obj = Instantiate(particlePrefab, pos, particlesTransform.rotation, particlesTransform);
               
               obj.transform.localScale = new Vector3(particleSize, particleSize, 1f);
               
               var p = obj.GetComponent<Particle>();
               
               p.position = pos;
               p.velocity = Vector2.zero;
               
               particles.Add(p); //add to hashset for tracking and updating later
               
               spawned++; //++
           }
       }
    }

    /// <summary>
    /// Function that determines the influence on pressure of nearby particles
    /// </summary>
    /// <param name="r">smoothing radius of particle</param>
    /// <param name="d">distance between particles</param>
    /// <returns></returns>
    private float SmoothingKernel(float r, float d)
    {
        float volume = Mathf.PI * Mathf.Pow(r, 8) / 4;
        float value = Mathf.Max(0, r * r - d * d);

        return value * value * value / volume;
    }

    static float SmoothingKernelDerivative(float r, float d)
    {
        if (d >= r) return 0;

        float f = r * r - d * d;
        float scale = -24 / Mathf.PI * Mathf.Pow(r, 8);
        return scale * d * f * f;
    }

    private float CalculateDensity(Vector2 position)
    {
        float density = 0;
        
        //Loop oover all particle positions 
        //TODO optimise to only check particles within smoothing radius
        foreach (var particle in particles)
        {
            float distance = (position - particle.position).magnitude;
            float influence = SmoothingKernel(smoothingRadius, distance);
            
            density += mass  *  influence;
        }
        return density;
    }

    /*
    float CalculateProperty(Vector2 position)
    {
        float pressureMagnitude = 0;

        foreach (var particle in particles)
        {
            float distance = (particle.position - position).magnitude;
            float influence = SmoothingKernel(smoothingRadius, distance);
            float density = CalculateDensity(particle.position);
            pressureMagnitude += particle.density * influence * mass / density;
        }
        
        return pressureMagnitude;
    }
    */

    Vector2 CalculatePressureMagnitude(Vector2 position)
    {
        Vector2 pressureMagnitude = Vector2.zero;

        foreach (var particle in particles)
        {
            float distance = (particle.position - position).magnitude;
            Vector2 dir = (particle.position - position) / distance;
            float influence = SmoothingKernel(smoothingRadius, distance);
            float density = CalculateDensity(particle.position);
            pressureMagnitude += -particle.density * dir * influence * mass / density;
        }
        
        return pressureMagnitude;
    }

    // Update is called once per frame
    void Update()
    {
        
        //loop through all particles and update pos and vel
        foreach (Particle particle in particles)
        {
            particle.velocity += Vector2.down * gravity * Time.deltaTime;
            particle.position += particle.velocity * Time.deltaTime;
            
            KeepInContainer(ref particle.position, ref particle.velocity);
            
            particle.transform.position = particle.position;
        }
    }
    
    void KeepInContainer(ref Vector2 pos, ref Vector2 vel)
    {
        
        float r = particleSize;
        Vector2 boundsSize = container.boundsSize;
        Vector2 halfBoundsSize = boundsSize / 2;

        if (pos.x > halfBoundsSize.x - r)
        {
            pos.x = halfBoundsSize.x - r;
            vel.x = -vel.x * collisionDamping;
        }

        if (pos.x < -halfBoundsSize.x + r)
        {
            pos.x = -halfBoundsSize.x + r;
            vel.x = -vel.x * collisionDamping;
        }
    
        if (pos.y < -halfBoundsSize.y + r)
        {
            pos.y = -halfBoundsSize.y + r;
            vel.y = -vel.y * collisionDamping;
        }
        if (pos.y > halfBoundsSize.y - r)
        {
            pos.y = halfBoundsSize.y - r;
            vel.y = -vel.y * collisionDamping;
        }
    }
}
