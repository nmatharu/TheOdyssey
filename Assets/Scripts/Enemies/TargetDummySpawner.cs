using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDummySpawner : MonoBehaviour
{
    [ SerializeField ] GameObject targetDummyPrefab;
    [ SerializeField ] GameObject dummySpawnPfx;
    [ SerializeField ] float spawnDelay;
    [ SerializeField ] Enemy[] dummies;

    Vector3[] _dummyPositions;
    bool[] _respawningAt;
    void Start()
    {
        _dummyPositions = new Vector3[ dummies.Length ];
        _respawningAt = new bool[ dummies.Length ];
        for( var i = 0; i < dummies.Length; i++ )
            _dummyPositions[ i ] = dummies[ i ].transform.position;
    }

    void Update()
    {
        for( var i = 0; i < dummies.Length; i++ )
        {
            if( dummies[ i ] == null && !_respawningAt[ i ] )
            {
                Respawn( _dummyPositions[ i ], i );
                _respawningAt[ i ] = true;
            }

        }
    }

    public void Respawn( Vector3 pos, int i )
    {
        this.Invoke( () => Instantiate( dummySpawnPfx, pos, Quaternion.identity ), 1f );
        this.Invoke( () =>
        {
            var o = Instantiate( targetDummyPrefab, pos, Quaternion.identity, transform );
            dummies[ i ] = o.GetComponent<Enemy>();
            _respawningAt[ i ] = false;
        }, 1f + spawnDelay );
    }
}
