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
    
    readonly Queue<Enemy> _enemiesToIntro = new();
    readonly Queue<Vector2> _waveQueue = new(); // this queue uses the world X positions instead of the pct above

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
            _waveQueue.Enqueue(  new Vector2( WorldGenerator.CoordXToWorldX( PlayerStartZone + CoreLength * w.x ), w.y ) );
    }

    public List<Enemy> WaveEnemies( float unscaledWaveBudget )
    {
        var numPlayers = GameManager.Instance.NumPlayersInParty();
        var multiplier = GameManager.Instance.EnemyWaveBudgetMultiplier();
        
        var waveBudget = (int) ( unscaledWaveBudget * multiplier );
        var es = new List<Enemy>();
        
        // Introductory wave
        if( waveBudget == 0 )
        {
            var enemy = _enemiesToIntro.Dequeue();

            // Cheap enemy : 1, 2, 3 players -> 2, 2, 3 enemies
            // Expensive enemy : 1, 2, 3 players -> 1 enemy
            var numToSpawn = enemy.spawnCost < 2 ? ( numPlayers > 2 ? 3 : 2 ) : 1;
            for( var i = 0; i < numToSpawn; i++ )
                es.Add( enemy );
        }

        return es;
    }
    
    public Vector2 NextWave() => _waveQueue.Dequeue();
    public bool OutOfWaves() => _waveQueue.Empty();

    public abstract void Generate( WorldGenerator generator );
    public Vector2Int WorldSize() => new( DefaultWorldSizeX, DefaultWorldSizeY );

    public float BossZoneCenterX() =>
        WorldGenerator.CoordXToWorldX( DefaultWorldSizeX - BossZoneOffset + BossZoneSize / 2f );
}