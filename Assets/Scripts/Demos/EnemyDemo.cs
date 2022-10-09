using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDemo : MonoBehaviour
{
    [ SerializeField ] Transform playersParent;
    [ SerializeField ] float speed = 3f;
    [ SerializeField ] Image hpBar;

    Queue<Vector2> _path;
    Vector3 _currentTargetTile;

    Rigidbody _body;
    Transform _targetT;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        // InvokeRepeating( nameof( FindNewTarget ), 0, 0.25f );

        _path = new Queue<Vector2>();
        _path.Enqueue( Vector2.one );
        _path.Enqueue( new Vector2( 2, 4 ) );
        _currentTargetTile = CoordsToWorldPosition( _path.Dequeue() );
    }

    void FixedUpdate()
    {
        var pos = transform.position;
        // if( _targetT != null )
        // _body.velocity = ( _targetT.position - pos ).normalized * speed;

        if( Vector3.Distance( pos, _currentTargetTile ) < speed * 0.01f )
        {
            _currentTargetTile = CoordsToWorldPosition( _path.Dequeue() );
        }

        _body.velocity = ( _currentTargetTile - pos ).normalized * speed;
        
        transform.LookAt( pos + _body.velocity );
    }

    static Vector3 CoordsToWorldPosition( Vector2 coords ) => 
        new Vector3( 2 * coords.x, 0, 2 * coords.y );

    void Update()
    {
        // var pos = transform.position;
        // transform.position = new Vector3( pos.x, 0.5f + 0.5f * Mathf.Sin( Time.time * 2f ), pos.z );
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