using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    [ SerializeField ] Transform[] shopPositions;
    [ SerializeField ] GameObject shopRune;
    
    void Start()
    {
        var runes = RuneIndex.Instance.ShopRunes( shopPositions.Length );
        for( var i = 0; i < shopPositions.Length; i++ )
        {
            var t = shopPositions[ i ];
            var o = Instantiate( shopRune, t.position, Quaternion.identity, WorldGenerator.Instance.objsParent );
            var r = o.GetComponent<ShopRune>();
            r.SetRune( runes[ i ] );
        }
    }
}
