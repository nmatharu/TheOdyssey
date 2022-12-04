using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RuneIndex : MonoBehaviour
{
    [ SerializeField ] Rune[] runes;
    [ SerializeField ] public NewRune[] runeIndex;

    Rune[][] _tieredRunes;

    public static RuneIndex Instance;
    
    void Awake()
    {
        if( Instance != null && Instance != this )
        {
            Destroy( this );
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad( this );
        }

        _tieredRunes = new Rune[ Enum.GetNames( typeof( Rune.RuneTier ) ).Length ][];
        foreach( Rune.RuneTier tier in Enum.GetValues( typeof( Rune.RuneTier ) ) )
        {
            _tieredRunes[ (int) tier ] = runes.Where( r => r.Tier() == tier ).ToArray();
        }
    }

    public Rune RandomRuneOfTier( Rune.RuneTier tier ) => _tieredRunes[ (int) tier ].RandomEntry();

    public Rune RandomShopRune()
    {
        var tier = Random.value switch
        {
            < 0.75f => Rune.RuneTier.Common,
            < 0.95f => Rune.RuneTier.Rare,
            _ => Rune.RuneTier.Legendary
        };
        return RandomRuneOfTier( tier );
    }

    public Rune[] RandomShopRunes( int count )
    {
        var randomRunes = new Rune[ count ];
        var randomIndices = new int[ runes.Length ];
        for( var i = 0; i < randomIndices.Length; i++ )
            randomIndices[ i ] = i;
        
        randomIndices.Shuffle();
        for( var i = 0; i < count; i++ )
            randomRunes[ i ] = runes[ randomIndices[ i ] ];

        return randomRunes;
    }

    public NewRune[] ShopRunes( int count )
    {
        var randomRunes = new NewRune[ count ];
        var randomIndices = new int[ runeIndex.Length ];
        for( var i = 0; i < randomIndices.Length; i++ )
            randomIndices[ i ] = i;
        
        randomIndices.Shuffle();
        for( var i = 0; i < count; i++ )
            randomRunes[ i ] = runeIndex[ randomIndices[ i ] ];

        return randomRunes;
    }

    public IEnumerable AllRunes() => runes;
}
