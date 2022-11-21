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

    Rune _rune;
    int _cost;
    
    void Start()
    {
        // _rune = RuneIndex.Instance.RandomShopRune();
        _cost = GameManager.Instance.RandomRunePrice( _rune.Tier() );

        foreach( var i in icons )
            i.sprite = _rune.Icon();

        titleText.text = _rune.Name().ToUpper();
        rarityText.text = _rune.Tier().ToString().ToUpper();
        descText.text = _rune.Desc();
        
        costText.text = _cost.ToString();
        interactPrompt = "PURCHASE";
        cannotInteractPrompt = "TOO MUCH";
    }

    public override void Interact( Player player ) => player.BuyRune( _rune, _cost );

    public override bool InteractionLocked( Player player ) => player.CantAfford( _cost );

    public void SetRune( Rune rune ) => _rune = rune;
}
