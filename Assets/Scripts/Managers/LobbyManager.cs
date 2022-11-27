using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [ SerializeField ] Vector3 playerSpawnPosition;
    [ SerializeField ] LobbyPlayerCanvas[] playerCanvases;
    [ SerializeField ] int maxPlayers = 3;
    [ SerializeField ] GameObject lobbyPlayer;
    
    [ SerializeField ] CanvasGroup[] difficultyCanvases;
    [ SerializeField ] float difficultyCanvasAlpha = 0.3f;

    [ SerializeField ] GameObject gameStartingBanner;
    [ SerializeField ] TextMeshProUGUI gameStartingText;
    [ SerializeField ] Image fadeToBlack;

    // 0: Casual, 1: Normal, 2: Brutal, 3: Unreal (locked by default)
    int _difficulty = 1;

    List<GlobalPlayerInput> _inputs;
    LobbyPlayer[] _players;

    Coroutine _gameStartCountdown;
    AsyncOperation _asyncGameLoad;

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
        StartCoroutine( LoadGame() );
    }

    void Update()
    {
        if( _players.Any( p => p != null ) && _players.All( p => p == null || p.ready ) )
        {
            _gameStartCountdown ??= StartCoroutine( GameStartCountdown() );
        }
        else
        {
            if( _gameStartCountdown != null )
                StopCoroutine( _gameStartCountdown );
            gameStartingBanner.SetActive( false );
            _gameStartCountdown = null;
        }
    }

    IEnumerator GameStartCountdown()
    {
        gameStartingBanner.SetActive( true );
        var wait = new WaitForSeconds( 1f );
        for( var i = 0; i < 3; i++ )
        {
            gameStartingText.text = $"GAME STARTING IN { 3 - i }";
            yield return wait;
        }

        StartCoroutine( ToGame() );
    }

    IEnumerator LoadGame()
    {
        _asyncGameLoad = SceneManager.LoadSceneAsync( "Game" );
        _asyncGameLoad.allowSceneActivation = false;
        yield break;
    }

    IEnumerator ToGame()
    {
        // Destroy( GlobalInputManager.Instance.gameObject );
        GlobalInputManager.Instance.ToGame();

        GameManager.GameConfig.difficulty = _difficulty;
        GameManager.GameConfig.playerCount = _players.Count( p => p != null );
        GameManager.GameConfig.playerNames = _players.Where( p => p != null ).Select( p => p.PlayerName() ).ToList();
        GameManager.GameConfig.playerSkins = _players.Where( p => p != null ).Select( p => p.Costume() ).ToList();
        
        _asyncGameLoad.allowSceneActivation = true;
        yield break;
    }

    public int RequestBinding( GlobalPlayerInput input, string deviceName )
    {
        var id = FirstAvailableId();
        if( id == -1 )
            return -1;
        
        var o = Instantiate( lobbyPlayer, playerSpawnPosition, Quaternion.identity );

        _inputs.Add( input );
        
        _players[ id ] = o.GetComponent<LobbyPlayer>();
        _players[ id ].Init( GetUnusedName(), playerCanvases[ id ] );
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