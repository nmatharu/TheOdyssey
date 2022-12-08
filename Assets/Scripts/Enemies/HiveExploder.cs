using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveExploder : MonoBehaviour
{
    [ SerializeField ] ParticleSystem pfx;
    [ SerializeField ] float radius;
    [ SerializeField ] ImageFader image;

    public void ShowIndicator() => image.FadeIn();
    
    public void Explode( float dmg, int lvl )
    {
        pfx.Play();
        image.FadeOut();
        var colliders = Physics.OverlapSphere( transform.position, radius );
        AudioManager.Instance.hiveExploderBurst.PlaySfx( 1f, 0.2f );
        foreach( var c in colliders )
        {
            var p = c.GetComponent<Player>();
            if( p != null )
                p.IncomingDamage( dmg, lvl );
        }
    }
}
