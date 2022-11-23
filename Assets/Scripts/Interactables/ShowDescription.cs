using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowDescription : MonoBehaviour
{
    [ SerializeField ] CanvasGroup canvasGroup;
    [ SerializeField ] float furthestShowDistance = 4f;
    [ SerializeField ] float closestShowDistance = 2.5f;

    void Start() => canvasGroup.alpha = 0;

    void OnTriggerStay( Collider o )
    {
        var p = o.GetComponent<Player>();
        if( p != null )
        {
            var d = JBB.DistXZSquared( transform.position, p.transform.position );
            canvasGroup.alpha = JBB.ClampedMap01( d, furthestShowDistance, closestShowDistance );
        }
    }

    void OnTriggerExit( Collider o )
    {
        var p = o.GetComponent<Player>();
        if( p != null )
            canvasGroup.alpha = 0;
    }
}
