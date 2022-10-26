using UnityEngine;

public class Tile
{
    // GameObject for the base of the tile, e.g. grass, water
    public GameObject Ground;

    // World space
    public Vector3 Position;

    // Index of this gameObject in WorldGenIndex.cs
    public int GenIndex;
    
    // Grid space
    public int X;
    public int Y;

    // Used for determining whether or not Node is traversable in A*
    public bool HasSurfaceObject = false;

    // Blocks like water, have colliders for players that they cannot walk through but
    // flying enemies will be able to traverse-- affects A* pathfinding of those enemies
    public bool EmptyCollider = false;
}