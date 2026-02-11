using Unity.VisualScripting;
using UnityEngine;

public class FluidParticle : MonoBehaviour
{
    
    [Header("Properties")]
    private Vector2 position;
    public Vector2 velocity;
    
    [Header("References")]
    public FluidSettings settings;
    private int lastSettingsVersion = -1;
    public Container container;
    private Vector2 pressure;
    private float density;
    private ParticleManager particleManager;

    private void Start()
    {
        position = transform.position;
        ApplySettingsIfChanged();
        particleManager = FindAnyObjectByType(typeof(ParticleManager)) as ParticleManager;
    }
    
    void Update()
    {
        //velocity = new Vector2(velocity.x, velocity.y + gravity * Time.deltaTime);
        //position = new Vector2(transform.position.x, transform.position.y);
        //KeepInContainer();
        //transform.position = (position + velocity * Time.deltaTime);
        
       
        
        
        ApplySettingsIfChanged();
        
        float dt =  Time.deltaTime;
        
        velocity += Vector2.down * settings.gravity * dt;
        
        position += velocity * dt;
        KeepInContainer();
        
        transform.position = position;

        FindDensity();
    }

    private void FindDensity()
    {
       foreach (FluidParticle particle in particleManager.particleSet)
       {
           //compare positions of all particles
       }
    }

    static float SmoothingKernel(float radius, float dist)
    {
        //smoothing radius equation y = -x + smoothing radius
             // if smoothing radius = >4, y = 0
             //grid cell width of 2
             float value = 0f;
             return value;
    }
    
    private void ApplySettingsIfChanged()
    {
        if (settings == null) return;
        if (settings.version == lastSettingsVersion) return;
        lastSettingsVersion = settings.version;
    
        // Only run when something changed
        transform.localScale = Vector3.one * settings.particleSize;
    }
    void KeepInContainer()
    {
        float r = settings.particleSize;
        Vector2 boundsSize = container.boundsSize;
        Vector2 halfBoundsSize = boundsSize / 2;
        float collisionDamping = settings.collisionDamping;

        if (position.x > halfBoundsSize.x - r)
        {
            position.x = halfBoundsSize.x - r;
            velocity.x = -velocity.x * collisionDamping;
        }

        if (position.x < -halfBoundsSize.x + r)
        {
            position.x = -halfBoundsSize.x + r;
            velocity.x = -velocity.x * collisionDamping;
        }
        
        if (position.y < -halfBoundsSize.y + r)
        {
            position.y = -halfBoundsSize.y + r;
            velocity.y = -velocity.y * collisionDamping;
        }
        if (position.y > halfBoundsSize.y - r)
        {
            position.y = halfBoundsSize.y - r;
            velocity.y = -velocity.y * collisionDamping;
        }
    }
}
