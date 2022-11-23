using System;
using UnityEngine;

public abstract class Level : MonoBehaviour
{
    [ SerializeField ] protected GameObject baseBlock;
    [ SerializeField ] protected Transform environmentalParent;
    
    protected const int DefaultWorldSizeX = 400;
    protected const int DefaultWorldSizeY = 12;

    protected const int PlayerStartZone = 20;
    protected const int BossZoneOffset = 70;

    protected int CoreLength;
    protected int BiomeAStart;
    protected int BiomeBStart;
    protected int BiomeCStart;

    void Awake()
    {
        CoreLength = DefaultWorldSizeX - PlayerStartZone - BossZoneOffset;
        BiomeAStart = (int) ( PlayerStartZone + 1 );
        BiomeBStart = (int) ( PlayerStartZone + CoreLength / 3f + 1 );
        BiomeCStart = (int) ( PlayerStartZone + 2 * CoreLength / 3f + 1 );
    }

    public abstract void Generate( WorldGenerator generator );
    public Vector2Int WorldSize() => new( DefaultWorldSizeX, DefaultWorldSizeY );

}