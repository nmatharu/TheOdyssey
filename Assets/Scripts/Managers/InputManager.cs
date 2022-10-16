using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
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
            DontDestroyOnLoad( this );
        }
    }
}
