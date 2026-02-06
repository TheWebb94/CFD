using UnityEngine;

public class Container : MonoBehaviour
{
    public Vector2 boundsSize = new Vector2(20, 20);
    public FluidSettings settings;
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        float r = settings.particleSize / 2;
        // Draw centered on THIS object
        Gizmos.DrawWireCube(transform.position, new Vector2((float)boundsSize.x - r, (float)boundsSize.y - r));
    }
}