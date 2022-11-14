using UnityEngine;

public abstract class Level : MonoBehaviour
{
    [ SerializeField ] GameObject baseBlock;
    [ SerializeField ] protected Transform environmentalParent;
    
    protected const int DefaultWorldSizeX = 400;
    protected const int DefaultWorldSizeY = 12;

    public abstract void Generate( WorldGenerator generator );
    public Vector2Int WorldSize() => new( DefaultWorldSizeX, DefaultWorldSizeY );

}