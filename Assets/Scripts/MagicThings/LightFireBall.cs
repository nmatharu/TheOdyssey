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

    void Start()
    {
        Destroy( gameObject, 3f );
    }

    void Update()
    {
        transform.position += transform.forward * Time.deltaTime * speed;
    }

    void OnTriggerEnter( Collider c )
    {
        var id = Guid.NewGuid();
        var e = Enemy.FromCollider( c );
        if( e != null )
        {
            // TODO Move out of magic, call player dmg, all in one place
            e.TakeDamage( _player, baseDamage * _player.DamageMultiplier() * _player.MagicMultiplier(), id );
        }
    }

    public void Init( Player p ) => _player = p;
}
