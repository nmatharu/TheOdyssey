using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealingRing : MonoBehaviour
{
    [ SerializeField ] float healTicksEverySeconds = 1f;

    HashSet<Player> _playersInZone;

    void Start()
    {
        _playersInZone = new HashSet<Player>();
        InvokeRepeating( nameof( Heal ), 0f, healTicksEverySeconds );
    }

    void OnTriggerEnter( Collider c )
    {
        var p = c.GetComponent<Player>();
        if( p != null )
            _playersInZone.Add( p );
    }

    void OnTriggerExit( Collider c )
    {
        var p = c.GetComponent<Player>();
        if( p != null )
            _playersInZone.Remove( p );
    }

    void Heal()
    {
        foreach( var p in _playersInZone )
            p.CampfireHeal();
    }
}
