using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IntermissionText : MonoBehaviour
{
    [ SerializeField ] float startDelaySeconds = 1f;
    [ SerializeField ] float endDelaySeconds = 2f;
    [ SerializeField ] float periodDelay = 0.5f;
    [ SerializeField ] float delayBetweenCharacters = 0.25f;
    [ SerializeField ] TextMeshProUGUI textDisplay;
    
    ImageFader _fader;
    void Start() => _fader = GetComponent<ImageFader>();

    public void StartText( string text ) => StartCoroutine( DisplayText( text ) );

    IEnumerator DisplayText( string text )
    {
        AudioManager.Instance.transition.PlaySfx( 0.5f );
        _fader.FadeIn();

        yield return new WaitForSeconds( startDelaySeconds );

        var wait = new WaitForSeconds( delayBetweenCharacters );
        var periodWait = new WaitForSeconds( periodDelay );
        
        textDisplay.color = Color.black;
        textDisplay.text = "";
        var color = textDisplay.color;
        foreach( var c in text )
        {
            textDisplay.text += c;
            
            if( c is not (' ' or '\n') )
                AudioManager.Instance.intermissionType.RandomEntry().PlaySfx( 1f, 0.1f );
            
            if( c == '.' )
                yield return periodWait;
            yield return wait;
        }

        yield return new WaitForSeconds( endDelaySeconds );
        AudioManager.Instance.transition.PlaySfx( 0.5f );
        
        var elapsed = 0f;
        const float fadeOutTime = 0.5f;
        while( elapsed < fadeOutTime )
        {
            var alpha = 1 - JBB.ClampedMap01( elapsed, 0, fadeOutTime );
            textDisplay.color = new Color( color.r, color.g, color.b, alpha );
            yield return null;
            elapsed += Time.deltaTime;
        }

        textDisplay.color = Color.clear;
        
        GameManager.Instance.BeginNextStage();
        yield return new WaitForSeconds( 0.25f );
        _fader.FadeOut();

    }
}
