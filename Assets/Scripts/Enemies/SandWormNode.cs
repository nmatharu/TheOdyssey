using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SandWormNode : MonoBehaviour
{
    [ SerializeField ] public Transform backPoint;
    [ SerializeField ] Transform nodeMesh;
    [ SerializeField ] public MeshRenderer meshRenderer;

    Enemy _enemy;
    Transform _target;
    SandWorm _worm;

    public bool childNode;

    Vector3 _v3Target;
    
    void Update()
    {
        var targetPos =  childNode ? _target.position : _v3Target;
        var pos = transform.position;

        var look = targetPos - pos;
        var lookRot = look != Vector3.zero ? Quaternion.LookRotation( targetPos - pos ) : Quaternion.identity;
        var lookTarget = Quaternion.Slerp( transform.rotation, lookRot, 
            ( childNode ? _worm.lookSlerp : _worm.headLookSlerp ) * Time.deltaTime );
        transform.rotation = lookTarget;

        if( childNode )
            transform.position = Vector3.MoveTowards( pos, targetPos, _worm.speed * Time.deltaTime );
        else
            transform.position += transform.forward * _worm.speed * Time.deltaTime;
        // _body.velocity = Vector3.ClampMagnitude( targetPos - pos, 1f ) * _worm.speed;
    }

    public void Init( Enemy enemy, SandWorm worm, Transform t )
    {
        _enemy = enemy;
        _worm = worm;
        _target = t;
    }

    public void SetV3Target( Vector3 v ) => _v3Target = v;

    public void InitAutoRot( int i, int wormSize )
    {
        var nodePct = (float) ( i - 1 ) / ( wormSize - 1 ); 
        nodeMesh.Rotate( 0, 0, nodePct * 360 );
        childNode = true;
    }

    public Enemy Enemy() => _enemy;
}
