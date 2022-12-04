using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

    public static int[] To( this int min, int max )
    {
        var arr = new int[ max - min + 1 ];
        for( var i = 0; i < arr.Length; i++ )
            arr[ i ] = min + i;
        return arr;
    }

    public static Quaternion Random90Rot() => Quaternion.Euler( new Vector3( 0, 90 * Random.Range( 0, 4 ), 0 ) );
    public static Quaternion RandomYRot() => Quaternion.Euler( new Vector3( 0, Random.value * 360f, 0 ) );

    public static float DistXZSquared( Vector3 p1, Vector3 p2 )
    {
        var dX = p2.x - p1.x;
        var dZ = p2.z - p1.z;
        return dX * dX + dZ * dZ;
    }

    public static void Shuffle<T>( this IList<T> list )
    {
        var n = list.Count;
        while( n > 1 )
        {
            n--;
            var k = Random.Range( 0, n + 1 );
            ( list[ k ], list[ n ] ) = ( list[ n ], list[ k ] );
        }
    }

    public static bool WithinArrayBounds<T>( this T[ , ] arr2d, int x, int y ) =>
        x >= 0 && x < arr2d.GetLength( 0 ) && y >= 0 && y < arr2d.GetLength( 1 );

    public static T RandomEntry<T>( this T[] arr ) => arr[ Random.Range( 0, arr.Length ) ];
    public static T RandomEntry<T>( this List<T> list ) => list[ Random.Range( 0, list.Count ) ];

    public static bool Empty<T>( this T[] arr ) => arr.Length == 0;
    public static bool Empty<T>( this List<T> list ) => list.Count == 0;
    public static bool Empty<T>( this Queue<T> queue ) => queue.Count == 0;
}