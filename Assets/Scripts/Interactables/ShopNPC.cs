using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopNPC : MonoBehaviour
{
    [ SerializeField ] Transform[] shopPositions; 
    
    void Start()
    {
        foreach( var t in shopPositions )
        {
            Instantiate( ShopBlocks.Instance.RuneShop(), t.position, Quaternion.identity );
        }
    }
}
