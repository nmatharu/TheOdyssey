using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractPrompt : MonoBehaviour
{
    [ SerializeField ] Transform parent;
    [ SerializeField ] Image interactButtonIcon;
    [ SerializeField ] Image lockedIcon;
    [ SerializeField ] TextMeshProUGUI textMesh;
    [ SerializeField ] TextMeshProUGUI lockedTextMesh;
    
    void Start() => Hide();

    void Show() => parent.gameObject.SetActive( true );
    public void Hide() => parent.gameObject.SetActive( false );

    public void SetInteractable( bool locked, string text )
    {
        Show();
        interactButtonIcon.enabled = !locked;
        textMesh.enabled = !locked;
        lockedIcon.enabled = locked;
        lockedTextMesh.enabled = locked;
        textMesh.text = text;
        lockedTextMesh.text = text;
    }
}
