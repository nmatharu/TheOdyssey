using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerInputBroadcast : MonoBehaviour
{
    PlayerInput _playerInput;
    LobbyPlayer _lobbyPlayer;
    int _playerId;

    void Start()
    {
        transform.parent = InputManager.Instance.transform;
        _playerInput = GetComponent<PlayerInput>();
        _playerId = LobbyManager.Instance.RequestBinding( this, DeviceName() );
        if( _playerId == -1 )
            Destroy( gameObject );
        _lobbyPlayer = LobbyManager.Instance.GetPlayer( _playerId );
    }

    string DeviceName()
    {
        var deviceNames = ( from d in _playerInput.devices select d.displayName ).ToList();
        return string.Join( " / ", deviceNames );
    }

    public void LobbyMove( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null )  return;
        _lobbyPlayer.InputMovement( context.ReadValue<Vector2>() );
    }

    public void LobbyChangeChar( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered )  return;
        _lobbyPlayer.ChangeCharacter( context.ReadValue<float>() > 0 );
    }

    public void LobbyLDUR( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered )  return;
        _lobbyPlayer.MenuNav( context.ReadValue<Vector2>() );
    }

    public void LobbyEditName( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered )  return;
        _lobbyPlayer.ToggleEditName();
    }

    public void LobbyChangeDifficulty( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered || _playerId != 0 )  return;
        LobbyManager.Instance.CycleDifficulty();
    }

    public void LobbyConfirmButton( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered )  return;
        _lobbyPlayer.ConfirmBtn();
    }

    public void LobbyBackToMenu( InputAction.CallbackContext context )
    {
        if( _lobbyPlayer == null || !context.action.triggered || _playerId != 0 )  return;
        LobbyManager.Instance.BackToMenu();
    }
}
