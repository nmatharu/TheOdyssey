using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    GameObject _playerObject;
    Player _player;
    int _playerId;

    void Start()
    {
        transform.parent = InputManager.Instance.transform;
        _playerId = InputManager.Instance.RequestBinding( this );
        if( _playerId == -1 )
        {
            Destroy( gameObject );
        }

        _playerObject = GameManager.Instance.SpawnPlayer( _playerId );
        _player = _playerObject.GetComponent<Player>();
    }

    public void Move( InputAction.CallbackContext context )
    {
        if( _player.inputDisabled ) return;
        _player.InputMovement( context.ReadValue<Vector2>() );
    }

    public void Attack1( InputAction.CallbackContext context )
    {
        if( _player.inputDisabled ) return;
        if( context.action.triggered )
            _player.LightAttack();
    }

    public void Attack2( InputAction.CallbackContext context )
    {
        if( _player.inputDisabled ) return;
        if( context.action.triggered )
            _player.SpecialAttack();
    }

    public void Roll( InputAction.CallbackContext context )
    {
        if( _player.inputDisabled ) return;
        if( context.action.triggered )
            _player.Roll();
    }
}