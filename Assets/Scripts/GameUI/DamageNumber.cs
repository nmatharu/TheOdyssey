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
    [ SerializeField ] float costTextSize = 12;

    public void Play( Vector3 pos, string s, Color color, float textSize )
    {
        gameObject.SetActive( true );
        transform.position = pos;
        text.text = s;
        text.fontSize = textSize;
        StartCoroutine( FloatAndFade( color ) );
    }
    
    public void Play( Vector3 pos, int dmg, bool friendly )
    {
        gameObject.SetActive( true );

        transform.position = pos;
        text.text = "" + dmg;
        text.fontSize = friendly ? friendlyTextSize : enemyTextSize;

        StartCoroutine( FloatAndFade( friendly ? Color.white : Color.red ) );
    }

    public void SpendMoney( Vector3 pos, int cost )
    {
        gameObject.SetActive( true );

        transform.position = pos;
        text.text = "-" + cost;
        text.fontSize = costTextSize;

        StartCoroutine( FloatAndFade( Color.green ) );
    }

    IEnumerator FloatAndFade( Color color )
    {
        var elapsed = 0f;
        var wait = new WaitForEndOfFrame();
        while( elapsed < floatDuration )
        {
            transform.position += Vector3.up * ( Time.deltaTime * floatSpeed * ( floatDuration - elapsed ) );
            text.color = new Color( color.r, color.g, color.b, 1 - elapsed / floatDuration );
            
            elapsed += Time.deltaTime;
            yield return wait;
        }

        gameObject.SetActive( false );
    }
}