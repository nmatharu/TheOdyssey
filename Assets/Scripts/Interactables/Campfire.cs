using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Campfire : Interactable
{
    bool _lit;

    [ SerializeField ] ParticleSystem firePfx;
    [ SerializeField ] GameObject healingRing;
    
    public override void Interact( Player player )
    {
        if( !_lit )
        {
            firePfx.Play();
            healingRing.SetActive( true );
            _lit = true;
            _disabled = true;
        }
    }

    public override bool InteractionLocked( Player player ) => _lit;
}
