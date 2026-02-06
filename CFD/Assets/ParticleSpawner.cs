using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{
    [Header("Container Configuration")]
    public int particlesToSpawn = 100;
    public GameObject particlePrefab;
    public Transform particleParent;
    public Container container; 
    public FluidSettings settings;
    public float spacing; // distance between particles

    void Start()
    {
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(particlesToSpawn));
        int spawned = 0;
        spacing = settings.particleSize;

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

                Vector2 pos = startOffset + new Vector2(x * spacing, y * spacing);

                GameObject obj = Instantiate(
                    particlePrefab,
                    pos,
                    Quaternion.identity,
                    particleParent
                );

                Particle particle = obj.GetComponent<Particle>();
                particle.container = container;
                particle.settings = settings;
                
                spawned++;
            }
        }
    }
}
