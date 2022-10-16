using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PurgeOffscreen : MonoBehaviour
{
    [ SerializeField ] float startDelay = 2.5f;
    [ SerializeField ] float purgeEvery = 1f;
    
    void Start() => InvokeRepeating( nameof( PurgeOffscreenProjectiles ), startDelay, purgeEvery );

    void PurgeOffscreenProjectiles()
    {
        foreach( Transform t in transform )
        {
            if( WorldGenerator.Instance.OffMap( t.position ) )
                Destroy( t.gameObject );
        }
    }
}
