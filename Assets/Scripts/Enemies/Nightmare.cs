using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Nightmare : MonoBehaviour
{
    [ SerializeField ] float speed = 3f;
    [ SerializeField ] float dashSpeedMultiplier = 3f;
    [ SerializeField ] float dashThreshold = 6f;
    [ SerializeField ] float dashTime = 0.75f;
    [ SerializeField ] float slashTime = 0.25f;
    [ SerializeField ] float slashCooldown = 2.5f;

    [ SerializeField ] ParticleSystem[] pfxs;
    [ SerializeField ] int pfxStartFrame = 2;
    [ SerializeField ] int pfxEndFrame = 10;
    const float FrameTime = 1 / 60f;

    [ SerializeField ] int activeDamageFrame = 10;
    [ SerializeField ] float slashDamageRadius = 2.5f;
    [ SerializeField ] int damage = 9;

    Quaternion _movementDirection;
    Queue<Vector2Int> _path;
    Vector3 _currentTargetTile;
    Vector3 _dashDir;

    Rigidbody _body;
    Transform _targetT;

    Animator _animator;
    bool _slashOnCooldown = true;

    static readonly int AChase = Animator.StringToHash( "Armature|1_Chase" );
    static readonly int ADash = Animator.StringToHash( "Armature|2_Dash" );
    static readonly int ASlash = Animator.StringToHash( "Armature|3_Slash" );

    NightmareState _state;

    enum NightmareState
    {
        Chasing, // Moving towards closest target
        Dashing, // Once in range, moving quickly, direction locked
        Slashing // After dash, slashing around, then returns to chasing with a cooldown before dashing again
    }

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _state = NightmareState.Chasing;

        InvokeRepeating( nameof( RunPathfinding ), 0, 1f );
        this.Invoke( () => _slashOnCooldown = false, Random.Range( 1f, 2f ) );
        
        _movementDirection = Quaternion.identity;
        _path = new Queue<Vector2Int>();
    }

    void FixedUpdate()
    {
        switch( _state )
        {
            case NightmareState.Chasing:
                Chase();
                break;

            case NightmareState.Dashing:
                Dash();
                break;

            case NightmareState.Slashing:
                Slash();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void Chase()
    {
        var pos = transform.position;
        var defaultTarget = _currentTargetTile == Vector3.zero;
        var onTarget = WorldGenerator.WorldPosToCoords( pos ) == WorldGenerator.WorldPosToCoords( _currentTargetTile );
        if( _path.Count > 0 && ( defaultTarget || onTarget ) )
        {
            _currentTargetTile = WorldGenerator.CoordsToWorldPos( _path.Dequeue() );
        }

        var lookAt = Quaternion.LookRotation( _currentTargetTile - transform.position );
        _movementDirection = Quaternion.Slerp( _movementDirection, lookAt, 0.04f );

        _body.velocity = _currentTargetTile == Vector3.zero
            ? Vector3.zero
            : _movementDirection * Vector3.forward * speed;

        if( _targetT != null )
            transform.LookAt( _targetT );

        if( !defaultTarget && !_slashOnCooldown &&
            JBB.DistXZSquared( pos, _targetT.position ) < dashThreshold * dashThreshold )
        {
            _state = NightmareState.Dashing;
            _dashDir = ( _currentTargetTile - transform.position ).normalized;
            transform.LookAt( _currentTargetTile );
            _animator.CrossFade( ADash, 0.2f );
            _slashOnCooldown = true;

            this.Invoke( () =>
            {
                _state = NightmareState.Slashing;
                _animator.CrossFade( ASlash, 0.1f );
                // damage thing
            }, dashTime );

            Invoke( nameof( StartPfx ), dashTime + pfxStartFrame * FrameTime );
            Invoke( nameof( Damage ), dashTime + activeDamageFrame * FrameTime );
            Invoke( nameof( StopPfx ), dashTime + pfxEndFrame * FrameTime );
            this.Invoke( () => _state = NightmareState.Chasing, dashTime + slashTime );
            this.Invoke( () => _slashOnCooldown = false, dashTime + slashTime + slashCooldown );
        }
    }

    void StartPfx()
    {
        foreach( var p in pfxs )
            p.Play();
    }

    void StopPfx()
    {
        foreach( var p in pfxs )
            p.Stop();
    }

    void Damage()
    {
        var colliders = Physics.OverlapSphere( transform.position, slashDamageRadius );
        foreach( var c in colliders )
        {
            var p = c.GetComponent<Player>();
            if( p != null )
            {
                p.IncomingDamage( damage );
            }
        }
    }

    void Dash() => _body.velocity = _dashDir * speed * dashSpeedMultiplier;

    void Slash() => _body.velocity = Vector3.zero;

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

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere( transform.position, slashDamageRadius );
    }
}