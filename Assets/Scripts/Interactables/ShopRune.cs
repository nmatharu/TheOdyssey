using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopRune : Interactable
{
    [ SerializeField ] CanvasGroup textCanvasGroup;
    [ SerializeField ] TextMeshProUGUI titleText;
    [ SerializeField ] TextMeshProUGUI rarityText;
    [ SerializeField ] TextMeshProUGUI descText;
    [ SerializeField ] TextMeshProUGUI costText;
    [ SerializeField ] Image[] icons;

    [ SerializeField ] MeshRenderer ringRenderer;
    [ SerializeField ] Material normalMat;
    [ SerializeField ] Material goldMat;
    [ SerializeField ] ParticleSystem goldPfx;

    Rune _rune;
    int _cost;
    
    void Start()
    {
        // _rune = RuneIndex.Instance.RandomShopRune();
        _cost = (int) ( GameManager.Instance.RandomRunePrice( _rune.Tier() ) );

        if( Random.value < 0.33f )
        {
            ringRenderer.material = goldMat;
            goldPfx.Play();
        }

        foreach( var i in icons )
            i.sprite = _rune.Icon();

        titleText.text = _rune.Name().ToUpper();
        rarityText.text = _rune.Tier().ToString().ToUpper();
        descText.text = _rune.Desc();
        
        costText.text = _cost.ToString();
        interactPrompt = "PURCHASE";
        cannotInteractPrompt = "NEED MORE";
    }

    public override void Interact( Player player ) => player.BuyRune( _rune, _cost );

    public override bool InteractionLocked( Player player ) => player.CantAfford( _cost );

    public void SetRune( Rune rune ) => _rune = rune;
}
