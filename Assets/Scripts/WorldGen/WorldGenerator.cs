using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [ SerializeField ] Transform tilesParent;
    [ SerializeField ] Transform edgesParent;
    [ SerializeField ] Transform wallsParent;

    [ SerializeField ] int worldSizeX = 600;
    [ SerializeField ] int worldSizeY = 12;

    float _perlinSeed;
    
    public static WorldGenerator Instance { get; private set; }
    bool _generating;

    void Awake()
    {
        if( Instance != null && Instance != this )
        {
            Destroy( this );
        }
        else
        {
            Instance = this;
        }
    }

    void Start() => StartCoroutine( Generate() );

    IEnumerator Generate()
    {
        _generating = true;


        yield return new WaitForSeconds( 2f );
        _generating = false;
        yield break;
    }

    public bool IsGenerating() => _generating;
}
