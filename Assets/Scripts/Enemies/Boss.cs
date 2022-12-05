using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [ SerializeField ] BossWave[] bossWaves;
    protected Enemy Enemy;

    Queue<BossWave> _waveQueue;

    [ Serializable ]
    class BossWave
    {
        public float HealthPct;
        public int SpawnBudget;
        public Enemy[] EnemyBucket;
    }

    void Start()
    {
        Enemy = GetComponent<Enemy>();

        _waveQueue = new Queue<BossWave>();
        foreach( var w in bossWaves )
            _waveQueue.Enqueue( w );
    }

    void Update()
    {
        if( _waveQueue.Empty() )
            return;

        if( !( Enemy.HpPct() <= _waveQueue.Peek().HealthPct ) ) return;
        
        var w = _waveQueue.Dequeue();
        var budget = w.SpawnBudget;

        var es = new List<Enemy>();
        for( var iters = 0; budget > 0 && iters < 100; iters++ )
        {
            var e = w.EnemyBucket.RandomEntry();
            if( e.spawnCost > budget ) continue;
            es.Add( e );
            budget -= e.spawnCost;
        }
            
        EnemySpawner.Instance.LetItRipBossWave( es );
    }

    void OnDestroy() => GameManager.Instance.AwardCrystal();
}
