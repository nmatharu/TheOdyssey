using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopBlocks : MonoBehaviour
{
    [ SerializeField ] GameObject[] commonRuneShops;
    [ SerializeField ] GameObject[] rareRuneShops;
    [ SerializeField ] GameObject[] legendaryRuneShops;
    
    public static ShopBlocks Instance { get; private set; }

    void Awake()
    {
        if( Instance != null && Instance != this )
            Destroy( this );
        else
            Instance = this;
    }

    public GameObject RuneShop()
    {
        var roll = Random.value;
        return roll switch
        {
            < 0.75f => commonRuneShops.RandomEntry(),
            < 0.95f => rareRuneShops.RandomEntry(),
            _ => legendaryRuneShops.RandomEntry()
        };
    }
}
