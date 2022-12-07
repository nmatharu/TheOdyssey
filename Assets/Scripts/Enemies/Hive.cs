using System;
using System.Collections.Generic;
using UnityEngine;

public class Hive : MonoBehaviour
{
    [ SerializeField ] Transform spoutPos;
    [ SerializeField ] float lockOnSpeed = 1f;

    Transform _targetPlayer;
    Rigidbody _body;
    Enemy _enemy;

    void Start()
    {
        _body = GetComponent<Rigidbody>();
        _enemy = GetComponent<Enemy>();

        InvokeRepeating( nameof( FindNewTarget ), 0, 1f );
    }

    void FixedUpdate()
    {
        _body.velocity = Vector3.zero;
    }

    void Update()
    {
        var pos = transform.position;
        if( _targetPlayer != null )
        {
            var look = _targetPlayer.position - pos;
            var lookRot = look != Vector3.zero ? Quaternion.LookRotation( look ) : Quaternion.identity;
            var lookTarget = Quaternion.Slerp( transform.rotation, lookRot, lockOnSpeed * Time.deltaTime );
            transform.rotation = lookTarget;
        }
    }

    void FindNewTarget()
    {
        var pos = transform.position;

        // Set closest player to targetPos
        var players = GameManager.Instance.Players();
        if( players.Empty() ) return;
        var closestPlayer = players[ 0 ].transform;

        foreach( var p in players )
            if( JBB.DistXZSquared( pos, p.transform.position ) < JBB.DistXZSquared( pos, closestPlayer.position ) )
                closestPlayer = p.transform;

        _targetPlayer = closestPlayer;
    }
}