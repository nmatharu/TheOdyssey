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

    NewRune _rune;
    int _cost;
    
    void Start()
    {
        // _rune = RuneIndex.Instance.RandomShopRune();
        _cost = GameManager.Instance.RandomRunePrice( _rune.rarity );

        if( _rune.rarity == NewRune.Rarity.Rare )
        {
            ringRenderer.material = goldMat;
            goldPfx.Play();
        }

        foreach( var i in icons )
            i.sprite = _rune.icon;

        titleText.text = _rune.runeName.ToUpper();
        rarityText.text = _rune.rarity.ToString().ToUpper();
        descText.text = _rune.description;
        
        costText.text = _cost.ToString();
        interactPrompt = "PURCHASE";
        cannotInteractPrompt = "NEED MORE";
    }

    public override void Interact( Player player ) => player.BuyRune( _rune, _cost );

    public override bool InteractionLocked( Player player ) => player.CantAfford( _cost );

    public void SetRune( NewRune rune ) => _rune = rune;
}
