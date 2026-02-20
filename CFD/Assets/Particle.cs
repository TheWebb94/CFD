using UnityEngine;

public class Particle : MonoBehaviour
{
    public Vector2 position;
    public Vector2 velocity;
    public float density;
    
    [HideInInspector] public SpriteRenderer sr;   // ADD THIS

    void Awake()                                   // ADD THIS
    {
        sr = GetComponent<SpriteRenderer>();
    }
}
