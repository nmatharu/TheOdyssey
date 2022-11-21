using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicShrine : Interactable
{
    MagicSpell _magic;

    void Start()
    {
        Init( gameObject.AddComponent<TestMagic>() );
    }

    public void Init( MagicSpell magic )
    {
        _magic = magic;
        
        // Set interface fields
    }

    public override void Interact( Player player ) => player.LearnMagic( _magic );

    public override bool InteractionLocked( Player player ) => false;
}
