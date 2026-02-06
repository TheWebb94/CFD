using System;
using UnityEngine;

public class FluidSimulation : MonoBehaviour
{
    public float gravity = 9.81f;
    private Vector2 position;
    public Vector2 velocity;
    public Vector2 pressure; //YAGNIIIIIIIIIIIIIIIII
    private float mass = 1f;
    private float particleSize = 1f;
    private Vector2 boundsSize = new Vector2(50, 50);
    public float collisionDamping;
    private SpriteRenderer spriteRenderer;


    private void Start()
    {
        transform.localScale = new Vector3(particleSize, particleSize, 1);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        velocity = new Vector2(velocity.x, velocity.y + gravity * Time.deltaTime);
        position = new Vector2(transform.position.x, transform.position.y);
        KeepInContainer();
        transform.position = (position + velocity * Time.deltaTime);
    }
    
    void KeepInContainer()
    {
        Vector2 halfBoundsSize = boundsSize / 2 - Vector2.one * particleSize;

        if (position.x > halfBoundsSize.x)
        {
            position.x = halfBoundsSize.x;
            velocity.x = -velocity.x * collisionDamping;
        }

        if (position.x < -halfBoundsSize.x)
        {
            position.x = -halfBoundsSize.x;
            velocity.x = velocity.x * collisionDamping;
        }
        
        if (position.y < -halfBoundsSize.y)
        {
            position.y = -halfBoundsSize.y;
            velocity.y = -velocity.y * collisionDamping;
        }
        if (position.y > halfBoundsSize.y)
        {
            position.y = halfBoundsSize.y;
            velocity.y = velocity.y * collisionDamping;
        }
    }
}
