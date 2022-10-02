using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemyDemo : MonoBehaviour
{
    [ SerializeField ] Transform playersParent;
    [ SerializeField ] float speed = 3f;
    [ SerializeField ] Image hpBar;
    
    Rigidbody _body;
    Transform _targetT;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        InvokeRepeating( nameof( FindNewTarget ), 0, 0.25f );
    }

    void FixedUpdate()
    {
        var pos = transform.position;
        
        if( _targetT != null)
            _body.velocity = ( _targetT.position - pos ).normalized * speed;
        
        transform.LookAt( pos + _body.velocity );
    }

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
