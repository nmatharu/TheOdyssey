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
    
    [ SerializeField ] CanvasGroup[] difficultyCanvases;
    [ SerializeField ] float difficultyCanvasAlpha = 0.3f;

    // 0: Casual, 1: Normal, 2: Brutal, 3: Unreal (locked by default)
    int _difficulty = 1;

    List<GlobalPlayerInput> _inputs;
    LobbyPlayer[] _players;

    public static LobbyManager Instance { get; private set; }
    
    void Start()
    {
        if( Instance != null && Instance != this )
            Destroy( this );
        else
            Instance = this;

        _inputs = new List<GlobalPlayerInput>();
        _players = new LobbyPlayer[ maxPlayers ];
        HighlightDifficulty( _difficulty );

        Application.targetFrameRate = 240;
    }
    
    public int RequestBinding( GlobalPlayerInput input, string deviceName )
    {
        var id = FirstAvailableId();
        if( id == -1 )
            return -1;
        
        var o = Instantiate( lobbyPlayer, playerSpawnPosition, Quaternion.identity );

        _inputs.Add( input );
        
        _players[ id ] = o.GetComponent<LobbyPlayer>();
        _players[ id ].Init( GetUnusedName() );
        playerCanvases[ id ].Init( input, _players[ id ].PlayerName(), deviceName );
        
        return id;
    }

    int FirstAvailableId()
    {
        for( var i = 0; i < _players.Length; i++ )
            if( _players[ i ] == null )
                return i;
        return -1;
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
        _difficulty = ( _difficulty + 1 ) % 4;
        HighlightDifficulty( _difficulty );
    }

    void HighlightDifficulty( int index )
    {
        for( var i = 0; i < difficultyCanvases.Length; i++ )
            difficultyCanvases[ i ].alpha = i == index ? 1 : difficultyCanvasAlpha;
    }

    public void BackToMenu()
    {
        GlobalInputManager.Instance.ToMenu();
        SceneManager.LoadScene( "Menu" );
    }

    public void RemovePlayer( GlobalPlayerInput input, int playerId )
    {
        _inputs.Remove( input );
        Destroy( _players[ playerId ].gameObject );
        _players[ playerId ] = null;
        playerCanvases[ playerId ].Reset();
        
        if( _inputs.Empty() )
            BackToMenu();
    }
}