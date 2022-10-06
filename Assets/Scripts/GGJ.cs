using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

public static class GGJ
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
}