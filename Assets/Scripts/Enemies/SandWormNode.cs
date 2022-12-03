using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SandWormNode : MonoBehaviour
{
    [ SerializeField ] public Transform backPoint;
    [ SerializeField ] Transform nodeMesh;
    
    Transform _target;
    SandWorm _worm;
    
    void Start()
    {
        _worm = GetComponentInParent<SandWorm>();
    }

    void Update()
    {
        var targetPos = _target.position;
        var pos = transform.position;

        var look = targetPos - pos;
        var lookRot = look != Vector3.zero ? Quaternion.LookRotation( targetPos - pos ) : Quaternion.identity;
        var lookTarget = Quaternion.Slerp( transform.rotation, lookRot, _worm.lookSlerp * Time.deltaTime );
        transform.rotation = lookTarget;

        transform.position = Vector3.MoveTowards( pos, targetPos, _worm.speed * Time.deltaTime );
        // _body.velocity = Vector3.ClampMagnitude( targetPos - pos, 1f ) * _worm.speed;
    }

    public void Init( Transform t, int i, int wormSize )
    {
        var nodePct = (float) ( i - 1 ) / ( wormSize - 1 ); 
        nodeMesh.Rotate( 0, 0, nodePct * 360 );
        
        _target = t;
    }

    public void Init( Transform t ) => _target = t;
}
