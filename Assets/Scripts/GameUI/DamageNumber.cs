using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [ SerializeField ] TextMeshProUGUI text;
    [ SerializeField ] float floatSpeed;
    [ SerializeField ] float floatDuration;
    
    public void Play( Vector3 pos, int dmg, bool friendly )
    {
        gameObject.SetActive( true );
        
        transform.position = pos;
        text.text = "" + dmg;
        text.color = friendly ? Color.gray : Color.red;

        StartCoroutine( FloatAndFade() );
    }

    IEnumerator FloatAndFade()
    {
        var elapsed = 0f;
        var wait = new WaitForEndOfFrame();
        while( elapsed < floatDuration )
        {
            transform.position += Vector3.up * ( Time.deltaTime * floatSpeed );
            text.color = new Color( 1f, 1f, 1f, 1 - elapsed / floatDuration );
            elapsed += Time.deltaTime;
            yield return wait;
        }
        
        gameObject.SetActive( false );
    }
}
