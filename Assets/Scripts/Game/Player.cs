using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [ SerializeField ] float maxHp = 30;
    int _level;
    float _hp;
    Material _material;
    
    void Start()
    {
        _hp = maxHp;
        _material = GetComponentInChildren<Renderer>().material;
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.D ) )
        {
            TakeDamage( 5 );
        }
    }

    public void TakeDamage( float dmg )
    {
        StartCoroutine( FlashMaterial() );
        
        _hp -= dmg;
        if( _hp <= 0 )
            Die();
    }
    
    IEnumerator FlashMaterial()
    {
        var delay = new WaitForFixedUpdate();
        for( var i = 0f; i < 1; i += 0.1f )
        {
            _material.color = new Color( i, i, i );
            yield return delay;
        }
        _material.color = Color.white;
    }

    void Die() => Destroy( gameObject );

    public float MaxHp() => maxHp;
    public float HpPct() => _hp / maxHp;
}
