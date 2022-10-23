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
    InteractPrompt _interactPrompt;
    PlayerMoves _playerMoves;

    PlayerMoves.SpecialAttackType _specialAttack;

    bool _inputDisabled = false;
    
    void Start()
    {
        _hp = maxHp;
        _material = GetComponentInChildren<Renderer>().material;
        _animationMovement = GetComponent<AnimationMovement>();
        _interactPrompt = GetComponentInChildren<InteractPrompt>();
        _playerMoves = GetComponent<PlayerMoves>();
        _specialAttack = PlayerMoves.SpecialAttackType.Slash;
        Debug.Log( _interactPrompt == null );
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.D ) )
        {
            TakeDamage( 5 );
        }

        if( Input.GetKeyDown( KeyCode.H ) )
        {
            _interactPrompt.SetInteractable( true, "PICK UP" );
        }
    }

    public void InputMovement( Vector2 v )
    {
        _moveDir = v;
        _animationMovement.Move( v );
    }
    
    public void LightAttack()
    {
        _playerMoves.LightAttack();
        _animationMovement.LightAttack();
    }

    public void SpecialAttack()
    {
        _playerMoves.SpecialAttack( _specialAttack );
        _animationMovement.HeavyAttack();
    }

    public void Roll()
    {
        _playerMoves.Roll();
        _animationMovement.Roll();
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

    void Die() => gameObject.SetActive( false );

    public float MaxHp() => maxHp;
    public float HpPct() => _hp / maxHp;
    public bool Rolling() => _animationMovement.Rolling();
    public bool InputDisabled() => _inputDisabled;

}
