using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyPlayer : MonoBehaviour
{
    [ SerializeField ] float speed = 8f;
    [ SerializeField ] TextMeshProUGUI playerName;
    [ SerializeField ] GameObject[] costumes;

    LobbyPlayerCanvas _canvas;
    Rigidbody _body;
    Animator _animator;

    public bool ready = false;

    // For sequences like the intro run-in and outro run-out where we don't want the player to control
    public bool inputDisabled = false;

    static Vector3 _cameraUp;
    Vector2 _moveInput;
    Vector3 _moveDir;

    int _costumeIndex = 0;

    string _secretCode = "";

    static readonly int IsRunning = Animator.StringToHash( "IsRunning" );

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        if( Camera.main == null ) return;
        var fwd = Camera.main.transform.forward;
        _cameraUp = new Vector3( fwd.x, 0, fwd.z );
    }

    void EnableCostume( int i )
    {
        foreach( var c in costumes )
            c.SetActive( false );
        costumes[ i ].SetActive( true );
    }

    public void Init( string pName, LobbyPlayerCanvas canvas )
    {
        playerName.text = pName;
        _canvas = canvas;
    }

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
        _costumeIndex = ( _costumeIndex + costumes.Length + ( toTheRight ? 1 : -1 ) ) % costumes.Length;
        EnableCostume( _costumeIndex );
    }

    public void EditName() => _canvas.ToNameEntry();

    public bool FinishEditName() => _canvas.FinishEditName();

    public void MenuNav( Vector2 nav ) => _canvas.MenuNav( Vector2Int.RoundToInt( nav ) );

    public void ConfirmBtn()
    {
        ready = !ready;
        _canvas.SetReady( ready );
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

    // This is just for fun, secret character costumes if you enter a particular DPad Code
    public void DPadCode( Vector2 readValue )
    {
        var r = Vector2Int.RoundToInt( readValue );
        var c = ' ';
        c = r.x == -1 ? 'L' : r.x == 1 ? 'R' : ' ';
        c = r.y == -1 ? 'D' : r.y == 1 ? 'U' : ' ';

        _secretCode += c;
        if( _secretCode.Length > 4 )
            _secretCode = _secretCode.Substring( 1, 4 );
        
        // if secret code == whatever, set a custom costume
    }

    public void WobbleCanvas( Vector2 readValue ) => _canvas.Wobble( readValue );
}
