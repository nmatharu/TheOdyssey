using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [ SerializeField ] GameObject[] topLevelMenuOptions;
    public static MenuManager Instance { get; private set; }
    int _topLevelMenuIndex = 0;

    [ SerializeField ] GameObject settingsCanvas;
    [ SerializeField ] GameObject creditsCanvas;

    MenuState _state;
    enum MenuState
    {
        Top,
        Settings,
        Credits
    }

    bool _loadingLobby;

    void Start()
    {
        if( Instance != null && Instance != this )
            Destroy( this );
        else
            Instance = this;
        
        UpdateMenuOptions();
        _state = MenuState.Top;

        Application.targetFrameRate = 60;
    }

    void ConfirmTopLevelSelection()
    {
        if( _loadingLobby ) return;
            
        AudioManager.Instance.uiBig.PlaySfx();
        
        switch( _topLevelMenuIndex )
        {
            case 0:
                LoadLobby( false );
                break;
            case 1:
                LoadLobby( true );
                break;
            case 2:
                settingsCanvas.SetActive( true );
                _state = MenuState.Settings;
                break;
            case 3:
                creditsCanvas.SetActive( true );
                _state = MenuState.Credits;
                break;
            case 4:
                Application.Quit();
                break;
        }
    }

    void LoadLobby( bool sandbox )
    {
        _loadingLobby = true;
        GameManager.GameConfig.Sandbox = sandbox;
        GlobalInputManager.Instance.ToLobby();
        SceneManager.LoadScene( "Lobby" );
    }

    public void MenuNavLDUR( Vector2Int nav )
    {
        AudioManager.Instance.uiClick.PlaySfx();
        switch( _state )
        {
            case MenuState.Top:
                _topLevelMenuIndex = ( _topLevelMenuIndex + nav.x + topLevelMenuOptions.Length ) % topLevelMenuOptions.Length;
                UpdateMenuOptions();
                break;
            case MenuState.Settings:
                break;
            case MenuState.Credits:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ConfirmSelection()
    {
        switch( _state )
        {
            case MenuState.Top:
                ConfirmTopLevelSelection();
                break;
            case MenuState.Settings:
                break;
            case MenuState.Credits:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void BackSelection()
    {
        AudioManager.Instance.uiClick.PlaySfx();
        switch( _state )
        {
            case MenuState.Top:
                break;
            case MenuState.Settings:
                settingsCanvas.SetActive( false );
                _state = MenuState.Top;
                break;
            case MenuState.Credits:
                creditsCanvas.SetActive( false );
                _state = MenuState.Top;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void UpdateMenuOptions()
    {
        for( var i = 0; i < topLevelMenuOptions.Length; i++ )
        {
            var img = topLevelMenuOptions[ i ].GetComponent<Image>();
            var icon = img.GetComponentsInChildren<Image>()[ 1 ];
            var txt = img.GetComponentInChildren<TextMeshProUGUI>();
            if( i == _topLevelMenuIndex )
            {
                img.color = new Color( 0.8f, 0.8f, 0.8f, 0.39f );
                icon.color = Color.white;
                txt.color = Color.white;
            }
            else
            {
                img.color = new Color( 0.34f, 0.34f, 0.34f, 0.39f );
                icon.color = new Color( 1f, 1f, 1f, 0.34f );
                txt.color = new Color( 1f, 1f, 1f, 0.34f );
            }
        }
    }
    
    
}
