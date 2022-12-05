using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingDagger : MonoBehaviour
{
    [ SerializeField ] ParticleSystem pfx;

    Player _player;
    Enemy _target;
    bool _moving = true;
    float _damage;
    float _timeToTarget;

    float _elapsed;
    Vector3 _startPos;

    void Start() => _startPos = transform.position;

    void Update()
    {
        if( !_moving )  return;

        if( _target == null )
        {
            _moving = false;
            pfx.Stop();
            Destroy( gameObject, 1f );
            return;
        }

        var elapsedPct = _elapsed / _timeToTarget; 

        transform.position = Vector3.Lerp( _startPos, _target.transform.position +
            new Vector3( 0, 3f * Mathf.Sin( elapsedPct * Mathf.PI ), 0 ), elapsedPct );

        if( _elapsed > _timeToTarget && _moving )
            DamageTarget();
        
        _elapsed += Time.deltaTime;
    }

    void DamageTarget()
    {
        _moving = false;
        pfx.Stop();

        _player.DamageEnemy( Guid.NewGuid(), _target, _damage, false, true );
        _player.OnHit( 1, false, true );
        
        Destroy( gameObject, 1f );
    }
    
    public void Init( Player p, Enemy t, float dmg, float ttt )
    {
        _player = p;
        _target = t;
        _damage = dmg;
        _timeToTarget = ttt;
    }
}
