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
        if( Paused() || _player == null || _player.inputDisabled ) return;
        _player.InputMovement( context.ReadValue<Vector2>() );
    }

    public void Interact( InputAction.CallbackContext context )
    {
        if( Paused() || _player == null || _player.inputDisabled ) return;
        if( context.action.triggered )
            _player.Interact();
    }

    public void Attack1( InputAction.CallbackContext context )
    {
        if( Paused() && _player != null && context.action.triggered )
            GameManager.Instance.AttemptQuitToMenu( _playerId );
        
        if( Paused() || _player == null || _player.inputDisabled ) return;
        if( context.action.triggered )
            _player.LightAttack();
    }

    public void Attack2( InputAction.CallbackContext context )
    {
        if( Paused() || _player == null || _player.inputDisabled ) return;
        if( context.action.triggered )
            _player.SpecialAttack();
    }

    public void Roll( InputAction.CallbackContext context )
    {
        if( Paused() || _player == null || _player.inputDisabled ) return;
        if( context.action.triggered )
            _player.Roll();
    }

    public void Inventory( InputAction.CallbackContext context )
    {
        if( Paused() || _player == null || _player.inputDisabled ) return;
        _player.ShowInventory( context.ReadValue<float>() > 0.9f );
    }

    public void GamePause( InputAction.CallbackContext context )
    {
        if( _player == null || _player.inputDisabled ) return;
        if( context.action.triggered )
            GameManager.Instance.PauseGame( _playerId );
    }

    public void LModifier( InputAction.CallbackContext context )
    {
        if( Paused() || _player == null || _player.inputDisabled ) return;
        if( context.action.triggered )
            _player.CastMagic();
    }
    
    bool Paused() => GameManager.Instance.Paused();
}