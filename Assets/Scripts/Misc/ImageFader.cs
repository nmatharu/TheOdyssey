using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFader : MonoBehaviour
{
    [ SerializeField ] float fadeTimeFrames = 6f;
    [ SerializeField ] bool showByDefault;

    static readonly Color Transparent = new( 0, 0, 0, 0 );
    
    Image _image;
    Color _color;
    WaitForFixedUpdate _wait = new();
    
    void Start()
    {
        _image = GetComponent<Image>();
        _color = _image.color;
        if( !showByDefault )
        {
            _image.color = Transparent;
        }
    }

    public void Fade( bool fadeIn )
    {
        if( fadeIn )
            FadeIn();
        else
            FadeOut();
    }

    public void Show() => _image.color = _color;
    
    public void FadeIn() => StartCoroutine( FadeInCoroutine() );

    public void FadeOut() => StartCoroutine( FadeOutCoroutine() );

    IEnumerator FadeInCoroutine()
    {
        for( var opacity = 0f; opacity < _color.a; opacity += _color.a / fadeTimeFrames )
        {
            _image.color = new Color( _color.r, _color.g, _color.b, opacity );
            yield return _wait;
        }
        _image.color = _color;
    }

    IEnumerator FadeOutCoroutine()
    {
        for( var opacity = _color.a; opacity > 0f; opacity -= _color.a / fadeTimeFrames )
        {
            _image.color = new Color( _color.r, _color.g, _color.b, opacity );
            yield return _wait;
        }
        _image.color = Transparent;
    }
}
