using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [ SerializeField ] GameObject spawnPillar;
    [ SerializeField ] Transform enemiesParent;
    [ SerializeField ] Transform spawnersParent;
    [ SerializeField ] GameObject enemyBarrier;
    
    Queue<EnemyWave> _waveQueue;

    // new wave gen, remove old stuff TODO
    float _nextWaveX;
    float _nextWaveBudget;
    
    enum EnemyType
    {
        Skull,
        Nightmare,
        Golem
    }
    
    public static EnemySpawner Instance { get; private set; }

    void Awake()
    {
        if( Instance != null && Instance != this )
            Destroy( this );
        else
            Instance = this;
    }

    public void SetWave( Vector2 wave )
    {
        _nextWaveX = wave.x;
        _nextWaveBudget = wave.y;
    }

    public void LetItRip( float maxPlayerX, Vector3[] spawnPoints )
    {
        var maxSpawnX = spawnPoints.Max( p => p.x );
        maxSpawnX = Mathf.Max( maxPlayerX, maxSpawnX );
        
        var b = Instantiate( enemyBarrier, new Vector3( maxSpawnX + 8f, 0, 0 ), 
            Quaternion.identity, spawnersParent );
        var barrier = b.GetComponent<EnemyBarrier>();

        var wave = _waveQueue.Dequeue().toSpawn;
        for( var i = 0; i < wave.Count; i++ )
        {
            var i1 = i;
            this.Invoke( () =>
            {
                var p = Instantiate( spawnPillar, spawnPoints[ i1 ], Quaternion.identity, spawnersParent ).GetComponent<SpawnPillar>();
                p.Set( wave[ i1 ], 2f, barrier );
            }, i * 0.25f );
        }
    }
    
    public void LetItRip2( float maxPlayerX )
    {
        var enemies = GameManager.Instance.CurrentLevel().WaveEnemies( _nextWaveBudget );
        var spawnPoints = WorldGenerator.Instance.ValidSpawnPointsAround( maxPlayerX, enemies.Count );
        
        var maxSpawnX = spawnPoints.Max( p => p.x );
        maxSpawnX = Mathf.Max( maxPlayerX, maxSpawnX );

        var b = Instantiate( enemyBarrier, new Vector3( maxSpawnX + 8f, 0, 0 ), 
            Quaternion.identity, spawnersParent );
        var barrier = b.GetComponent<EnemyBarrier>();
        
        for( var i = 0; i < enemies.Count; i++ )
        {
            var i1 = i;
            this.Invoke( () =>
            {
                var p = Instantiate( spawnPillar, spawnPoints[ i1 ], Quaternion.identity, spawnersParent ).GetComponent<SpawnPillar>();
                p.Set( enemies[ i1 ].gameObject, 2f, barrier );
            }, i * 0.25f );
        }

        GameManager.Instance.RequestNextWave();
    }

    public void LetItRipBossWave( List<Enemy> enemies )
    {
        var spawnPoints = WorldGenerator.Instance.BossWaveSpawnPoints( enemies.Count );
        
        for( var i = 0; i < enemies.Count; i++ )
        {
            var i1 = i;
            this.Invoke( () =>
            {
                var p = Instantiate( spawnPillar, spawnPoints[ i1 ], Quaternion.identity, spawnersParent ).GetComponent<SpawnPillar>();
                p.Set( enemies[ i1 ].gameObject, 2f, null );
            }, i * 1f );
        }
    }

    public void OutOfWaves() => _nextWaveX = int.MaxValue;

    public Transform EnemiesParent() => enemiesParent;

    public float NextXTrigger() => _nextWaveX;

    [ Serializable ]
    class EnemyWave
    {
        [ SerializeField ] public int xTrigger;
        [ SerializeField ] public List<GameObject> toSpawn;

        public EnemyWave( int xTrigger, IEnumerable<GameObject> toSpawn )
        {
            this.xTrigger = xTrigger;
            this.toSpawn = toSpawn.ToList();
        }
    }

    public void StartBoss()
    {
        var o = WorldGenerator.Instance.BossSpawner();
        var p = WorldGenerator.Instance.BossZoneCenter();

        this.Invoke( () =>
        {
            o = Instantiate( o, p, Quaternion.identity, enemiesParent );
            var e = o.GetComponent<Enemy>();
            if( e != null )
                e.Init( GameManager.Instance.EnemyLevel() );
        }, 1f );
    }
}