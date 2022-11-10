using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyPlayer : MonoBehaviour
{
    [ SerializeField ] float speed = 8f;
    [ SerializeField ] TextMeshProUGUI playerName;

    Rigidbody _body;
    Animator _animator;

    // For sequences like the intro run-in and outro run-out where we don't want the player to control
    public bool inputDisabled = false;

    static Vector3 _cameraUp;
    Vector2 _moveInput;
    Vector3 _moveDir;

    static readonly int IsRunning = Animator.StringToHash( "IsRunning" );

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        if( Camera.main == null ) return;
        var fwd = Camera.main.transform.forward;
        _cameraUp = new Vector3( fwd.x, 0, fwd.z );
    }

    public void Init( string pName ) => playerName.text = pName;

    void FixedUpdate()
    {
        var pos = transform.position;

        _moveDir = CameraCompensation( _moveInput );
        _body.velocity = _moveDir * speed;

        _animator.SetBool( IsRunning, _moveDir != Vector3.zero );
        transform.LookAt( pos + _moveDir );
    }

    public void ChangeCharacter( bool toTheRight )
    {
        Debug.Log( "Changing char " + toTheRight );
    }

    public void ToggleEditName()
    {
        Debug.Log( "Toggle Edit" );
    }

    public void MenuNav( Vector2 nav )
    {
        // Debug.Log( "Nav: " + nav );
    }

    public void ConfirmBtn()
    {
        Debug.Log( "Confirm btn" );
    }

    public void InputMovement( Vector2 v ) => _moveInput = v;
    
    static Vector3 CameraCompensation( Vector2 dir )
    {
        if( dir == Vector2.zero ) return Vector3.zero;
        var rotDegrees = Mathf.Atan2( dir.x, dir.y ) * Mathf.Rad2Deg;
        var v3Dir = Quaternion.Euler( 0, rotDegrees, 0 ) * _cameraUp;
        return v3Dir.normalized;
    }

    public string PlayerName() => playerName.text;
}
