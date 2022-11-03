using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    [ SerializeField ] Transform[] shopPositions;
    [ SerializeField ] GameObject shopRune;
    
    void Start()
    {
        foreach( var t in shopPositions )
            Instantiate( shopRune, t.position, Quaternion.identity, GameManager.Instance.interactablesParent );
    }
}
