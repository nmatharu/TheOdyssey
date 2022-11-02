using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [ SerializeField ] protected string interactPrompt;
    [ SerializeField ] protected string cannotInteractPrompt;

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

    public abstract void Interact( Player player );
    public abstract bool InteractionLocked( Player player );
    public string InteractPrompt() => interactPrompt;
    public string CannotInteractPrompt() => cannotInteractPrompt;
    public string Prompt( bool l ) => l ? cannotInteractPrompt : interactPrompt;
}
