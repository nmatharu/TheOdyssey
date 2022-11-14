using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyBarrier : MonoBehaviour
{
    [ SerializeField ] ParticleSystem pfx;
    [ SerializeField ] Collider barrierCollider;

    readonly List<GameObject> _enemies = new();

    void Start()
    {
        pfx.Play();
        barrierCollider.enabled = true;
        InvokeRepeating( nameof( CheckIfWaveKilled ), 3f, 0.25f );
    }
    
    void CheckIfWaveKilled()
    {
        if( _enemies.Any( e => e != null ) ) return;
        CancelInvoke( nameof( CheckIfWaveKilled ) );
        Finish();
    }

    public void AddEnemy( GameObject e ) => _enemies.Add( e );

    void Finish()
    {
        pfx.Stop();
        barrierCollider.enabled = false;
        Destroy( gameObject, 5f );
    }
}