using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SandboxShopNPC : MonoBehaviour
{
    [ SerializeField ] Vector3 baselinePos;
    [ SerializeField ] GameObject shopRune;
    [ SerializeField ] Transform barrierL;
    [ SerializeField ] Transform barrierR;
    [ SerializeField ] Vector3 offsetL;
    [ SerializeField ] Vector3 offsetR;
    [ SerializeField ] Transform shopMan;
    
    void Start()
    {
        var numRunes = RuneIndex.Instance.runeIndex.Length;
        for( var i = 0; i < numRunes; i++ )
        {
            var rune = RuneIndex.Instance.runeIndex[ i ];

            var o = Instantiate( shopRune, baselinePos + Vector3.right * ( 2 * i ), Quaternion.identity,
                WorldGenerator.Instance.objsParent );
            o.GetComponent<ShopRune>().SetRune( rune );
        }

        barrierL.position = baselinePos + offsetL;
        barrierR.position = ( baselinePos + Vector3.right * ( 2 * numRunes - 1 ) ) + offsetR;
        shopMan.position = ( baselinePos + Vector3.right * ( numRunes ) ) + ( 2 * Vector3.forward );
    }
}
