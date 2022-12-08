using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Skull : MonoBehaviour
{
    [ SerializeField ] float speed = 3f;
    [ SerializeField ] GameObject fireballPrefab;
    [ SerializeField ] Transform mouthPos;
    [ SerializeField ] ParticleSystem mouthFire;
    [ SerializeField ] float fireballFireRate = 2.5f;
    AudioSource _chargeSfx;

    Quaternion _movementDirection;
    Queue<Vector2Int> _path;
    Vector3 _currentTargetTile;
    Transform _targetPlayer;

    Rigidbody _body;
    Enemy _enemy;
    
    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _enemy = GetComponent<Enemy>();
        
        InvokeRepeating( nameof( RunPathfinding ), 0, 1f );
        InvokeRepeating( nameof( FireFireball ), Random.Range( 0f, 1f ), fireballFireRate );

        _movementDirection = Quaternion.identity;
        _path = new Queue<Vector2Int>();
        _chargeSfx = GetComponent<AudioSource>();
    }

    void FixedUpdate()
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

        _body.velocity = _currentTargetTile == Vector3.zero ? Vector3.zero : _movementDirection * Vector3.forward * speed;

        if( _targetPlayer != null )
            transform.LookAt( _targetPlayer );
    }

    void FindNewTarget()
    {
        var pos = transform.position;

        // Set closest player to targetPos
        var players = GameManager.Instance.Players();
        if( players.Empty() )   return;
        var closestPlayer = players[ 0 ].transform;
        
        foreach( var p in players )
            if( JBB.DistXZSquared( pos, p.transform.position ) < JBB.DistXZSquared( pos, closestPlayer.position ) )
                closestPlayer = p.transform;

        _targetPlayer = closestPlayer;
    }

    void RunPathfinding()
    {
        FindNewTarget();
        PathFindToTarget();
    }

    void PathFindToTarget()
    {
        if( _targetPlayer.position == Vector3.zero ) return;
        
        var startXY = WorldGenerator.WorldPosToCoords( transform.position );
        var targetXY = WorldGenerator.WorldPosToCoords( _targetPlayer.position );

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

    void FlashPathDebug()
    {
        foreach( var n in _path )
        {
            var mat = WorldGenDemo.TileIndices[ n.x, n.y ].GetComponentInChildren<Renderer>().material;
            var oldColor = mat.color;
            mat.color = Color.white;
            this.Invoke( () => mat.color = oldColor, 0.5f );
        }
    }

    void FireFireball()
    {
        if( _targetPlayer == null ) return;

        _chargeSfx.Play();
        const float indicatorFrames = 75f;
        mouthFire.Play();
        this.Invoke( () =>
        {
            var t = transform;
            var o = Instantiate( fireballPrefab, mouthPos.position, t.rotation, GameManager.Instance.Projectiles() );
            var fireball = o.GetComponent<SkullFireball>();
            fireball.Init( _enemy.Level(), _targetPlayer.position );
        }, indicatorFrames / 50f );
        
    }
}