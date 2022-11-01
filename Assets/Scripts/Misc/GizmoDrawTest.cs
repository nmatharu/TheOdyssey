using UnityEngine;

public class GizmoDrawTest : MonoBehaviour
{
    [ SerializeField ] Vector3 bounds;
    [ SerializeField ] float centerDist;

    void OverlapBox()
    {
        var colliders = Physics.OverlapBox( transform.position + transform.forward * centerDist,
            bounds / 2, transform.rotation );
    }

    void OnDrawGizmos()
    {
        var prevMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        var boxPosition = transform.position + transform.forward * centerDist;

        // convert from world position to local position 
        boxPosition = transform.InverseTransformPoint( boxPosition );

        Gizmos.DrawWireCube( boxPosition, bounds );

        // restore previous Gizmos settings
        Gizmos.matrix = prevMatrix;
    }
}