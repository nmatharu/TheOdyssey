using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class BossGolemDropIn : MonoBehaviour
{
    [ SerializeField ] float dropInHeight = 20;
    [ SerializeField ] float initVelocity = 2f;
    [ SerializeField ] float dropInGravityStrength = 2f;
    [ SerializeField ] float smashDelay;
    
    [ SerializeField ] ImageFader smashIndicator;
    [ SerializeField ] ParticleSystem smashPfx;
    [ SerializeField ] float initSmashRadius;
    [ SerializeField ] int initSmashDamage = 6;

    Rigidbody _body;
    Enemy _enemy;
    Golem _golem;
    
    float _dropSpeed;

    bool _stopFalling;
    
    void Start()
    {
        transform.position += Vector3.up * dropInHeight;
        _body = GetComponent<Rigidbody>();
        _enemy = GetComponent<Enemy>();
        _golem = GetComponent<Golem>();
        _golem.enabled = false;
        
        this.Invoke( () => smashIndicator.FadeIn(), 0.1f );
        
        _dropSpeed = initVelocity;
    }

    void FixedUpdate()
    {
        if( _stopFalling )
            return;

        transform.position += Vector3.down * _dropSpeed;
        smashIndicator.transform.position =
            new Vector3( transform.position.x, 0.01f, transform.position.z );
        _dropSpeed += dropInGravityStrength;

        if( transform.position.y <= 0 )
        {
            transform.position = new Vector3( transform.position.x, 0, transform.position.z );
            _stopFalling = true;

            this.Invoke( () =>
            {
                // smashIndicator.Show();
                smashPfx.Play();
                smashIndicator.FadeOut();
                
                var colliders = Physics.OverlapSphere( transform.position, initSmashRadius );
                foreach( var c in colliders )
                {
                    var p = c.GetComponent<Player>();
                    if( p != null )
                        p.IncomingDamage( initSmashDamage, _enemy.Level() );
                }
                
            }, smashDelay );
            
            _golem.enabled = true;
        }
    }
}
