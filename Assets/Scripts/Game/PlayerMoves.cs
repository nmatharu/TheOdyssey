using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoves : MonoBehaviour
{
    Player _player;
    
    public enum SpecialAttackType
    {
        Slash
    }
    
    void Start() => _player = GetComponent<Player>();

    public void LightAttack()
    {
        Debug.Log( "Light" );
    }

    public void SpecialAttack( SpecialAttackType type )
    {
        switch( type )
        {
            case SpecialAttackType.Slash:
                SpecialSlash();
                break;
            default:
                throw new ArgumentOutOfRangeException( nameof( type ), type, null );
        }
    }

    public void SpecialSlash()
    {
        
    }

    public void Roll()
    {
        Debug.Log( "Roll" );
    }

}
