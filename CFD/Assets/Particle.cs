using UnityEngine;

public class Particle : MonoBehaviour
{
    public float gravity = 9.81f;
    private Vector2 position;
    public Vector2 velocity;
    public Vector2 pressure;
    private float mass = 1f;
    
    void Update()
    {
        velocity = new Vector2(velocity.x, velocity.y + gravity * Time.deltaTime);
        position = new Vector2(transform.position.x, transform.position.y);
        
        transform.position = (position + velocity * Time.deltaTime);
    }
}
