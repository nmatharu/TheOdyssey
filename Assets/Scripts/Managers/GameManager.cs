using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [ SerializeField ] Transform players;
    [ SerializeField ] GameObject[] playersArr;
    [ SerializeField ] Transform projectiles;

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
    }

    void Start()
    {
        
    }

    // This implementation is for testing purposes
    public GameObject SpawnPlayer( int playerId )
    {
        playersArr[ playerId ].SetActive( true );
        return playersArr[ playerId ];
    }

    public Transform Projectiles() => projectiles;
    public Transform Players() => players;
}
