using System;
using UnityEngine;

public class AnimationMovement : MonoBehaviour
{
    [ SerializeField ] float speed = 5f;
    Rigidbody _body;
    Animator _animator;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        float x = Input.GetKey( KeyCode.D ) ? 1 : Input.GetKey( KeyCode.A ) ? -1 : 0;
        float y = Input.GetKey( KeyCode.W ) ? 1 : Input.GetKey( KeyCode.S ) ? -1 : 0;
        
        x = Input.GetAxis( "Horizontal" );
        y = Input.GetAxis( "Vertical" );
        
        var dir = new Vector2( x, y ).normalized;
        var v3Dir = new Vector3( dir.x, 0, dir.y );

        Debug.Log( dir );
        _animator.SetBool( "IsRunning", dir != Vector2.zero );

        _body.velocity = new Vector3( dir.x, 0, dir.y ) * speed;
        transform.LookAt( transform.position + v3Dir );
    }
}