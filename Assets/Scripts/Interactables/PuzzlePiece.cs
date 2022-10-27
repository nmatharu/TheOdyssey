using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePiece : Interactable
{
    [ SerializeField ] GameObject puzzleLight;
    public override void Interact()
    {
        puzzleLight.SetActive( !puzzleLight.activeInHierarchy );
        Debug.Log( "Interact!" );
    }
}
