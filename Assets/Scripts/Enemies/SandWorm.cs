using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandWorm : MonoBehaviour
{
    [ SerializeField ] SandWormNode[] nodes;
    [ SerializeField ] Transform toFollow;
    [ SerializeField ] public float speed = 5f;
    [ SerializeField ] public float lookSlerp = 10f;
    [ SerializeField ] Transform wormParent;

    public SandWormNode head;
    Enemy _enemy;
    
    void Start()
    {
        _enemy = GetComponentInParent<Enemy>();
        
        head = nodes[ 0 ];
        head.Init( toFollow );
        
        for( var i = 1; i < nodes.Length; i++ )
            nodes[ i ].Init( nodes[ i - 1 ].backPoint, i, nodes.Length );
    }
}
