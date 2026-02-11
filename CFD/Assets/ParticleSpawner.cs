using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    [Header("Container Configuration")]
    public int particlesToSpawn = 100;
    public GameObject particlePrefab;
    public Transform particleParent;
    public Container container; 
    public FluidSettings settings;
    public float spacing = 0; // distance between particles
    public ParticleManager particles;

    void Start()
    {
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(particlesToSpawn));
        int spawned = 0;
        
        float totalSpacing = spacing + settings.particleSize;

        Vector2 startOffset = new Vector2(
            -gridSize * spacing * 0.5f,
            -gridSize * spacing * 0.5f
        );

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (spawned >= particlesToSpawn)
                    return;

                Vector2 pos = startOffset + new Vector2(x * totalSpacing, y * totalSpacing);

                GameObject obj = Instantiate(
                    particlePrefab,
                    pos,
                    Quaternion.identity,
                    particleParent
                );

                FluidParticle fluidParticle = obj.GetComponent<FluidParticle>();
                fluidParticle.container = container;
                fluidParticle.settings = settings;
                particles.particleSet.Add(fluidParticle);
                
                spawned++;
            }
        }
    }
}