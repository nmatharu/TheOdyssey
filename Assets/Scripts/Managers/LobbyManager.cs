using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [ SerializeField ] Vector3 playerSpawnPosition;
    [ SerializeField ] LobbyPlayerCanvas[] playerCanvases;
    [ SerializeField ] int maxPlayers = 3;
    [ SerializeField ] GameObject lobbyPlayer;

    // 0: Casual, 1: Normal, 2: Brutal, 3: Unreal (locked by default)
    int _difficulty = 1;

    List<PlayerInputBroadcast> _inputs;
    LobbyPlayer[] _players;

    public static LobbyManager Instance { get; private set; }
    
    void Start()
    {
        if( Instance != null && Instance != this )
            Destroy( this );
        else
            Instance = this;

        _inputs = new List<PlayerInputBroadcast>();
        _players = new LobbyPlayer[ maxPlayers ];
    }

    public void PlayerJoined( PlayerInput input )
    {
    }

    public void PlayerLeft( PlayerInput input )
    {
        Debug.Log( input.devices.ToCommaSeparatedString() + " Left" );
    }

    public int RequestBinding( PlayerInputBroadcast input, string deviceName )
    {
        if( _inputs.Count > maxPlayers )
            return -1;
        
        var o = Instantiate( lobbyPlayer, playerSpawnPosition, Quaternion.identity );

        _inputs.Add( input );
        var playerId = _inputs.Count - 1;
        
        _players[ playerId ] = o.GetComponent<LobbyPlayer>();
        _players[ playerId ].Init( GetUnusedName() );
        playerCanvases[ playerId ].Init( input, _players[ playerId ].PlayerName(), deviceName );
        
        return _inputs.Count - 1;
    }

    string GetUnusedName()
    {
        var pName = "P1";
        for( var i = 2; NameInUse( pName ); i++ )
        {
            pName = "P" + i;
            if( i == 100 ) break;
        }
        return pName;
    }
    bool NameInUse( string n ) => _players.Where( lp => lp != null ).Any( lp => lp.PlayerName() == n );

    public LobbyPlayer GetPlayer( int index ) => _players[ index ];

    public void CycleDifficulty()
    {
        _difficulty = ( _difficulty + 1 ) % 3;
        Debug.Log( "Difficulty is now " + _difficulty );
    }

    public void BackToMenu() => SceneManager.LoadScene( "Menu" );
}