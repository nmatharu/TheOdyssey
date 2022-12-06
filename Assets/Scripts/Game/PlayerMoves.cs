using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerMoves : MonoBehaviour
{
    public enum SpecialAttackType
    {
        Slash,
        Dash,       // Dashing forward, execute rectangle slash
        Double,     // Fast double slash, kites backwards
        Throw,      // Throw and retrieve sword
        Vortex,     // Sword vortex
        Stack,      // Big stacking overhead slash
        Fragment    // Throw blade fragment
    }

    [ SerializeField ] int lightAttackFramesLock = 12;
    [ SerializeField ] int lightAttackActiveFrame = 2;
    [ SerializeField ] float lightAttackSlashRadius = 2f;
    [ SerializeField ] Vector3 lightStabRectBounds = new( 2f, 1f, 2f ); 
    
    [ SerializeField ] int slashAttackFramesLock = 30;
    [ SerializeField ] int slashAttackActiveFrame = 16;
    [ SerializeField ] float slashAttackRadius = 3f;

    // The amount of time to wait since the last swing before 
    // we reset the swing sequence to the first slash again
    [ SerializeField ] float firstSwingDelay = 0.75f;

    [ SerializeField ] int lightAttackDmg = 3;
    [ SerializeField ] int lightAttackDmg3 = 5;
    [ SerializeField ] int slashAttackDmg = 8;

    Player _player;
    PlayerStatusBar _statusBar;
    SwordPfx _swordPfx;
    Collider _collider;

    // 0 - slash, 1 - slash, 2 - stab
    int _lightSwingIndex = 0;

    int[] _aSwings;
    static readonly int ASwingA = Animator.StringToHash( "Armature|5_SwingA" );
    static readonly int ASwingB = Animator.StringToHash( "Armature|5_SwingB" );
    static readonly int ASwingC = Animator.StringToHash( "Armature|5_SwingC" );

    static readonly int ASlash = Animator.StringToHash( "Armature|7_Heavy_A_Default" );
    static readonly int ARoll = Animator.StringToHash( "Armature|6_Roll" );

    static readonly int ARunningSpeed = Animator.StringToHash( "RunningSpeed" );

    int _physicsLayerPlayer;
    int _physicsLayerPlayerDashing;

    void Start()
    {
        _player = GetComponent<Player>();
        _statusBar = GetComponentInChildren<PlayerStatusBar>();
        _swordPfx = GetComponent<SwordPfx>();
        _collider = GetComponent<Collider>();
        _aSwings = new[] { ASwingA, ASwingB, ASwingC };
        
        _physicsLayerPlayer = LayerMask.NameToLayer( "PlayerRb" );
        _physicsLayerPlayerDashing = LayerMask.NameToLayer( "PlayerRbDashing" );
    }

    public void SetRunSpeed( float spdMultiplier )
    {
        _player.Mator().SetFloat( ARunningSpeed, spdMultiplier );
    }

    public void LightAttack()
    {
        if( _player.dead || _player.rolling || _player.locked ) return;

        if( _lightSwingIndex != 2 ) // Left/Right Slashes
        {
            DamageOnActiveFrame( 
                Physics.OverlapSphere( transform.position + transform.forward / 2f, 
                    lightAttackSlashRadius ), lightAttackDmg, lightAttackActiveFrame );
        }
        else // Straight stab
        {
            DamageOnActiveFrame( 
                Physics.OverlapBox( transform.position + transform.forward, 
                    lightStabRectBounds ), lightAttackDmg3, lightAttackActiveFrame );
        }
        
        var swing = _aSwings[ _lightSwingIndex ];
        _lightSwingIndex = ( _lightSwingIndex + 1 ) % _aSwings.Length;

        _player.Mator().Play( swing );
        _swordPfx.Light();
        LockFor( lightAttackFramesLock );
        AudioManager.Instance.swordSwings.RandomEntry().PlaySfx( 1f, 1f, 0.95f, 1.05f );

        CancelInvoke( nameof( ResetLightAttackSequence ) );
        Invoke( nameof( ResetLightAttackSequence ), firstSwingDelay );
    }

    void ResetLightAttackSequence() => _lightSwingIndex = 0;

    public void SpecialAttack( SpecialAttackType type )
    {
        if( _player.dead )  return;
        
        switch( type )
        {
            case SpecialAttackType.Slash:
                SpecialSlash();
                break;
            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    void SpecialSlash()
    {
        if( _player.rolling || _player.locked ) return;

        AudioManager.Instance.bigSwings.RandomEntry().PlaySfx( 1f, 0.1f );
        
        _player.Mator().Play( ASlash );
        _swordPfx.Slash();
        LockFor( slashAttackFramesLock );
        DamageOnActiveFrame( Physics.OverlapSphere( transform.position, 
            slashAttackRadius ), slashAttackDmg, slashAttackActiveFrame );
    }

    public void Roll()
    {
        if( _player.dead || _player.rolling || _player.locked || _player.rollOnCd ) return;

        _player.Mator().Play( ARoll );

        _player.rolling = true;
        _player.rollOnCd = true;

        gameObject.layer = _physicsLayerPlayerDashing;

        this.Invoke( () =>
        {
            gameObject.layer = _physicsLayerPlayer;
            _player.rolling = false;
            
        }, _player.rollDuration );
        
        this.Invoke( () => _player.rollOnCd = false, _player.rollCooldown );
        _statusBar.SetRollCdBar( _player.rollCooldown );
    }

    void LockFor( float frames60 )
    {
        _player.locked = true;
        this.Invoke( () => _player.locked = false, frames60 / 60f );
    }

    void PlayerFootstep()
    {
        var i = WorldGenerator.Instance.FootstepsIndex( transform.position );
        AudioManager.Instance.FootstepsSfx( i ).RandomEntry().PlaySfx( 0.4f, 0.2f );
    }

    void DamageOnActiveFrame( IEnumerable<Collider> colliders, int damage, float frames60 )
    {
        this.Invoke( () =>
        {
            var enemiesHit = 0;

            var id = Guid.NewGuid();
            
            foreach( var c in colliders )
            {
                if( _player.dead )  return;
                if( c == null ) continue;
                
                var e = Enemy.FromCollider( c );
                if( e != null )
                {
                    enemiesHit++;

                    _player.DamageEnemy( id, e, damage, true, false );
                }

            }

            if( enemiesHit > 0 )
            {
                _player.OnHit( enemiesHit, true, false );
                // _player.LifeSteal();
                // _player.ReduceMagicCd( 0.25f );
            }
            
        }, frames60 / 60f );
    }
}