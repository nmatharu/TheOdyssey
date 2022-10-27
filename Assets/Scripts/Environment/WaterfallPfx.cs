using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterfallPfx : MonoBehaviour
{
    [ SerializeField ] ParticleSystem waterPfx;
    [ SerializeField ] ParticleSystem upPfx;
    [ SerializeField ] ParticleSystem acrossPfx;

    [ SerializeField ] int upPfxBaseRate = 60;
    [ SerializeField ] int acrossPfxBaseRate = 30;
    [ SerializeField ] int waterPfxBaseRate = 80;
    
    void Start()
    {
        SetWidth( 3 );
    }

    public void SetWidth( int blocks )
    {
        var upSh = upPfx.shape;
        var acSh = acrossPfx.shape;
        var wtSh = waterPfx.shape;
        
        var upEm = upPfx.emission;
        var acEm = acrossPfx.emission;
        var wtEm = waterPfx.emission;
        
        upSh.radius = blocks;
        acSh.radius = blocks;
        wtSh.radius = blocks;
        
        upEm.rateOverTime = new ParticleSystem.MinMaxCurve( blocks * upPfxBaseRate );
        acEm.rateOverTime = new ParticleSystem.MinMaxCurve( blocks * acrossPfxBaseRate );
        wtEm.rateOverTime = new ParticleSystem.MinMaxCurve( blocks * waterPfxBaseRate );
    }
}
