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

    // Only used internally for world generations-- for example, logs have adjacent spaces 
    // off limits else players would not be able to move onto or off of them
    public bool OffLimits = false;

    public Tile()
    {
    }

    public Tile( GameObject o, int x, int y, int i, Vector3 pos, bool emptyCollider )
    {
        Ground = o;
        X = x;
        Y = y;
        GenIndex = i;
        Position = pos;
        EmptyCollider = emptyCollider;
    }
}