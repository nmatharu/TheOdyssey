using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerCanvas : MonoBehaviour
{
    [ SerializeField ] Image backdrop;
    
    [ SerializeField ] GameObject promptText;
    [ SerializeField ] GameObject descCanvas;

    [ SerializeField ] GameObject descOverview;
    [ SerializeField ] GameObject descTypeName;
    [ SerializeField ] GameObject descReady;

    [ SerializeField ] Color defColor;
    [ SerializeField ] Color readyColor;
    
    [ SerializeField ] TextMeshProUGUI playerName;
    [ SerializeField ] TextMeshProUGUI controllerName;

    [ TextArea( 1, 2 ) ]
    [ SerializeField ] string validNameCharacters;
    [ SerializeField ] Vector3 typeCharStartPos;
    [ SerializeField ] float typeCharDistDelta;
    [ SerializeField ] int typeCharRowLength = 13;
    [ SerializeField ] GameObject typeCharPrefab;
    [ SerializeField ] Transform typeCharParent;

    LobbyTypeChar[ , ] _keyboard;

    [ SerializeField ] float wobbleAmount = 50f;
    
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
        _keyboard = new LobbyTypeChar[ typeCharRowLength, validNameCharacters.Length / typeCharRowLength + 1 ];
        
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
        descReady.SetActive( false );
    }

    public void ToNameEntry()
    {
        descOverview.SetActive( false );
        descTypeName.SetActive( true );
        descReady.SetActive( false );
    }

    public void MenuNav( Vector2Int nav )
    {
        Debug.Log( nav );
    }

    // Returns true if we successfully edited the name and can switch back to the other action map
    public bool FinishEditName()
    {
        descOverview.SetActive( true );
        descTypeName.SetActive( false );
        descReady.SetActive( false );
        return true;
    }

    public void SetReady( bool ready )
    {
        if( !ready )
        {
            FinishEditName();
            StopWobble();
            backdrop.color = defColor;
            return;
        }
        
        descReady.SetActive( true );
        descOverview.SetActive( false );
        descTypeName.SetActive( false );
        backdrop.color = readyColor;
    }

    void StopWobble() => transform.rotation = Quaternion.identity;

    public void Wobble( Vector2 readValue ) =>
        transform.rotation = Quaternion.Euler( readValue.y * wobbleAmount, 0, readValue.x * wobbleAmount );
}
