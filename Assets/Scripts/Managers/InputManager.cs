using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [ SerializeField ] int maxNumPlayers = 3;

    // 
    PlayerControls[] bindings;

    public static InputManager Instance { get; private set; }

    void Awake()
    {
        if( Instance != null && Instance != this )
        {
            Destroy( this );
        }
        else
        {
            Instance = this;
            
            // TODO This should be here, just disabled for demo purposes
            // DontDestroyOnLoad( this );
        }
    }

    void Start() => bindings = new PlayerControls[ maxNumPlayers ];

    public int RequestBinding( PlayerControls controls )
    {
        for( var i = 0; i < maxNumPlayers; i++ )
        {
            // If there is a free binding
            if( bindings[ i ] == null )
            {
                bindings[ i ] = controls;
                return i;
            }
        }
        return -1;
    }
}