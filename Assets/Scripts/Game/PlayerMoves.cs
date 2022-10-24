using System;
using System.Collections.Generic;
using UnityEngine;

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

    // 0 - slash, 1 - slash, 2 - stab
    int _lightSwingIndex = 0;

    int[] _aSwings;
    static readonly int ASwingA = Animator.StringToHash( "Armature|5_SwingA" );
    static readonly int ASwingB = Animator.StringToHash( "Armature|5_SwingB" );
    static readonly int ASwingC = Animator.StringToHash( "Armature|5_SwingC" );

    static readonly int ASlash = Animator.StringToHash( "Armature|7_Heavy_A_Default" );
    static readonly int ARoll = Animator.StringToHash( "Armature|6_Roll" );

    void Start()
    {
        _player = GetComponent<Player>();
        _aSwings = new[] { ASwingA, ASwingB, ASwingC };
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
        LockFor( lightAttackFramesLock );

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

        _player.Mator().Play( ASlash );
        LockFor( slashAttackFramesLock );
        DamageOnActiveFrame( Physics.OverlapSphere( transform.position, 3f ), slashAttackDmg, slashAttackActiveFrame );
    }

    public void Roll()
    {
        if( _player.dead || _player.rolling || _player.locked || _player.rollOnCd ) return;

        _player.Mator().Play( ARoll );

        _player.rolling = true;
        _player.rollOnCd = true;

        this.Invoke( () => _player.rolling = false, _player.rollDuration );
        this.Invoke( () => _player.rollOnCd = false, _player.rollCooldown );
    }

    void LockFor( float frames60 )
    {
        _player.locked = true;
        this.Invoke( () => _player.locked = false, frames60 / 60f );
    }

    void DamageOnActiveFrame( IEnumerable<Collider> colliders, int damage, float frames60 )
    {
        this.Invoke( () =>
        {
            foreach( var c in colliders )
            {
                if( _player.dead )  return;

                if( c == null ) continue;
                var e = c.GetComponent<Enemy>();
                if( e != null )
                {
                    e.TakeDamage( _player, damage );
                }
            }
        }, frames60 / 60f );
    }

    void OnDrawGizmos()
    {
        // Gizmos.DrawWireCube( transform.position + transform.forward, lightStabRectBounds * 2f );
        // Gizmos.DrawWireSphere( transform.position + transform.forward / 2f, 2 );
    }
}