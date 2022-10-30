using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordPfx : MonoBehaviour
{
    [ SerializeField ] ParticleSystem pfx;
    [ SerializeField ] PfxDefinition basicDef;
    [ SerializeField ] PfxDefinition slashDef;
    const float AnimFrameTime = 1 / 60f;

    public void Light() => PlayPfx( basicDef );
    public void Slash() => PlayPfx( slashDef );

    void PlayPfx( PfxDefinition definition )
    {
        this.Invoke( () => pfx.Play(), definition.start * AnimFrameTime );
        this.Invoke( () => pfx.Stop(), definition.stop * AnimFrameTime );
        var sh = pfx.shape;
        sh.position = definition.position;
        sh.radius = definition.radius;
    }

    [ Serializable ]
    class PfxDefinition
    {
        [ SerializeField ] public int start;
        [ SerializeField ] public int stop;
        [ SerializeField ] public Vector3 position;
        [ SerializeField ] public int radius;
    }
    
}
