using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PostGameStatsGroup : MonoBehaviour
{
    [ SerializeField ] TextMeshProUGUI playerName;
    [ SerializeField ] TextMeshProUGUI kills;
    [ SerializeField ] TextMeshProUGUI deaths;
    [ SerializeField ] TextMeshProUGUI distanceTravelled;
    [ SerializeField ] TextMeshProUGUI damageDealt;
    [ SerializeField ] TextMeshProUGUI biggestHit;
    [ SerializeField ] TextMeshProUGUI damageTaken;
    [ SerializeField ] TextMeshProUGUI goldCollected;
    [ SerializeField ] TextMeshProUGUI timeAlive;

    CanvasGroup _canvasGroup;

    void Start() => _canvasGroup = GetComponent<CanvasGroup>();

    public void Init( string n, InGameStatistics stats )
    {
        _canvasGroup.alpha = 1f;
        playerName.text = n;
        kills.text = stats._kills.ToString();
        deaths.text = stats._deaths.ToString();
        distanceTravelled.text = (int) stats._metresTravelled + "m";
        damageDealt.text = ( (int) stats._damageDealt ).ToString();
        biggestHit.text = ( (int) stats._mostDamageDealt ).ToString();
        damageTaken.text = ( (int) stats._damageTaken ).ToString();
        goldCollected.text = stats._goldCollected.ToString();
        timeAlive.text = TimeString( stats._timeAliveTimeSteps );
    }

    string TimeString( int aliveTimeSteps )
    {
        var seconds = aliveTimeSteps / 50;
        var minutes = seconds / 60;
        
        seconds = seconds % 60;

        return minutes + ":" + seconds.ToString( "00" );
    }
}
