using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [ SerializeField ] Transform players;
    [ SerializeField ] Transform enemies;
    [ SerializeField ] GameObject[] playersArr;
    [ SerializeField ] Transform projectiles;

    // TEMP
    [ SerializeField ] GameObject skullEnemy;
    
    public static GameManager Instance { get; private set; }
    void Awake()
    {
        if( Instance != null && Instance != this )
        {
            Destroy( this );
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad( this );
        }
        
        InvokeRepeating( nameof( SpawnSkull ), 2f, 8f );
    }

    void Start()
    {
        
    }

    void SpawnSkull()
    {
        var maxX = playersArr.Max( p => p.transform.position.x );
        for( var i = 0; i < NumPlayers(); i++ )
        {
            Instantiate( skullEnemy, new Vector3( maxX + 30f, 0, Random.Range( 2, 20 ) ), 
                Quaternion.identity, enemies );
        }
        
    }

    // This implementation is for testing purposes
    public GameObject SpawnPlayer( int playerId )
    {
        playersArr[ playerId ].SetActive( true );
        return playersArr[ playerId ];
    }

    public int NumPlayers() => playersArr.Count( p => p.activeInHierarchy );

    public Transform Projectiles() => projectiles;
    public Transform Players() => players;
}
