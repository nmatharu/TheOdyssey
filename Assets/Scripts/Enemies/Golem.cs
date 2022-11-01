using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Golem : MonoBehaviour
{
    [ SerializeField ] float speed = 3f;

    const float FrameTime = 1 / 60f;

    [ SerializeField ] int damage = 17;
    [ SerializeField ] float smashRange = 12f;

    [ SerializeField ] int smashingFrames = 20;
    [ SerializeField ] int smashActiveFrame = 15;

    [ SerializeField ] ImageFader smashIndicator; 
    [ SerializeField ] ParticleSystem smashPfx;

    Quaternion _movementDirection;
    Queue<Vector2Int> _path;
    Vector3 _currentTargetTile;

    Rigidbody _body;
    Transform _targetT;

    Animator _animator;

    bool _smashing = false;
    bool _smashOnCooldown = false;

    static readonly int AWalk = Animator.StringToHash( "Walk" );
    static readonly int ASmash = Animator.StringToHash( "Smash" );

    [ SerializeField ] float smashBoxCenterDist = 10f;
    [ SerializeField ] Vector3 smashBoxBounds;

    [ SerializeField ] float smashCooldownMin = 4;
    [ SerializeField ] float smashCooldownMax = 6;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();

        InvokeRepeating( nameof( RunPathfinding ), 0, 1f );

        _movementDirection = Quaternion.identity;
        _path = new Queue<Vector2Int>();
    }

    void FixedUpdate()
    {
        var pos = transform.position;
        if( _targetT == null )  return;
        if( !_smashing && !_smashOnCooldown && JBB.DistXZSquared( pos, _targetT.position ) < smashRange * smashRange )
        {
            _smashing = true;
            _smashOnCooldown = true;
            _animator.CrossFade( ASmash, 0.2f );
            smashIndicator.FadeIn();
            this.Invoke( () =>
            {
                smashPfx.Play();
                smashIndicator.FadeOut();

            }, smashActiveFrame * FrameTime );
            this.Invoke( SmashCollision, ( smashActiveFrame + 1 ) * FrameTime );
            this.Invoke( () => _smashing = false, smashingFrames * FrameTime );
            this.Invoke( () => _smashOnCooldown = false, smashingFrames * FrameTime + Random.Range( smashCooldownMin, smashCooldownMax ) );
            
        }

        if( _smashing )
        {
            _body.velocity = Vector3.zero;
            return;
        }

        var defaultTarget = _currentTargetTile == Vector3.zero;
        var onTarget = WorldGenerator.WorldPosToCoords( pos ) == WorldGenerator.WorldPosToCoords( _currentTargetTile );
        if( _path.Count > 0 && ( defaultTarget || onTarget ) )
        {
            _currentTargetTile = WorldGenerator.CoordsToWorldPos( _path.Dequeue() );
        }

        var lookAt = Quaternion.LookRotation( _currentTargetTile - transform.position );
        _movementDirection = Quaternion.Slerp( _movementDirection, lookAt, 1f );

        _body.velocity = _currentTargetTile == Vector3.zero ? Vector3.zero : _movementDirection * Vector3.forward * speed;

        if( _targetT != null )
            transform.LookAt( _targetT );
    }

    void SmashCollision()
    {
        var colliders = Physics.OverlapBox( transform.position + transform.forward * smashBoxCenterDist, smashBoxBounds / 2, transform.rotation );
        foreach( var c in colliders )
        {
            var p = c.GetComponent<Player>();
            if( p != null )
                p.IncomingDamage( damage );
        }
    }
    
    void OnDrawGizmos()
    {
        // Matrix4x4 prevMatrix = Gizmos.matrix;
        // Gizmos.matrix = transform.localToWorldMatrix;
        //
        // Vector3 boxPosition = transform.position + transform.forward * smashBoxCenterDist;
        //
        // // convert from world position to local position 
        // boxPosition = transform.InverseTransformPoint(boxPosition); 
        //
        // Gizmos.DrawWireCube (boxPosition, smashBoxBounds);
        //
        // // restore previous Gizmos settings
        // Gizmos.matrix = prevMatrix;
        
    }

    void FindNewTarget()
    {
        var pos = transform.position;

        // Set closest player to targetPos
        var transforms = GameManager.Instance.Players().Cast<Transform>()
            .Where( p => p.gameObject.activeInHierarchy ).ToArray();

        if( transforms.Length == 0 ) return;

        var closestT = transforms[ 0 ];
        foreach( var t in transforms )
        {
            if( JBB.DistXZSquared( pos, t.position ) < JBB.DistXZSquared( pos, closestT.position ) )
                closestT = t;
        }

        _targetT = closestT;
    }

    void RunPathfinding()
    {
        FindNewTarget();
        PathFindToTarget();
    }

    void PathFindToTarget()
    {
        if( _targetT.position == Vector3.zero ) return;

        var startXY = WorldGenerator.WorldPosToCoords( transform.position );
        var targetXY = WorldGenerator.WorldPosToCoords( _targetT.position );

        // var pathSegment = new List<AStarNode>();
        // var node = WorldGenDemo.PathFind( startXY, targetXY );
        //
        // if( node == null )  return;
        // while( node.Parent != null )
        // {
        //     node = node.Parent;
        //     pathSegment.Add( node );
        // }
        //
        // pathSegment.Reverse();
        // _path.Clear();
        //
        // foreach( var n in pathSegment )
        //     _path.Enqueue( new Vector2Int( n.X, n.Y ) );
        //
        // // FlashPathDebug();
        //
        // if( _path.Count > 0 )
        //     _currentTargetTile = WorldGenerator.CoordsToWorldPos( _path.Dequeue() );

        _path.Clear();
        _path.Enqueue( targetXY );
        _currentTargetTile = WorldGenerator.CoordsToWorldPos( _path.Dequeue() );
    }
}