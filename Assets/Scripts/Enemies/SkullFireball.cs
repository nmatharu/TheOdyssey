using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkullFireball : MonoBehaviour
{
    [ SerializeField ] float speed = 8f;
    [ SerializeField ] float dmg = 4f;
    
    Rigidbody _body;

    void Start() => _body = GetComponent<Rigidbody>();
    public void TargetDirection( Vector3 target ) => transform.LookAt( target );
    void FixedUpdate() => _body.velocity = transform.forward * speed;

    void OnTriggerEnter( Collider other )
    {
        var p = other.GetComponent<Player>();
        if( p == null || p.Rolling() ) return;
        p.TakeDamage( dmg );
        Destroy( gameObject );
    }
}
