using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [ SerializeField ] TextMeshProUGUI text;
    [ SerializeField ] float floatSpeed;
    [ SerializeField ] float floatDuration;
    [ SerializeField ] float friendlyTextSize = 8;
    [ SerializeField ] float enemyTextSize = 12;

    public void Play( Vector3 pos, int dmg, bool friendly )
    {
        gameObject.SetActive( true );

        transform.position = pos;
        text.text = "" + dmg;
        text.fontSize = friendly ? friendlyTextSize : enemyTextSize;

        StartCoroutine( FloatAndFade( friendly ) );
    }

    IEnumerator FloatAndFade( bool friendly )
    {
        var elapsed = 0f;
        var wait = new WaitForEndOfFrame();
        while( elapsed < floatDuration )
        {
            transform.position += Vector3.up * ( Time.deltaTime * floatSpeed * ( floatDuration - elapsed ) );
            
            text.color = friendly ? new Color( 1f, 1f, 1f, 1 - elapsed / floatDuration ) : new Color( 1f, 0f, 0f, 1 - elapsed / floatDuration );
            
            elapsed += Time.deltaTime;
            yield return wait;
        }

        gameObject.SetActive( false );
    }
}