using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FloatingIcon : MonoBehaviour
{
    [ SerializeField ] Image image;

    Color _color;
    float _size;
    float _speed;
    float _duration;

    void Start() => StartCoroutine( FloatAndFade( _color ) );

    public void Init( Vector3 pos, Sprite s, Color color, float size, float speed, float duration )
    {
        gameObject.SetActive( true );
        transform.position = pos;
        image.sprite = s;
        _color = color;
        _size = size;
        _speed = speed;
        _duration = duration;
    }

    IEnumerator FloatAndFade( Color color )
    {
        var elapsed = 0f;
        var wait = new WaitForEndOfFrame();
        image.rectTransform.sizeDelta = new Vector2( _size, _size );
        while( elapsed < _duration )
        {
            transform.position += Vector3.up * ( Time.deltaTime * _speed * ( _duration - elapsed ) );
            image.color = new Color( color.r, color.g, color.b, 1 - elapsed / _duration );

            elapsed += Time.deltaTime;
            yield return wait;
        }

        Destroy( gameObject );
    }
}