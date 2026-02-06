using UnityEngine;

public class FluidSettings : MonoBehaviour
{
    public float gravity = 9.81f;
    [Range(0f, 1f)] public float collisionDamping = 0.9f;
    public float particleSize = 1f;
   
    [HideInInspector] public int version;

    private void OnValidate()
    {
        version++;
    }
}
