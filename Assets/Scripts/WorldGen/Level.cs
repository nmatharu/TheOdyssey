using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Level : MonoBehaviour
{
    // enemies in this level that don't warrant introduction, will be added to as enemies are intro'd
    [ SerializeField ] List<Enemy> seenEnemies;
    [ SerializeField ] List<Enemy> newEnemies; // enemies new to this stage
    [ SerializeField ] GameObject bossSpawner;  // spawner object that will introduce the boss

    // x - the x position at which this wave will spawn in terms of a percent of the core length
    // (e.g. 0.5 -> enemies will spawn at PlayerStartZone + 0.5 * CoreLength ) 
    // y - the total budget for which to spawn enemies defined by their spawn costs
    // if y is 0, then this wave is an INTRO WAVE, and a new enemy will be spawned 
    [ SerializeField ] Vector2[] enemyWaves;
    
    // Queues of some of the serialized fields above, can't serialize queues ):
    readonly Queue<Enemy> _enemiesToIntro = new();
    readonly Queue<Vector2> _waveQueue = new();

    [ SerializeField ] protected GameObject baseBlock;
    [ SerializeField ] protected Transform environmentalParent;

    protected const int DefaultWorldSizeX = 400;
    protected const int DefaultWorldSizeY = 12;

    protected const int PlayerStartZone = 30;
    protected const int BossZoneOffset = 70;
    protected const float BossZoneSize = 18f;

    protected int CoreLength;
    protected int BiomeAStart;
    protected int BiomeBStart;
    protected int BiomeCStart;
    protected int[] DefaultShopPlacements;
    protected int DefaultMagicPlacement;

    void Awake()
    {
        CoreLength = DefaultWorldSizeX - PlayerStartZone - BossZoneOffset;
        BiomeAStart = (int) ( PlayerStartZone + 1 );
        BiomeBStart = (int) ( PlayerStartZone + CoreLength / 3f + 1 );
        BiomeCStart = (int) ( PlayerStartZone + 2 * CoreLength / 3f + 1 );

        DefaultShopPlacements = new[]
        {
            (int) ( 2/10f * CoreLength + PlayerStartZone ),
            (int) ( 5/10f * CoreLength + PlayerStartZone ),
            (int) ( 7/10f * CoreLength + PlayerStartZone ),
            DefaultWorldSizeX - BossZoneOffset - 6
        };
        DefaultMagicPlacement = (int) ( PlayerStartZone + 2/5f * CoreLength );
    }

    void Start()
    {
        foreach( var e in newEnemies )
            _enemiesToIntro.Enqueue( e );
        foreach( var w in enemyWaves )
            _waveQueue.Enqueue( w );
    }

    public abstract void Generate( WorldGenerator generator );
    public Vector2Int WorldSize() => new( DefaultWorldSizeX, DefaultWorldSizeY );

    public float BossZoneCenterX() =>
        WorldGenerator.CoordXToWorldX( DefaultWorldSizeX - BossZoneOffset + BossZoneSize / 2f );

}