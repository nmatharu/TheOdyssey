using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MagicShrine : Interactable
{
    [ SerializeField ] TextMeshProUGUI magicTitle;
    [ SerializeField ] TextMeshProUGUI magicDescription;
    [ SerializeField ] Image magicImage;

    MagicSpell _magic;

    void Start()
    {
        if( _magic == null )
            Init( GameManager.Instance.magicPool.RandomEntry() );
    }

    public void Init( MagicSpell magic )
    {
        _magic = magic;
        magicTitle.text = magic.magicName;
        magicDescription.text = magic.magicDescription;
        magicImage.sprite = magic.magicIcon;
    }

    public void Init( int i )
    {
        if( i >= GameManager.Instance.magicPool.Length )
        {
            Destroy( gameObject );
            return;
        }
        Init( GameManager.Instance.magicPool[ i ] );
    }

    public override void Interact( Player player ) => player.LearnMagic( _magic );

    public override bool InteractionLocked( Player player ) => player.MagicSpell() == _magic;
}
