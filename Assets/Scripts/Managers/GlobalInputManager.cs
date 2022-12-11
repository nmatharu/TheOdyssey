using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalInputManager : MonoBehaviour
{
    public static GlobalInputManager Instance { get; private set; }

    PlayerInputManager _inputManager;
    List<PlayerInput> _inputs;

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

        _inputManager = GetComponent<PlayerInputManager>();
        _inputs = new List<PlayerInput>();
    }

    public void PlayerJoined( PlayerInput playerInput )
    {
        if( GlobalPlayerInput.IsMouseKeyboardInput( playerInput ))
            return;
        
        playerInput.gameObject.transform.parent = transform;
        _inputs.Add( playerInput );
    }

    public void FindAndBind( Player player, int pId )
    {
        foreach( Transform t in transform )
        {
            var input = t.GetComponent<GlobalPlayerInput>();
            if( input != null && input.PId() == pId )
                input.BindGamePlayer( player );
        }
    }

    public void ToMenu()
    {
        SwitchActionMap( "MenuNav" );
        DetachAllPlayers();
    }

    public void DetachAllPlayers()
    {
        foreach( Transform t in transform )
        {
            var input = t.GetComponent<GlobalPlayerInput>();
            if( input != null )
                input.Detach();
        }
    }

    public void ToLobby() => SwitchActionMap( "Lobby" );
    public void ToGame() => SwitchActionMap( "InGame" );

    void SwitchActionMap( string map )
    {
        foreach( var input in _inputs )
        {
            input.defaultActionMap = map;
            input.SwitchCurrentActionMap( input.defaultActionMap );
        }
    }

    public static Vector2Int InterpretCardinals( Vector2 nav ) => new( (int) nav.x, (int) nav.y );
}