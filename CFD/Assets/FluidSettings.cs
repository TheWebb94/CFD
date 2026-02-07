using UnityEngine;

public class FluidSettings : MonoBehaviour
{
    public float gravity = -9.81f;
    [Range(0f, 1f)] public float collisionDamping = 0.9f; //elasticity of the collisions (1 = perfect elasticity)
    [Range(0.01f, 4f)] public float particleSize = 1f;
    public float smoothingFactor = 4f; // zone of influence for detecting other particles to interact with
    private float smoothingRadius;
    [HideInInspector] public int version;


    void Start()
    {
        smoothingRadius = particleSize * smoothingFactor;
        version = 0;
    }
    private void OnValidate()
    {
        version++;
    }
}
