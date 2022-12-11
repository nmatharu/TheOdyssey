using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GlobalPlayerInput : MonoBehaviour
{
    PlayerInput _playerInput;
    string _deviceName;
    int _playerId = -1;
    
    LobbyPlayer _lobbyPlayer;
    Player _gamePlayer;

    void Start()
    {
        _playerInput = GetComponent<PlayerInput>();
        _deviceName = DeviceName( _playerInput );

        if( IsMouseKeyboardInput( _playerInput ) )
        {
            Destroy( gameObject );
            return;
        }

        var lobby = LobbyManager.Instance;
        
        if( lobby != null )
        {
            GlobalInputManager.Instance.ToLobby();
            _playerId = lobby.RequestBinding( this, DeviceName( _playerInput ) );
            if( _playerId != -1 )
                _lobbyPlayer = lobby.GetPlayer( _playerId );
        }

        if( GameManager.Instance != null )
        {
            // _playerId = game.RequestBinding(  )
        }
    }

    static string DeviceName( PlayerInput p )
    {
        var deviceNames = ( from d in p.devices select d.displayName ).ToList();
        return string.Join( " / ", deviceNames );
    }

    public static bool IsMouseKeyboardInput( PlayerInput p ) => DeviceName( p ).ToLower().Contains( "mouse" );

    public void MenuLDUR( InputAction.CallbackContext context )
    {
        if( MenuManager.Instance == null || !context.action.triggered ) return;
        MenuManager.Instance.MenuNavLDUR( GlobalInputManager.InterpretCardinals( context.ReadValue<Vector2>() ) );
    }

    public void MenuConfirm( InputAction.CallbackContext context )
    {
        if( MenuManager.Instance == null || !context.action.triggered ) return;
        MenuManager.Instance.ConfirmSelection();
    }

    public void MenuBack( InputAction.CallbackContext context )
    {
        if( MenuManager.Instance == null || !context.action.triggered ) return;
        MenuManager.Instance.BackSelection();
    }
    
    public void LobbyMove( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null )  return;
        _lobbyPlayer.InputMovement( context.ReadValue<Vector2>() );
        if( _lobbyPlayer.ready )
            _lobbyPlayer.WobbleCanvas( context.ReadValue<Vector2>() );
    }

    public void LobbyChangeChar( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered || _lobbyPlayer.ready )  return;
        _lobbyPlayer.ChangeCharacter( context.ReadValue<float>() > 0 );
    }

    public void LobbyEditName( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered || _lobbyPlayer.ready )  return;
        
        // TODO Implement name entry
        // return;
        
        // _lobbyPlayer.EditName();
        // _playerInput.SwitchCurrentActionMap( "LobbyType" );
    }

    public void LobbyChangeDifficulty( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered || _lobbyPlayer.ready )  return;
        LobbyManager.Instance.CycleDifficulty( _playerId );
    }

    public void LobbyConfirmButton( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered )  return;
        _lobbyPlayer.ConfirmBtn();
    }

    public void LobbyBackToMenu( InputAction.CallbackContext context )
    {
        if( !context.action.triggered || context.canceled )  return;
        LobbyManager.Instance.CheckIfReturnToMenu();
        if( _lobbyPlayer == null || _playerId == -1 )   return;

        if( _lobbyPlayer.ready )
        {
            _lobbyPlayer.ConfirmBtn();
        }
        else
        {
            LobbyManager.Instance.RemovePlayer( this, _playerId );
            _playerId = -1;
        }

        
        // LobbyManager.Instance.BackToMenu();
    }

    public void LobbyJoin( InputAction.CallbackContext context )
    {
        if( !context.action.triggered || _lobbyPlayer != null ) return;
        
        var lobby = LobbyManager.Instance;
        if( lobby == null ) return;
        
        _playerId = lobby.RequestBinding( this, DeviceName( _playerInput ) );
        if( _playerId != -1 )
            _lobbyPlayer = lobby.GetPlayer( _playerId );
    }
    
    public void LobbyDPadCode( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null )  return;
        _lobbyPlayer.DPadCode( context.ReadValue<Vector2>() );
    }
    
    public void LobbyTypeLDUR( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered )  return;
        _lobbyPlayer.MenuNav( context.ReadValue<Vector2>() );
    }

    public void LobbyTypeConfirm( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered )  return;
        var success = _lobbyPlayer.FinishEditName();
        if( success )
            _playerInput.SwitchCurrentActionMap( "Lobby" );
    }
    
    public void GameMove( InputAction.CallbackContext context )
    {
        if( Paused() || _gamePlayer == null || _gamePlayer.inputDisabled ) return;
        _gamePlayer.InputMovement( context.ReadValue<Vector2>() );
    }

    public void GameInteract( InputAction.CallbackContext context )
    {
        if( Paused() || _gamePlayer == null || _gamePlayer.inputDisabled ) return;
        if( context.action.triggered )
            _gamePlayer.Interact();
    }

    public void GameAttack1( InputAction.CallbackContext context )
    {
        if( Paused() && _gamePlayer != null && context.action.triggered )
            GameManager.Instance.AttemptQuitToMenu( _playerId );
        
        if( Paused() || _gamePlayer == null || _gamePlayer.inputDisabled ) return;
        if( context.action.triggered )
            _gamePlayer.LightAttack();
    }

    public void GameAttack2( InputAction.CallbackContext context )
    {
        if( Paused() || _gamePlayer == null || _gamePlayer.inputDisabled ) return;
        if( context.action.triggered )
            _gamePlayer.SpecialAttack();
    }

    public void GameRoll( InputAction.CallbackContext context )
    {
        if( Paused() || _gamePlayer == null || _gamePlayer.inputDisabled ) return;
        if( context.action.triggered )
            _gamePlayer.Roll();
    }

    public void GameInventory( InputAction.CallbackContext context )
    {
        if( Paused() || _gamePlayer == null || _gamePlayer.inputDisabled ) return;
        _gamePlayer.ShowInventory( context.ReadValue<float>() > 0.9f );
    }

    public void GamePause( InputAction.CallbackContext context )
    {
        if( _gamePlayer == null || _gamePlayer.inputDisabled ) return;
        if( context.action.triggered )
            GameManager.Instance.PauseGame( _playerId );
    }

    public void GameMagic( InputAction.CallbackContext context )
    {
        if( Paused() || _gamePlayer == null || _gamePlayer.inputDisabled ) return;
        if( context.action.triggered )
            _gamePlayer.CastMagic();
    }
    
    bool Paused() => GameManager.Instance.Paused();
    public int PId() => _playerId;

    public void BindGamePlayer( Player player ) => _gamePlayer = player;

    public void Detach()
    {
        _playerId = -1;
        _lobbyPlayer = null;
        _gamePlayer = null;
    }
}