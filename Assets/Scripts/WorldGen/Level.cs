using UnityEngine;

public abstract class Level : MonoBehaviour
{
    [ SerializeField ] GameObject baseBlock;
    
    protected const int DefaultWorldSizeX = 200;
    protected const int DefaultWorldSizeY = 12;

    public abstract void Generate( WorldGenerator generator );
    public Vector2Int WorldSize() => new( DefaultWorldSizeX, DefaultWorldSizeY + 1 );

}