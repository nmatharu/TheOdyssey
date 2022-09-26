using System;
using UnityEngine;

public class AnimationMovement : MonoBehaviour
{
    [ SerializeField ] bool disableInput = false;
    [ SerializeField ] float speed = 5f;
    Rigidbody _body;
    Animator _animator;
    static Vector3 cameraUp;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        var fwd = Camera.main.transform.forward;
        cameraUp = new Vector3( fwd.x, 0, fwd.z );
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.D ) )
        {
            // die
            
        }
    }

    void FixedUpdate()
    {
        var x = Input.GetAxis( "Horizontal" );
        var y = Input.GetAxis( "Vertical" );

        var dir = new Vector2( x, y ).normalized;
        dir = disableInput ? Vector2.zero : dir;
        
        _animator.SetBool( "IsRunning", dir != Vector2.zero );
        var v3Dir = CameraCompensation( dir );
        
        _body.velocity = v3Dir * speed;
        transform.LookAt( transform.position + v3Dir );
    }

    Vector3 CameraCompensation( Vector2 dir )
    {
        if( dir == Vector2.zero )   return Vector3.zero;
        var rotDegrees = Mathf.Atan2( dir.x, dir.y ) * Mathf.Rad2Deg;
        var v3Dir = Quaternion.Euler( 0, rotDegrees, 0 ) * cameraUp;
        return v3Dir.normalized;
    }
}