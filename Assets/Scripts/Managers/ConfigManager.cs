using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager Instance { get; private set; }

    bool _trailerMode;
    
    void Awake()
    {
        if( Instance != null && Instance != this )
        {
            Destroy( gameObject );
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad( this );
        }
    }

    void Start()
    {
        _trailerMode = false;
    }

    public bool TrailerMode() => _trailerMode;
}
