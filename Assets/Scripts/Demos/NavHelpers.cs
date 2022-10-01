using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NavHelpers
{
    public static Transform FindClosestUnit( Transform transform, List<Transform> ts )
    {
        if( ts.Count == 0 ) return null;

        var closest = ts[ 0 ];
        var smallestSquare = cSqr3DTopDown( transform, closest );

        foreach( var t in ts )
        {
            var sqr = cSqr3DTopDown( transform, t );
            if( !( sqr < smallestSquare ) ) continue;
            closest = t;
            smallestSquare = sqr;
        }
        return closest;
    }

    static float cSqr3DTopDown( Transform a, Transform b ) =>
        ( a.position.x - b.position.x ) * ( a.position.x - b.position.x ) +
        ( a.position.z - b.position.z ) * ( a.position.z - b.position.z );
    
    public static void Invoke(this MonoBehaviour mb, Action f, float delay)
    {
        mb.StartCoroutine(InvokeRoutine(f, delay));
    }
 
    private static IEnumerator InvokeRoutine(System.Action f, float delay)
    {
        yield return new WaitForSeconds(delay);
        f();
    }
}