using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FluidSimulation : MonoBehaviour
{
    [Header("Particle Settings")] 
    [Range(0.05f,1)] public float particleSize = 1f;
    public int numOfParticles;
    [Range(0,20)] public float gravity = 9.81f;
    public float mass = 1f;
    [Range(0,1)] public float spacing = 0f;
    
    [Header("Colour")]
    public Gradient speedGradient;
    public float maxSpeed = 10f;
    
    [Header("Particle Interactions")]
    [Range(0,1)] public float collisionDamping = 0.9f; // efficiencyy of collision bounces (1 = perfect elasticity, 0 = all momentum lost)
    [Range(1,3)] public float smoothingRadius; // radiuus of influence for nearby particles
    public float targetDensity = 1;
    public float pressureMultiplier = 1f; //stronger forces creates a strongeer pressure field, higher numbers are currently causing pooling at the container bounds
    [Range(0,3)] public float viscosityStrength = 0.1f; // 0 = gas-like,  0.1 = water-like, 0.5+ syrup-y
    public float surfaceTensionStrength = 0.1f;
    public float surfaceNormalThreshold = 0.1f;
    
    [Header("Mouse Interaction")]
    public float mouseInteractionRadius = 2f;
    public float mouseInteractionStrength = 5f;
    
    [Header("Setup")]
    public  Transform particlesTransform;
    public GameObject particlePrefab;
    public Container container;
    
    [Header("Boundary")]
    public float boundaryRepulsion = 150f;
    public float boundaryRepulsionDist = 1.5f;

    
    private HashSet<Particle> particles = new HashSet<Particle>();
    
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
               Vector2 pos = new Vector2((x * step) - container.boundsSize.x / 2, y * step);
                          
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
    
    // Update is called once per frame
    void Update()
    {
        // --- Pass 1: compute density for every particle ---
        foreach (Particle particle in particles)
        {
            particle.density = CalculateDensity(particle.position);
        }

        // --- Pass 2: pressure + viscosityy + gravity → velocity → position ---
        foreach (Particle particle in particles)
        {
            Vector2 pressureForce = CalculatePressureForce(particle);
            Vector2 pressureAcceleration = pressureForce / particle.density;
            Vector2 boundaryForce = BoundaryRepulsionForce(particle.position); 

            Vector2 viscosityForce = CalculateViscosityForce(particle);
            Vector2 viscosityAcceleration = viscosityForce / particle.density;
            
            Vector2 surfaceTensionForce = CalculateSurfaceTensionForce(particle);
            Vector2 surfaceTensionAcceleration = surfaceTensionForce / particle.density;
            
            particle.velocity += (Vector2.down * gravity + pressureAcceleration + viscosityAcceleration + surfaceTensionAcceleration + boundaryForce) * Time.deltaTime;
            particle.position += particle.velocity * Time.deltaTime;

            KeepInContainer(ref particle.position, ref particle.velocity);
            particle.transform.position = particle.position;
            
            ColourParticle(particle);   //colours the particle based on velocity for easier visualisation
        }
        
        // --- Pass 3: mouse interaction ---
        bool attract = Input.GetMouseButton(0);  // left click = pull in
        bool repel   = Input.GetMouseButton(1);  // right click = push out

        if (attract || repel)
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            
            float strength;

            if (attract)
            {
                strength = mouseInteractionStrength;
            }
            else
            {
                strength = -mouseInteractionStrength;
            }
            
            ApplyMouseForce(mouseWorld, strength);
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
    
    private static float ViscosityKernelLaplacian(float r, float d)
    {
        if (d >= r) return 0;

        return (45f / (Mathf.PI * Mathf.Pow(r, 6))) * (r - d);
    }

    
    Vector2 CalculateViscosityForce(Particle sampleParticle)
    {
        Vector2 viscosityForce = Vector2.zero;

        foreach (var particle in particles)
        {
            if (particle == sampleParticle) continue;

            float distance = (particle.position - sampleParticle.position).magnitude;
            float influence = ViscosityKernelLaplacian(smoothingRadius, distance);

            // https://matthias-research.github.io/pages/publications/sca03.pdf - viscosity header - vel forces are only dependent on vel DIFFERENCES not absolute vel 
            viscosityForce += mass * (particle.velocity - sampleParticle.velocity)
                / particle.density * influence;
            //particle is accelerated on average vel of its local environment
        }

        return viscosityStrength * viscosityForce;
    }

    static float SmoothingKernelDerivative(float r, float d)
    {
        if (d >= r) return 0;

        float f = r * r - d * d;
        float scale = -24 / (Mathf.PI * Mathf.Pow(r, 8));

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

    Vector2 CalculatePressureForce(Particle sampleParticle)
    {
        Vector2 pressureForce = Vector2.zero;

        foreach (var particle in particles)
        {
            if (particle == sampleParticle) continue;  // skip self

            float distance = (particle.position - sampleParticle.position).magnitude;
            if (distance == 0f) continue;  // safety

            Vector2 dir = (particle.position - sampleParticle.position) / distance;
            float slope = SmoothingKernelDerivative(smoothingRadius, distance);

            float sharedPressure = (DensityToPressure(particle.density)
                                    + DensityToPressure(sampleParticle.density)) / 2f;

            pressureForce += sharedPressure * dir * slope * mass / particle.density;
        }

        return pressureForce;
    }

    float DensityToPressure(float density)
    {
        return pressureMultiplier * (density - targetDensity);
    }
    
    void ColourParticle(Particle p)
    {
        float t = Mathf.Clamp01(p.velocity.magnitude / maxSpeed);
        p.sr.color = speedGradient.Evaluate(t);
    }

    Vector2 BoundaryRepulsionForce(Vector2 pos)
    {
        Vector2 force = Vector2.zero;
        Vector2 half = container.boundsSize / 2 - Vector2.one * particleSize;

        float lx = pos.x - (-half.x);
        if (lx < boundaryRepulsionDist)
            force.x += boundaryRepulsion * (1f - lx / boundaryRepulsionDist);

        float rx = half.x - pos.x;
        if (rx < boundaryRepulsionDist)
            force.x -= boundaryRepulsion * (1f - rx / boundaryRepulsionDist);

       // float by = pos.y - (-half.y);
       // if (by < boundaryRepulsionDist)
       //     force.y += boundaryRepulsion * (1f - by / boundaryRepulsionDist);
       //
       // float ty = half.y - pos.y;
       // if (ty < boundaryRepulsionDist)
       //     force.y -= boundaryRepulsion * (1f - ty / boundaryRepulsionDist);
       //
        return force;
    }

    // Laplacian of the poly6 color field: used to compute surface curvature
    // Formula: (45 / (π * r^6)) * (r - d)
    float SurfaceTensionLaplacian(float r, float d)
    {
        if (d >= r) return 0;
        return (45f / (Mathf.PI * Mathf.Pow(r, 6))) * (r - d);
    }
    
    void ApplyMouseForce(Vector2 mousePos, float strength)
    {
        foreach (Particle particle in particles)
        {
            Vector2 offset = mousePos - particle.position;
            float distance = offset.magnitude;
            if (distance >= mouseInteractionRadius || distance == 0f) continue;

            // Linear falloff: full strength at centre, zero at edge
            float t = 1f - (distance / mouseInteractionRadius);
            Vector2 dir = offset / distance;
            particle.velocity += dir * strength * t * Time.deltaTime;
        }
    }


// Müller 2003: surface tension via color field gradient (normal) and Laplacian (curvature)
// Force = -σ * κ * n̂, only applied at the surface where |n| exceeds threshold
    Vector2 CalculateSurfaceTensionForce(Particle sampleParticle)
    {
        Vector2 normal = Vector2.zero;
        float curvature = 0f;

        foreach (var particle in particles)
        {
            if (particle == sampleParticle) continue;

            float distance = (particle.position - sampleParticle.position).magnitude;
            if (distance == 0f || distance >= smoothingRadius) continue;

            Vector2 dir = (particle.position - sampleParticle.position) / distance;
            float weight = mass / particle.density;

            // color field gradient → surface normal
            normal += weight * SmoothingKernelDerivative(smoothingRadius, distance) * dir;

            // color field Laplacian → curvature
            curvature -= weight * SurfaceTensionLaplacian(smoothingRadius, distance);
        }

        // Only apply at the surface; interior particles have near-zero normals
        if (normal.magnitude < surfaceNormalThreshold) return Vector2.zero;

        return -surfaceTensionStrength * curvature * normal.normalized;
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
