using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFireBall : MonoBehaviour
{
    [ SerializeField ] ParticleSystem pfx;
    [ SerializeField ] float speed = 20f;
    [ SerializeField ] float timeToEnd = 0.75f;
    [ SerializeField ] float baseDamage = 5;
    Player _player;
    Guid _id;
    
    void Start()
    {
        Destroy( gameObject, 3f );
        _id = Guid.NewGuid();
    }

    void Update()
    {
        transform.position += transform.forward * Time.deltaTime * speed;
    }

    void OnTriggerEnter( Collider c )
    {
        var e = Enemy.FromCollider( c );
        if( e != null )
        {
            _player.DamageEnemy( _id, e, baseDamage, false, true );
            _player.OnHit( 1, false, true );
        }
    }

    public void Init( Player p ) => _player = p;
}
