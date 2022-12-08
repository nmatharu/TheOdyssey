using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SkullFireball : MonoBehaviour
{
    [ SerializeField ] ParticleSystem pfx;
    [ SerializeField ] float speed = 8f;
    [ SerializeField ] float dmg = 4f;

    [ SerializeField ] bool sineMovement = false;
    [ SerializeField ] float sineAmount = 2f;
    [ SerializeField ] float sinePeriod = 2f;
    
    Rigidbody _body;
    int _level;
    int _barrierLayer;

    float _elapsed = 0f;
    Vector3 _movementDir;
    bool _sineLeft;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _barrierLayer = LayerMask.NameToLayer( "EnvironmentBarrier" );
        _movementDir = transform.forward;
        _sineLeft = Random.value < 0.5f;
    }

    public void Init( int level, Vector3 target )
    {
        _level = level;
        transform.LookAt( target );
    }
    void FixedUpdate()
    {
        var t = transform;
        if( sineMovement )
        {
            _body.velocity = speed * t.forward + t.right * (( _sineLeft ? 1 : -1 ) * sineAmount * Mathf.Cos( _elapsed / sinePeriod ));
            _elapsed += Time.fixedDeltaTime;
            return;
        }

        _body.velocity = t.forward * speed;
    }

    void OnTriggerEnter( Collider other )
    {
        if( other.gameObject.layer == _barrierLayer )
        {
            pfx.Stop();
            Destroy( gameObject, 1f / speed );
            return;
        }

        var p = other.GetComponent<Player>();

        if( p == null || p.rolling ) return;
        
        ( sineMovement ? AudioManager.Instance.sandSkullFireballHit : AudioManager.Instance.skullFireballHit ).PlaySfx( 1f, 0.1f );
        
        p.IncomingDamage( dmg, _level );
        Destroy( gameObject );
    }
}
