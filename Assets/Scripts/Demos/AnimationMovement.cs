using System;
using System.Collections;
using UnityEngine;

public class AnimationMovement : MonoBehaviour
{
    [ SerializeField ] bool disableInput = false;
    [ SerializeField ] float speed = 5f;
    [ SerializeField ] float rollSpeedMultiplier = 2f;

    [ SerializeField ] KeyCode lKey;
    [ SerializeField ] KeyCode rKey;

    [ SerializeField ] Animator animator;

    [ SerializeField ] ParticleSystem swordPfx;
    
    Rigidbody _body;
    Animator _animator;
    static Vector3 cameraUp;
    bool locked = false;
    bool rolling = false;
    float attackJabSpeedMultiplier = 1;
    Vector3 movementDir;

    int swingI = 0;
    
    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        var fwd = Camera.main.transform.forward;
        cameraUp = new Vector3( fwd.x, 0, fwd.z );
    }

    void Update()
    {
        if( swordPfx == null )  return;
        
        if( !locked && ( Input.GetKeyDown( KeyCode.Z ) || Input.GetButtonDown( "Fire3" ) ) )
        {
            var animStr = swingI switch
            {
                0 => "Armature|5_SwingA",
                1 => "Armature|5_SwingB",
                _ => "Armature|5_SwingC"
            };
            swingI = ( swingI + 1 ) % 3;
            
            // animator.CrossFade( animStr, 0.02f );
            animator.Play( animStr );
            
            LockFor( 12 );
        }
        if( !locked && ( Input.GetKeyDown( KeyCode.X ) || Input.GetButtonDown( "Jump" ) ) )
        {
            animator.Play( "Armature|7_Heavy_A_Default" );
            StartCoroutine( HeavyAttack() );
            // animator.CrossFade( "Armature|5_SwingB", 0.02f );
            // LockFor( 15 );
        }

        // if( Input.GetKeyDown( KeyCode.X ) || Input.GetButtonDown( "Jump" ) )
        // {
        //     animator.CrossFade( "Armature|4_SwingB_Legless", 0.02f );
        //     LockFor( 8 );
        // }

        if( !rolling && ( Input.GetKeyDown( KeyCode.C ) || Input.GetButtonDown( "Fire2" ) ) )
        {
            animator.Play( "Armature|6_Roll" );
            rolling = true;
            this.Invoke( () => rolling = false, 0.3f );
            // Debug.Log( "ROLL" );
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
        
        // Debug.Log( $"On {x}, {y}, Tile: {tile}" );
    }

    void FixedUpdate()
    {
        if( disableInput )
        {
            var xDir = Input.GetKey( lKey ) ? -1 : Input.GetKey( rKey ) ? 1 : 0;
            _body.velocity = CameraCompensation( new Vector2( xDir, 0 ) ) * 8f;
            transform.LookAt( transform.position + _body.velocity );
            _animator.SetBool( "IsRunning", _body.velocity != Vector3.zero );
            return;
        }

        var x = Input.GetAxis( "Horizontal" );
        var y = Input.GetAxis( "Vertical" );

        var dir = new Vector2( x, y ).normalized;
        dir = disableInput ? Vector2.zero : dir;

        if( rolling )
        {
            _body.velocity = movementDir * ( rollSpeedMultiplier * speed );
            return;
        }

        // movementDir = locked ? movementDir : CameraCompensation( dir );
        movementDir = locked ? movementDir : CameraCompensation( dir );

        _body.velocity = movementDir * ( ( locked ? 0 : 1 ) * speed * attackJabSpeedMultiplier );
        _animator.SetBool( "IsRunning", movementDir != Vector3.zero );
        
        transform.LookAt( transform.position + movementDir );
    }

    void LateUpdate()
    {
        // const float timeStep = 0.002f;
        //
        // if( swordPfx == null )  return;
        // for( var timeElapsed = 0f; timeElapsed < Time.deltaTime; timeElapsed += timeStep )
        // {
        //     swordPfx.Simulate( timeStep );
        // }
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
        swordPfx.Play();
        for( var i = 0; i < frames60; i++ )
        {
            if( i == 2 )
            {
                Collider[] colliders = Physics.OverlapSphere( transform.position, 2f );
                foreach( var c in colliders )
                {
                    if( c.GetComponent<EnemyDemo>() != null )
                    {
                        c.GetComponent<EnemyDemo>().TakeHit();
                    }
                }
            }

            if( i == (int) ( frames60 - 3 ) )
            {
                swordPfx.Stop();
            }

            attackJabSpeedMultiplier = 1 + 0.5f * ( frames60 - i ) / frames60;
            yield return new WaitForFixedUpdate();
        }

        attackJabSpeedMultiplier = 1;
        
        locked = false;
    }

    IEnumerator HeavyAttack()
    {
        var w = new WaitForSeconds( 1f / 24f );
        locked = true;
        
        var sh = swordPfx.shape;
        var em = swordPfx.emission;
        var oldScale = sh.scale;
        var oldPos = sh.position;
        var oldRateOverTime = em.rateOverTime;
        sh.scale = new Vector3( 0, 0, 8 );
        sh.position = new Vector3( 0, -0.5f, 8 );
        em.rateOverTime = new ParticleSystem.MinMaxCurve( 150 );
        
        for( var i = 0; i < 15; i++ )
        {
            switch( i )
            {
                case 5:
                    swordPfx.Play();
                    break;
                case 8:
                    swordPfx.Stop();
                    break;
            }

            yield return w;
        }

        sh.scale = oldScale;
        sh.position = oldPos;
        em.rateOverTime = oldRateOverTime;
        
        swordPfx.Stop();
        locked = false;
    }
}