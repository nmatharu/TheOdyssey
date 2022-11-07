using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LobbyPlayerCanvas : MonoBehaviour
{
    [ SerializeField ] GameObject promptText;
    [ SerializeField ] GameObject descCanvas;
    [ SerializeField ] TextMeshProUGUI playerName;
    [ SerializeField ] TextMeshProUGUI controllerName;
    
    void Start()
    {
        promptText.SetActive( true );
        descCanvas.SetActive( false );
    }

    public void Init( PlayerInputBroadcast input, string pName, string deviceName )
    {
        promptText.SetActive( false );
        descCanvas.SetActive( true );
        controllerName.text = deviceName.ToUpper();
        playerName.text = pName;
    }
}
