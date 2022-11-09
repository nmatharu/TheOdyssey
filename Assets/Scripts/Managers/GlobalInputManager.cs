using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalInputManager : MonoBehaviour
{
    public static GlobalInputManager Instance { get; private set; }

    PlayerInputManager _inputManager;
    List<PlayerInput> _inputs;

    void Start()
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

        _inputManager = GetComponent<PlayerInputManager>();
        _inputs = new List<PlayerInput>();
    }

    public void PlayerJoined( PlayerInput playerInput )
    {
        playerInput.gameObject.transform.parent = transform;
        _inputs.Add( playerInput );
    }

    public void ToMenu()
    {
        foreach( var input in _inputs )
        {
            input.defaultActionMap = "MenuNav";
            input.SwitchCurrentActionMap( input.defaultActionMap );
        }
    }

    public void ToLobby()
    {
        foreach( var input in _inputs )
        {
            input.defaultActionMap = "Lobby";
            input.SwitchCurrentActionMap( input.defaultActionMap );
        }
    }

    public void ToGame()
    {
        foreach( var input in _inputs )
        {
            input.defaultActionMap = "InGame";
            input.SwitchCurrentActionMap( input.defaultActionMap );
        }
    }

    public static Vector2Int InterpretCardinals( Vector2 nav ) => new( (int) nav.x, (int) nav.y );
}