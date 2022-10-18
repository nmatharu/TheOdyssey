using UnityEngine;

public interface Level
{
    public const int DefaultWorldSizeX = 500;
    public const int DefaultWorldSizeY = 12;

    public abstract void Generate( Tile[ , ] tileMap );
    public abstract Vector2Int WorldSize();

}