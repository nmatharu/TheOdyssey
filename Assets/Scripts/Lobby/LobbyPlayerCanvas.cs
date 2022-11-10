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

    [ TextArea( 1, 2 ) ]
    [ SerializeField ] string validNameCharacters;
    [ SerializeField ] Vector3 typeCharStartPos;
    [ SerializeField ] float typeCharDistDelta;
    [ SerializeField ] int typeCharRowLength = 13;
    [ SerializeField ] GameObject typeCharPrefab;
    [ SerializeField ] Transform typeCharParent;
    
    void Start()
    {
        promptText.SetActive( true );
        descCanvas.SetActive( false );
    }

    public void Init( GlobalPlayerInput input, string pName, string deviceName )
    {
        promptText.SetActive( false );
        descCanvas.SetActive( true );
        controllerName.text = deviceName.ToUpper();
        playerName.text = pName;

        PopulateCharacters();
    }

    void PopulateCharacters()
    {
        for( var i = 0; i < validNameCharacters.Length; i++ )
        {
            var x = i % typeCharRowLength;
            var y = i / typeCharRowLength;
            var o = Instantiate( typeCharPrefab, 
                typeCharStartPos + typeCharDistDelta * new Vector3( x, -y, 0f ), 
                Quaternion.identity );
            o.GetComponent<RectTransform>().SetParent( typeCharParent, false );
            o.GetComponent<LobbyTypeChar>().Init( validNameCharacters[ i ] );
        }

        for( var x = typeCharRowLength - 3; x < typeCharRowLength; x++ )
        {
            var o = Instantiate( typeCharPrefab,
                typeCharStartPos + typeCharDistDelta * new Vector3( x, -3f, 0f ),
                Quaternion.identity );
            o.GetComponent<RectTransform>().SetParent( typeCharParent, false );
            o.GetComponent<LobbyTypeChar>().InitSpecial( x - typeCharRowLength + 3 );
        }
    }

    public void Reset()
    {
        promptText.SetActive( true );
        descCanvas.SetActive( false );
    }
}
