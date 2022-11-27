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
        _deviceName = DeviceName();

        var lobby = LobbyManager.Instance;
        var game = InputManager.Instance;
        
        if( lobby != null )
        {
            GlobalInputManager.Instance.ToLobby();
            _playerId = lobby.RequestBinding( this, DeviceName() );
            if( _playerId != -1 )
                _lobbyPlayer = lobby.GetPlayer( _playerId );
        }

        if( GameManager.Instance != null )
        {
            // _playerId = game.RequestBinding(  )
        }
    }

    string DeviceName()
    {
        var deviceNames = ( from d in _playerInput.devices select d.displayName ).ToList();
        return string.Join( " / ", deviceNames );
    }

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
        _lobbyPlayer.EditName();
        _playerInput.SwitchCurrentActionMap( "LobbyType" );
    }

    public void LobbyChangeDifficulty( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered || _playerId != 0 || _lobbyPlayer.ready )  return;
        LobbyManager.Instance.CycleDifficulty();
    }

    public void LobbyConfirmButton( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered )  return;
        _lobbyPlayer.ConfirmBtn();
    }

    public void LobbyBackToMenu( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered || context.canceled || _playerId == -1 )  return;

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
        
        _playerId = lobby.RequestBinding( this, DeviceName() );
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
}