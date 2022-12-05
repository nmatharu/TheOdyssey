using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AutoImageFader : MonoBehaviour
{
    [ SerializeField ] float fadeInTimeFrames = 6f;
    [ SerializeField ] float fadeOutTimeFrames = 2f;
    [ SerializeField ] float fadeOutAfter = 19f;
    [ SerializeField ] float destroyAfter = 12f;
    [ SerializeField ] bool targetAlpha100;

    [ SerializeField ] bool autoFadeOut = true;
    [ SerializeField ] bool destroyAfterFadeOut = true;
    
    static readonly Color Transparent = new( 0, 0, 0, 0 );

    Image _image;
    Color _color;
    WaitForFixedUpdate _wait = new();
    
    void Start()
    {
        _image = GetComponent<Image>();
        _color = _image.color;
        _color.a = targetAlpha100 ? 1 : _color.a;
        FadeIn();
        
        if( !autoFadeOut ) return;
        Invoke( nameof( FadeOut ), fadeOutAfter / 50f );
        Destroy( transform.root.gameObject, destroyAfter / 50f );
    }

    public void FadeIn() => StartCoroutine( FadeInCoroutine() );

    public void FadeOut() => StartCoroutine( FadeOutCoroutine() );

    IEnumerator FadeInCoroutine()
    {
        for( var opacity = 0f; opacity < _color.a; opacity += _color.a / fadeInTimeFrames )
        {
            _image.color = new Color( _color.r, _color.g, _color.b, opacity );
            yield return _wait;
        }
        _image.color = _color;
    }

    IEnumerator FadeOutCoroutine()
    {
        for( var opacity = _color.a; opacity > 0f; opacity -= _color.a / fadeOutTimeFrames )
        {
            _image.color = new Color( _color.r, _color.g, _color.b, opacity );
            yield return _wait;
        }
        _image.color = Transparent;
        Destroy( transform.root.gameObject, 1f );
    }
}
