using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyLevelGraphic : MonoBehaviour
{
    [ SerializeField ] TextMeshProUGUI levelText;
    [ SerializeField ] TextMeshProUGUI totalTimeElapsedText;
    [ SerializeField ] Image timerCircle;

    int _lastSeconds = 0;

    public void UpdateGraphic( int level, float elapsedPct, float totalTimeElapsed )
    {
        levelText.text = level.ToString();
        timerCircle.transform.rotation = Quaternion.Euler( 0, 0, elapsedPct * -360f );

        var seconds = (int) totalTimeElapsed;
        if( _lastSeconds == seconds )   return;
        totalTimeElapsedText.text = ToClockFormat( seconds );
    }

    string ToClockFormat( int seconds )
    {
        _lastSeconds = seconds;
        var minutes = seconds / 60;
        seconds %= 60;
        return minutes + ":" + seconds.ToString( "D2" );
    }
}