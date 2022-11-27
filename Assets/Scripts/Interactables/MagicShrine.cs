using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MagicShrine : Interactable
{
    [ SerializeField ] MagicSpell[] magicPool;
    
    [ SerializeField ] TextMeshProUGUI magicTitle;
    [ SerializeField ] TextMeshProUGUI magicDescription;

    MagicSpell _magic;

    void Start() => Init( magicPool.RandomEntry() );

    void Init( MagicSpell magic )
    {
        _magic = magic;
        magicTitle.text = magic.magicName;
        magicDescription.text = magic.magicDescription;
    }

    public override void Interact( Player player ) => player.LearnMagic( _magic );

    public override bool InteractionLocked( Player player ) => false;
}
