using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PostGameScreen : MonoBehaviour
{
    CanvasGroup _canvas;
    public float timeToEnableInput = 5f;
    [ SerializeField ] CanvasGroup groupToHide;
    [ SerializeField ] GameObject[] difficultyLabels;
    [ SerializeField ] CanvasGroup returnToLobbyGroup;
    [ SerializeField ] TextMeshProUGUI resultText;
    [ SerializeField ] TextMeshProUGUI clockText;

    [ SerializeField ] PostGameStatsGroup[] statsGroups;
    
    void Start()
    {
        _canvas = GetComponent<CanvasGroup>();
        _canvas.alpha = 0;
    }

    public void Init( bool defeat, int difficulty, string gameClock, Player[] players )
    {
        resultText.text = defeat ? "DEFEAT" : "VICTORY!";
        difficultyLabels[ difficulty ].SetActive( true );
        clockText.text = gameClock;

        for( var i = 0; i < players.Length; i++ )
            statsGroups[ i ].Init( players[ i ].PlayerName(), players[ i ].Statistics() );

        StartCoroutine( ShowPostGame() );
    }

    IEnumerator ShowPostGame()
    {
        const float fadeOutTime = 0.5f;
        const float waitTime = 1f;
        const float fadeInTime = 0.1f;

        var elapsed = 0f;
        while( elapsed < fadeOutTime )
        {
            groupToHide.alpha = ( fadeOutTime - elapsed ) / fadeOutTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds( waitTime );
        
        elapsed = 0f;
        while( elapsed < fadeInTime )
        {
            _canvas.alpha = elapsed / fadeInTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        _canvas.alpha = 1f;

        var timeToWait = timeToEnableInput - fadeOutTime - waitTime - fadeInTime;
        yield return new WaitForSeconds( timeToWait );
        
        elapsed = 0f;
        while( elapsed < fadeInTime )
        {
            returnToLobbyGroup.alpha = elapsed / fadeInTime;
            elapsed += Time.deltaTime;
            yield return null;
        }

        returnToLobbyGroup.alpha = 1f;
    }
}
