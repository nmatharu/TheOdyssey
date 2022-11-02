using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RuneShop : Interactable
{
    [ SerializeField ] RuneIndex.Runes rune;
    [ SerializeField ] RuneIndex.RuneTiers tier;
    [ SerializeField ] TextMeshProUGUI costText;
    int _cost;
    
    void Start()
    {
        _cost = GameManager.Instance.RandomRunePrice( tier );
        costText.text = _cost.ToString();
        interactPrompt = "PURCHASE";
        cannotInteractPrompt = "TOO MUCH";
    }

    public override void Interact( Player player ) => player.BuyRune( rune, _cost );

    public override bool InteractionLocked( Player player ) => player.CantAfford( _cost );
}
