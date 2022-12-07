using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldChest : Interactable
{
    [ SerializeField ] Transform chestHinge;
    [ SerializeField ] ParticleSystem openPfx;
    [ SerializeField ] ParticleSystem twinklePfx;
    
    bool _opened;

    void OpenChest()
    {
        _opened = true;
        _disabled = true;
        openPfx.Play();
        twinklePfx.Stop();
        interactPrompt = "";
        cannotInteractPrompt = "EMPTY";
        
        AudioManager.Instance.chestOpen.PlaySfx( 1f, 0.1f );

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

    public override void Interact( Player player )
    {
        if( _opened ) return;
        
        var gold = GameManager.Instance.ChestGoldAmount();
        player.AwardCurrency( gold, false );
        GameManager.Instance.SpawnGenericFloating( player.transform.position, $"+{gold}", Color.yellow, 24f );

        var emissionPct = GameManager.Instance.PctOfExpectedChestGold( gold );
        // Debug.Log( emissionPct );

        var particleCount = openPfx.emission.GetBurst( 0 ).count.constant;
        var main = openPfx.main;
        var lifetime = main.startLifetime.constant;
            
        openPfx.emission.SetBurst( 0, new ParticleSystem.Burst( 0, 
            new ParticleSystem.MinMaxCurve( particleCount * emissionPct * emissionPct ) ) );
        main.startLifetime = new ParticleSystem.MinMaxCurve( lifetime * Mathf.Sqrt( emissionPct ) );
            
        OpenChest();
    }

    public override bool InteractionLocked( Player player ) => _opened;
}
