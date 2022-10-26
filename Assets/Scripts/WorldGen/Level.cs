using UnityEngine;

public interface Level
{
    public const int DefaultWorldSizeX = 400;
    public const int DefaultWorldSizeY = 12;

    public abstract void Generate( WorldGenerator generator );
    public abstract Vector2Int WorldSize();

}