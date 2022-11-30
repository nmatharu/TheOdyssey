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
    [ SerializeField ] TextMeshProUGUI clockText;

    void Start()
    {
        _canvas = GetComponent<CanvasGroup>();
        _canvas.alpha = 0;
    }

    public void Init( int difficulty, string gameClock )
    {
        difficultyLabels[ difficulty ].SetActive( true );
        clockText.text = gameClock;
        StartCoroutine( ShowPostGame() );
    }

    IEnumerator ShowPostGame()
    {
        const float fadeOutTime = 0.5f;
        const float waitTime = 1f;
        const float fadeInTime = 0.25f;

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
