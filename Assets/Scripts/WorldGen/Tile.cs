using UnityEngine;

public class Tile
{
    // GameObject for the base of the tile, e.g. grass, water
    GameObject _ground;

    // World space
    Vector3 _position;
    
    // Grid space
    int _x;
    int _y;

    // Used for determining whether or not Node is traversable in A*
    bool _hasSurfaceObject;
}