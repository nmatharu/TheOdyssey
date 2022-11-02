using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullFireball : MonoBehaviour
{
    [ SerializeField ] float speed = 8f;
    [ SerializeField ] float dmg = 4f;
    
    Rigidbody _body;
    int _level;
    int _barrierLayer;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _barrierLayer = LayerMask.NameToLayer( "EnvironmentBarrier" );
    }

    public void Init( int level, Vector3 target )
    {
        _level = level;
        transform.LookAt( target );
    }
    void FixedUpdate() => _body.velocity = transform.forward * speed;

    void OnTriggerEnter( Collider other )
    {
        if( other.gameObject.layer == _barrierLayer )
        {
            Destroy( gameObject, 1f / speed );
            return;
        }

        var p = other.GetComponent<Player>();

        if( p == null || p.rolling ) return;
        p.IncomingDamage( dmg, _level );
        Destroy( gameObject );
    }
}
