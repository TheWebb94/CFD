using UnityEngine;

public class ParticleSpawner : MonoBehaviour
{

    public float particlesToSpawn;
    public GameObject particlePrefab;

    public Transform particleParent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        float particleGridDimensions = Mathf.Sqrt(particlesToSpawn);

        for (int x = 0; x < particlesToSpawn; x++)
        {
            for (int y = 0; y < particlesToSpawn; y++)
            {
                Instantiate(particlePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
