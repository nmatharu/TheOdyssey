using UnityEngine;

public class Tile
{
    // GameObject for the base of the tile, e.g. grass, water
    public GameObject Ground;

    // World space
    public Vector3 Position;

    // Index of this gameObject in WorldGenIndex.cs (blocks)
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

    // Used temporarily to find evenly spaced points between objects-- essentially a temp OffLimits field
    public bool Spacing = false;

    // Grass, Log, Sand, Stone
    public int FootstepsIndex;

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

    public void EvalFootsteps()
    {
        if( Ground.name.Contains( "Grass" ) ) FootstepsIndex = 0;
        if( Ground.name.Contains( "Log" ) ) FootstepsIndex = 1;
        if( Ground.name.Contains( "Sand" ) ) FootstepsIndex = 2;
        if( Ground.name.Contains( "Rock" ) ) FootstepsIndex = 3;
    }

    public bool Is( WorldGenIndex.Blocks blockType ) => GenIndex == (int) blockType;
}