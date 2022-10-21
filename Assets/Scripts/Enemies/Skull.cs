using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Skull : MonoBehaviour
{
    [ SerializeField ] float speed = 3f;
    [ SerializeField ] GameObject fireballPrefab;
    [ SerializeField ] Transform mouthPos;
    [ SerializeField ] float fireballFireRate = 2.5f;

    Quaternion _movementDirection;
    Queue<Vector2Int> _path;
    Vector3 _currentTargetTile;

    Rigidbody _body;
    Transform _targetT;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        InvokeRepeating( nameof( RunPathfinding ), 0, 1f );
        InvokeRepeating( nameof( FireFireball ), 1 + 2 * Random.value, fireballFireRate );

        _movementDirection = Quaternion.identity;
        _path = new Queue<Vector2Int>();
    }

    void FixedUpdate()
    {
        var pos = transform.position;
        if( AnimationMovement.WhichTileAmIOn( pos ) == AnimationMovement.WhichTileAmIOn( _currentTargetTile ) &&
            _path.Count > 0 )
        {
            _currentTargetTile = WorldGenerator.CoordsToWorldPos( _path.Dequeue() );
        }

        var lookAt = Quaternion.LookRotation( _currentTargetTile - transform.position );
        _movementDirection = Quaternion.Slerp( _movementDirection, lookAt, 0.01f );

        _body.velocity = _movementDirection * Vector3.forward * speed;

        if( _targetT != null )
            transform.LookAt( _targetT );
    }

    void FindNewTarget()
    {
        var pos = transform.position;

        // Set closest player to targetPos
        var transforms = GameManager.Instance.Players().Cast<Transform>()
            .Where( p => p.gameObject.activeInHierarchy ).ToArray();
        
        if( transforms.Length == 0 )    return;

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
        var startXY = AnimationMovement.WhichTileAmIOn( transform.position );
        var targetXY = AnimationMovement.WhichTileAmIOn( _targetT.position );

        var pathSegment = new List<AStarNode>();
        var node = WorldGenDemo.PathFind( startXY, targetXY );

        if( node == null )  return;
        while( node.Parent != null )
        {
            node = node.Parent;
            pathSegment.Add( node );
        }

        // JBB.LogQueue( _path );

        pathSegment.Reverse();
        _path.Clear();

        foreach( var n in pathSegment )
            _path.Enqueue( new Vector2Int( n.X, n.Y ) );

        // FlashPathDebug();
        
        if( _path.Count > 0 )
            _currentTargetTile = WorldGenerator.CoordsToWorldPos( _path.Dequeue() );

        // JBB.LogQueue( _path );
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
        if( _targetT == null ) return;
        var t = transform;
        var o = Instantiate( fireballPrefab, mouthPos.position, t.rotation, GameManager.Instance.Projectiles() );
        var fireball = o.GetComponent<SkullFireball>();
        fireball.TargetDirection( _targetT.position );
    }
}