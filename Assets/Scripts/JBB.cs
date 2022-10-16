using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class JBB
{
    public static void Invoke( this MonoBehaviour mb, Action f, float delay ) =>
        mb.StartCoroutine( InvokeRoutine( f, delay ) );

    private static IEnumerator InvokeRoutine( Action f, float delay )
    {
        yield return new WaitForSeconds( delay );
        f();
    }

    public static void LogArray<T>( T[] arr )
    {
        var outStr = "[ ";
        for( var i = 0; i < arr.Length; i++ )
        {
            var e = arr[ i ];
            outStr += e + ( i < arr.Length - 1 ? ", " : "" );
        }

        Debug.Log( outStr + " ]" );
    }

    public static void LogQueue<T>( Queue<T> q )
    {
        var outStr = "[ ";
        foreach( var item in q )
        {
            outStr += item + " ";
        }
        Debug.Log( outStr + "]" );
    }

    public static void LogCommaSeparated( params object[] o )
    {
        var outStr = "";
        for( var i = 0; i < o.Length; i++ )
        {
            var e = o[ i ];
            outStr += e + ( i < o.Length - 1 ? ", " : "" );
        }

        Debug.Log( outStr );
    }

    public static float Map( float value, float inputMin, float inputMax, float outputMin, float outputMax ) =>
        outputMin + ( outputMax - outputMin ) * ( ( value - inputMin ) / ( inputMax - inputMin ) );

    public static float ClampedMap( float value, float inputMin, float inputMax, float outputMin, float outputMax ) =>
        Mathf.Clamp( Map( value, inputMin, inputMax, outputMin, outputMax ), outputMin, outputMax );

    public static float ClampedMap01( float value, float inputMin, float inputMax ) =>
        Mathf.Clamp01( Map( value, inputMin, inputMax, 0, 1 ) );

    // https://forum.unity.com/threads/suggest-add-vector3int-manhattandistance-vector3int-a-vector3int-b.444672/
    public static int ManhattanDistance( this Vector2Int a, Vector2Int b )
    {
        checked
        {
            return Mathf.Abs( a.x - b.x ) + Mathf.Abs( a.y - b.y );
        }
    }
    
    public static float DistXZSquared( Vector3 p1, Vector3 p2 )
    {
        var dX = p2.x - p1.x;
        var dZ = p2.z - p1.z;
        return dX * dX + dZ * dZ;
    }
}