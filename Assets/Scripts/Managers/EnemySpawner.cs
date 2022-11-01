using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [ SerializeField ] GameObject spawnPillar;
    [ SerializeField ] Transform enemiesParent;
    [ SerializeField ] Transform spawnersParent;

    [ SerializeField ] Enemy[] enemies;

    Queue<EnemyWave> _waveStack;
    
    public static EnemySpawner Instance { get; private set; }

    void Awake()
    {
        if( Instance != null && Instance != this )
            Destroy( this );
        else
            Instance = this;

        _waveStack = new Queue<EnemyWave>();
        
        _waveStack.Enqueue( new EnemyWave( 40, new[] { enemies[ 0 ].gameObject, enemies[ 0 ].gameObject } ) );

        _waveStack.Enqueue( new EnemyWave( 60, new[] { enemies[ 1 ].gameObject, enemies[ 2 ].gameObject, 
            enemies[ 2 ].gameObject, enemies[ 0 ].gameObject } ) );

        _waveStack.Enqueue( new EnemyWave( 80, new[] { enemies[ 1 ].gameObject, enemies[ 2 ].gameObject, 
            enemies[ 2 ].gameObject, enemies[ 0 ].gameObject, enemies[ 1 ].gameObject, enemies[ 2 ].gameObject, 
            enemies[ 2 ].gameObject, enemies[ 0 ].gameObject, enemies[ 1 ].gameObject } ) );
    }

    public void Spawn( GameObject o, Vector3 pos )
    {
        var p = Instantiate( spawnPillar, pos, Quaternion.identity, spawnersParent ).GetComponent<SpawnPillar>();
        p.Set( o, 2f );
    }

    public void LetItRip( Vector3[] spawnPoints )
    {
        var wave = _waveStack.Dequeue().ToSpawn;
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
    
    public Transform EnemiesParent() => enemiesParent;

    public int NextXTrigger() => _waveStack.Count > 0 ? _waveStack.Peek().XTrigger : int.MaxValue;
    public int NextWaveSize() => _waveStack.Peek().ToSpawn.Count;

    class EnemyWave
    {
        public readonly int XTrigger;
        public readonly List<GameObject> ToSpawn;

        public EnemyWave( int xTrigger, IEnumerable<GameObject> toSpawn )
        {
            XTrigger = xTrigger;
            ToSpawn = toSpawn.ToList();
        }
    }
}