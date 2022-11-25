using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : Interactable
{
    [ SerializeField ] Transform chestHinge;
    [ SerializeField ] ParticleSystem openPfx;
    [ SerializeField ] ParticleSystem twinklePfx;
    
    bool _opened;
    bool _taken;
    
    void Start()
    {
    }

    void OpenChest()
    {
        _opened = true;
        openPfx.Play();
        twinklePfx.Stop();
        interactPrompt = "TAKE";

        StartCoroutine( SwingOpenHinge() );
        IEnumerator SwingOpenHinge()
        {
            var x = 270f;
            for( var elapsed = 0f; elapsed < 1.5f; elapsed += Time.deltaTime )
            {
                x = Mathf.Lerp( x, 180, 15f * Time.deltaTime );
                chestHinge.localEulerAngles = new Vector3( x, 0, 0 );
                yield return null;
            }

            chestHinge.localEulerAngles = new Vector3( 180, 0, 0 );
        }
    }

    void TakeRune()
    {
        _taken = true;
        cannotInteractPrompt = "EMPTY";
    }

    public override void Interact( Player player )
    {
        if( !_opened )
        {
            OpenChest();
            return;
        }

        if( !_taken )
        {
            TakeRune();
        }
    }

    public override bool InteractionLocked( Player player ) => _taken;
}
