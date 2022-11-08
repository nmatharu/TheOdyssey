using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyTypeChar : MonoBehaviour
{
    [ SerializeField ] float unhoverAlpha = 0.5f;
    [ SerializeField ] TextMeshProUGUI charDisplay;
    [ SerializeField ] Image outlineBox;
    [ SerializeField ] Image imageDisplay;
    [ SerializeField ] Sprite[] sprites;
    
    char _display;

    // Start is called before the first frame update
    void Start()
    {
        Unhover();
    }

    public void Hover()
    {
        charDisplay.color = Color.white;
        outlineBox.enabled = true;
        imageDisplay.color = Color.white;
    }

    public void Unhover()
    {
        charDisplay.color = new Color( 1f, 1f, 1f, unhoverAlpha );
        outlineBox.enabled = false;
        imageDisplay.color = new Color( 1f, 1f, 1f, unhoverAlpha );
    }

    public void Init( char c )
    {
        _display = c;
        charDisplay.text = _display.ToString();
        imageDisplay.enabled = false;
    }

    public void InitSpecial( int x )
    {
        imageDisplay.sprite = sprites[ x ];
        charDisplay.enabled = false;
    }
}
