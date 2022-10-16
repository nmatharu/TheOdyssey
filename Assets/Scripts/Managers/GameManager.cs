using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [ SerializeField ] Transform players;
    
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
}
