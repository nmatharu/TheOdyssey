using System;
using System.Collections;
using UnityEngine;

public class AnimationMovement : MonoBehaviour
{
    [ SerializeField ] bool disableInput = false;
    [ SerializeField ] float speed = 5f;

    [ SerializeField ] Animator animator;
    Rigidbody _body;
    Animator _animator;
    static Vector3 cameraUp;
    bool locked = false;
    float attackJabSpeedMultiplier = 1;
    Vector3 movementDir;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        var fwd = Camera.main.transform.forward;
        cameraUp = new Vector3( fwd.x, 0, fwd.z );
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.Z ) || Input.GetButtonDown( "Fire3" ) )
        {
            animator.CrossFade( "Armature|4_SwingA_Legless", 0.02f );
            LockFor( 8 );
        }

        if( Input.GetKeyDown( KeyCode.X ) || Input.GetButtonDown( "Jump" ) )
        {
            animator.CrossFade( "Armature|4_SwingB_Legless", 0.02f );
            LockFor( 8 );
        }

        if( Input.GetKeyDown( KeyCode.C ) || Input.GetButtonDown( "Fire2" ) )
        {
            Debug.Log( "ROLL" );
        }

        WhichTileAmIOn();
    }

    void WhichTileAmIOn()
    {
        var pos = transform.position;
        var x = (int) ( pos.x + 1 ) / 2;
        var y = (int) ( pos.z + 1 ) / 2;

        var tile = WorldGenDemo.Tiles[ x, y ];
        // var tile = WorldGenDemo.TileIndices[ x, y ];
        // tile.GetComponentInChildren<Renderer>().material.color = Color.white;
        Debug.Log( $"On {x}, {y}, Tile: {tile}" );
    }

    void FixedUpdate()
    {
        var x = Input.GetAxis( "Horizontal" );
        var y = Input.GetAxis( "Vertical" );

        var dir = new Vector2( x, y ).normalized;
        dir = disableInput ? Vector2.zero : dir;

        _animator.SetBool( "IsRunning", dir != Vector2.zero );
        // movementDir = locked ? movementDir : CameraCompensation( dir );
        movementDir = locked ? movementDir : CameraCompensation( dir );

        _body.velocity = movementDir * ( speed * attackJabSpeedMultiplier );
        transform.LookAt( transform.position + movementDir );
    }

    Vector3 CameraCompensation( Vector2 dir )
    {
        if( dir == Vector2.zero ) return Vector3.zero;
        var rotDegrees = Mathf.Atan2( dir.x, dir.y ) * Mathf.Rad2Deg;
        var v3Dir = Quaternion.Euler( 0, rotDegrees, 0 ) * cameraUp;
        return v3Dir.normalized;
    }

    void LockFor( int frames ) => StartCoroutine( LockForCoroutine( frames ) );

    void LockForAnim( int animFrames ) => StartCoroutine( LockForCoroutine( 60f / 24f * animFrames ) );

    IEnumerator LockForCoroutine( float frames60 )
    {
        locked = true;
        for( var i = 0; i < frames60; i++ )
        {
            attackJabSpeedMultiplier = 1 + 0.5f * ( frames60 - i ) / frames60;
            yield return new WaitForFixedUpdate();
        }

        attackJabSpeedMultiplier = 1;
        locked = false;
    }
}