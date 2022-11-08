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
    
    void Start()
    {
        if( Instance != null && Instance != this )
            Destroy( this );
        else
            Instance = this;
        
        UpdateMenuOptions();
    }

    void Update()
    {
        if( Input.GetKeyDown( KeyCode.A ) )
        {
            _topLevelMenuIndex = ( _topLevelMenuIndex - 1 + 4 ) % 4;
            UpdateMenuOptions();
        }
        if( Input.GetKeyDown( KeyCode.D ) )
        {
            _topLevelMenuIndex = ( _topLevelMenuIndex + 1 ) % 4;
            UpdateMenuOptions();
        }

        if( Input.GetKeyDown( KeyCode.Return ) )
            ConfirmTopLevelSelection();
    }

    void ConfirmTopLevelSelection()
    {
        switch( _topLevelMenuIndex )
        {
            case 0:
                SceneManager.LoadScene( "Lobby" );
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                Application.Quit();
                break;
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
