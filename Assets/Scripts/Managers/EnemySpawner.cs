using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [ SerializeField ] GameObject spawnPillar;
    [ SerializeField ] Transform enemiesParent;
    [ SerializeField ] Transform spawnersParent;
    [ SerializeField ] GameObject enemyBarrier;
    
    [ SerializeField ] Enemy[] enemies;
    [ SerializeField ] EnemyWave[] waves;
    [ SerializeField ] EnemyWave bossWave;

    Queue<EnemyWave> _waveQueue;
    
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

        _waveQueue = new Queue<EnemyWave>();

        foreach( var w in waves )
            _waveQueue.Enqueue( w );
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
    
    public Transform EnemiesParent() => enemiesParent;

    public int NextXTrigger() => _waveQueue.Count > 0 ? _waveQueue.Peek().xTrigger : int.MaxValue;
    public int NextWaveSize() => _waveQueue.Peek().toSpawn.Count;

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

    GameObject Enemy( EnemyType type ) => enemies[ (int) type ].gameObject;

    public void StartBoss( float posX )
    {
        var wave = bossWave.toSpawn;
        var spawnPoints = WorldGenerator.Instance.ValidSpawnPointsAround( posX, wave.Count );
        for( var i = 0; i < wave.Count; i++ )
        {
            var i1 = i;
            this.Invoke( () =>
            {
                var p = Instantiate( spawnPillar, spawnPoints[ i1 ], Quaternion.identity, spawnersParent ).GetComponent<SpawnPillar>();
                p.Set( wave[ i1 ], 2f );
            }, i * 0.25f );
        }
    }
}