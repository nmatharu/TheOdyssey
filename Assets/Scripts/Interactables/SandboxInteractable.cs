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
        Die,
        EnemyHpUp,
        EnemyHpDown
    }

    public override void Interact( Player player )
    {
        activatePfx.Play();
        player.SandboxControl( control );
        
        if( control is SandboxControl.EnemyHpDown or SandboxControl.EnemyHpUp )
        {
            var es = FindObjectsOfType<Enemy>();
            // TODO 
        }
    }

    public override bool InteractionLocked( Player player ) => false;
}