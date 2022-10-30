using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCTest : Interactable
{
    public override void Interact() => Debug.Log( "Interact w/ NPC" );

    public override bool InteractionLocked( Player player ) => false;
}
