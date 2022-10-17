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

    Vector2 _moveDir;

    AnimationMovement _animationMovement;
    
    void Start()
    {
        _hp = maxHp;
        _material = GetComponentInChildren<Renderer>().material;
        _animationMovement = GetComponent<AnimationMovement>();
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.D ) )
        {
            TakeDamage( 5 );
        }
    }

    public void InputMovement( Vector2 v )
    {
        _moveDir = v;
        _animationMovement.Move( v );
    }
    
    public void LightAttack() => _animationMovement.LightAttack();
    public void HeavyAttack() => _animationMovement.HeavyAttack();
    public void Roll() => _animationMovement.Roll();

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

    void Die() => gameObject.SetActive( false );

    public float MaxHp() => maxHp;
    public float HpPct() => _hp / maxHp;
    public bool Rolling() => _animationMovement.Rolling();

}
