using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDemo : MonoBehaviour
{
    [ SerializeField ] Transform playersParent;
    [ SerializeField ] float speed = 3f;
    [ SerializeField ] Image hpBar;

    Queue<Vector2Int> _path;
    Vector3 _currentTargetTile;

    Rigidbody _body;
    Transform _targetT;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        InvokeRepeating( nameof( RunPathfinding ), 0, 1f );

        _path = new Queue<Vector2Int>();
        _path.Enqueue( Vector2Int.one );
        _path.Enqueue( new Vector2Int( 2, 4 ) );
        _currentTargetTile = CoordsToWorldPosition( _path.Dequeue() );
    }

    void FixedUpdate()
    {
        var pos = transform.position;
        // if( _targetT != null )
        // _body.velocity = ( _targetT.position - pos ).normalized * speed;

        // if( Vector3.Distance( pos, _currentTargetTile ) < speed * 0.01f && _path.Count > 0 )
        // {
        //     _currentTargetTile = CoordsToWorldPosition( _path.Dequeue() );
        // }

        if( AnimationMovement.WhichTileAmIOn( pos ) == AnimationMovement.WhichTileAmIOn( _currentTargetTile ) && _path.Count > 0 )
        {
            _currentTargetTile = CoordsToWorldPosition( _path.Dequeue() );
        }

        // _body.velocity = ( _currentTargetTile - pos ).normalized * speed;

        var lookAt = Quaternion.LookRotation( _currentTargetTile - transform.position );
        transform.rotation = Quaternion.Slerp( transform.rotation, lookAt, Time.deltaTime );
        // transform.LookAt( _currentTargetTile );
        _body.velocity = transform.forward * speed;

        Debug.Log( "I am on " + transform.position + ", target on " + _currentTargetTile );
        
        // transform.LookAt( _currentTargetTile );
        // _body.velocity = Vector3.left * speed;
    }

    static Vector3 CoordsToWorldPosition( Vector2 coords ) => 
        new Vector3( 2 * coords.x, 0, 2 * coords.y );

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.P ) )
        {
            RunPathfinding();
        }
    }

    void FindNewTarget()
    {
        var pos = transform.position;

        // Set closest player to targetPos
        var transforms = playersParent.Cast<Transform>()
            .Where( p => p.gameObject.activeInHierarchy ).ToArray();
        var closestT = transforms[ 0 ];
        foreach( var t in transforms )
        {
            if( DistXZSquared( pos, t.position ) < DistXZSquared( pos, closestT.position ) )
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

        List<AStarNode> pathSegment = new List<AStarNode>();
        AStarNode node = WorldGenDemo.PathFind( startXY, targetXY );
        // pathSegment.Add( node );
        
        while( node.Parent != null )
        {
            node = node.Parent;
            pathSegment.Add( node );
        }

        JBB.LogQueue( _path );
        
        pathSegment.Reverse();
        _path.Clear();
        foreach( var n in pathSegment )
        {
            _path.Enqueue( new Vector2Int( n.X, n.Y ) );
        }
        _currentTargetTile = CoordsToWorldPosition( _path.Dequeue() );
        
        JBB.LogQueue( _path );
    }

    float DistXZSquared( Vector3 p1, Vector3 p2 )
    {
        var dX = p2.x - p1.x;
        var dZ = p2.z - p1.z;
        return dX * dX + dZ * dZ;
    }

    public void TakeHit()
    {
        hpBar.rectTransform.sizeDelta =
            new Vector2( hpBar.rectTransform.rect.width - 0.3f, hpBar.rectTransform.rect.height );
        if( hpBar.rectTransform.rect.width <= 0 )
        {
            Destroy( gameObject );
        }
    }
}