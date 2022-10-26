using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [ SerializeField ] string interactPrompt;
    [ SerializeField ] string cannotInteractPrompt;

    void OnTriggerEnter( Collider o )
    {
        var p = o.GetComponent<Player>();
        if( p != null )
            p.AddInteractable( this );
    }

    void OnTriggerExit( Collider o )
    {
        var p = o.GetComponent<Player>();
        if( p != null )
            p.RemoveInteractable( this );
    }

    public abstract void Interact();
    public string InteractPrompt() => interactPrompt;
    public string CannotInteractPrompt() => cannotInteractPrompt;
}
