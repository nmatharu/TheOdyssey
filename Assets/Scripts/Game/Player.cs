using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [ SerializeField ] float maxHp = 30;
    [ SerializeField ] float rollSpeedMultiplier = 1.5f;
    [ SerializeField ] float speed = 8f;

    public float rollDuration = 0.3f;
    public float rollCooldown = 1f;

    [ SerializeField ] ParticleSystem swordPfx;
    
    Rigidbody _body;
    Animator _animator;
    
    int _level;
    float _hp;
    Material _material;
    
    AnimationMovement _animationMovement;
    InteractPrompt _interactPrompt;
    PlayerMoves _playerMoves;

    PlayerMoves.SpecialAttackType _specialAttack;

    HashSet<Interactable> _interactables;
    Interactable _closestInteractable;
    
    InGameStatistics _statistics;
    
    // For sequences like the intro run-in and outro run-out where we don't want the player to control
    public bool inputDisabled = false;

    public bool showOnCamera = true;    
    public bool dead = false;

    public bool rolling = false;    
    public bool locked = false;     // Locked in attack animation
    public bool stunned = false;
    public bool weaponless = false; // For special attacks that involve throwing the sword
    public bool rollOnCd = false;

    static Vector3 _cameraUp;
    Vector2 _moveInput;
    Vector3 _moveDir;
    Vector3 _lastPos;
    
    static readonly int IsRunning = Animator.StringToHash( "IsRunning" );

    void Start()
    {
        _hp = maxHp;
        _material = GetComponentInChildren<Renderer>().material;
        _animationMovement = GetComponent<AnimationMovement>();
        _interactPrompt = GetComponentInChildren<InteractPrompt>();
        _playerMoves = GetComponent<PlayerMoves>();
        _specialAttack = PlayerMoves.SpecialAttackType.Slash;
        _lastPos = transform.position;

        _body = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _interactables = new HashSet<Interactable>();
        _statistics = new InGameStatistics();
        
        if( Camera.main == null ) return;
        var fwd = Camera.main.transform.forward;
        _cameraUp = new Vector3( fwd.x, 0, fwd.z );
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

        FindClosestInteractable();
        UpdateInteractablePrompt();
    }

    void FixedUpdate()
    {
        if( dead )  return;
        if( rolling )
        {
            _body.velocity = transform.forward * ( rollSpeedMultiplier * speed );
            return;
        }

        var pos = transform.position;

        _moveDir = locked ? _moveDir : CameraCompensation( _moveInput );
        _body.velocity = locked ? Vector3.zero : _moveDir * speed;

        _animator.SetBool( IsRunning, _moveDir != Vector3.zero );
        transform.LookAt( pos + _moveDir );
        
        // Statistics junk
        _statistics.Move( Vector3.Distance( _lastPos, pos ) );
        _statistics.AliveForFixedTimeStep();
        _lastPos = transform.position;
    }

    public void InputMovement( Vector2 v ) => _moveInput = v;

    public void LightAttack()
    {
        _playerMoves.LightAttack();
        // _animationMovement.LightAttack();
    }

    public void SpecialAttack()
    {
        _playerMoves.SpecialAttack( _specialAttack );
        // _animationMovement.HeavyAttack();
    }

    public void Roll() => _playerMoves.Roll();

    public void TakeDamage( float dmg )
    {
        StartCoroutine( FlashMaterial() );

        var d = Mathf.RoundToInt( dmg );
        GameManager.Instance.SpawnDamageNumber( transform.position, d, false );
        _statistics.TakeDamage( d );
        
        _hp -= d;
        if( _hp <= 0 )
            Die();
    }
    
    IEnumerator FlashMaterial()
    {
        var delay = new WaitForFixedUpdate();
        for( var i = 0f; i < 1; i += 0.05f )
        {
            _material.color = new Color( 1f, i, i );
            yield return delay;
        }
        _material.color = Color.white;
    }

    static Vector3 CameraCompensation( Vector2 dir )
    {
        if( dir == Vector2.zero ) return Vector3.zero;
        var rotDegrees = Mathf.Atan2( dir.x, dir.y ) * Mathf.Rad2Deg;
        var v3Dir = Quaternion.Euler( 0, rotDegrees, 0 ) * _cameraUp;
        return v3Dir.normalized;
    }

    public void AddInteractable( Interactable interactable ) => _interactables.Add( interactable );
    public void RemoveInteractable( Interactable interactable ) => _interactables.Remove( interactable );
    
    void FindClosestInteractable()
    {
        var pos = transform.position;
        
        Interactable closest = null;
        var closestDSqr = Mathf.Infinity;
        foreach( var interactable in _interactables )
        {
            if( closest == null )
            {
                closest = interactable;
                closestDSqr = JBB.DistXZSquared( pos, interactable.transform.position );
                continue;
            }
            
            var dSqr = JBB.DistXZSquared( pos, interactable.transform.position );
            if( dSqr < closestDSqr )
            {
                closest = interactable;
                closestDSqr = dSqr;
            }
        }

        _closestInteractable = closest;
    }

    void UpdateInteractablePrompt()
    {
        if( _closestInteractable == null )
        {
            _interactPrompt.Hide();
            return;
        }
        
        _interactPrompt.SetInteractable( false, _closestInteractable.InteractPrompt() );
    }
    
    public void Interact()
    {
        if( _closestInteractable != null )
            _closestInteractable.Interact();
    }

    void Die()
    {
        _statistics.Die();
        dead = true;
        // gameObject.SetActive( false );
    }

    public float MaxHp() => maxHp;
    public float HpPct() => _hp / maxHp;
    public Animator Mator() => _animator;
    public InGameStatistics Statistics() => _statistics;
}
