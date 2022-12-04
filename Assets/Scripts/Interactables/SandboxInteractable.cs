using UnityEngine;

public class SandboxInteractable : Interactable
{
    [ SerializeField ] ParticleSystem activatePfx;
    [ SerializeField ] SandboxControl control;
    
    public enum SandboxControl
    {
        Reset,
        To1Hp,
        ToFull,
        Die
    }

    public override void Interact( Player player )
    {
        activatePfx.Play();
        player.SandboxControl( control );
    }

    public override bool InteractionLocked( Player player ) => false;
}