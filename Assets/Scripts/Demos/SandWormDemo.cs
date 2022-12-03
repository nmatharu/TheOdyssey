using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandWormDemo : MonoBehaviour
{
    [ SerializeField ] Transform follow;
    [ SerializeField ] Transform[] nodes;

    Rigidbody _body;

    void Start() => _body = GetComponent<Rigidbody>();

    void Update()
    {
        nodes[ 0 ].LookAt( follow );
        nodes[ 0 ].position += nodes[ 0 ].forward * ( Time.deltaTime * 12f );

        for( var i = 1; i < nodes.Length; i++ )
        {
            var target = nodes[ i - 1 ].position - nodes[ i - 1 ].forward * 4f;
            var lookRot = Quaternion.Slerp( nodes[ i ].rotation, Quaternion.LookRotation( target ),
                1000f * Time.deltaTime );
            nodes[ i ].rotation = lookRot;
            nodes[ i ].position = Vector3.Lerp( nodes[ i ].position, target, 10f * Time.deltaTime );
            // nodes[ i ].position += nodes[ i ].forward * ( Time.deltaTime * 5f );
        }
    }
}
