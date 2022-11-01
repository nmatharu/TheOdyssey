using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoDrawTest : MonoBehaviour
{
    [ SerializeField ] Vector3 position;
    [ SerializeField ] Vector3 bounds;
    void OnDrawGizmos()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube( transform.InverseTransformPoint( transform.position ) + position, bounds );
    }
}
