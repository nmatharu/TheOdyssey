using UnityEngine;

[ CreateAssetMenu( fileName = "AOEHeal", menuName = "Scriptable Objects/AOEHeal" ) ]
public class AOEHeal : MagicSpell
{
    [ SerializeField ] ParticleSystem healPfx;
    [ SerializeField ] float healRadius = 8f;
    [ SerializeField ] float baseHealAmount = 5f;
    
    public override void Cast( Player player )
    {
        var t = player.transform;
        // var o = Instantiate( healPfx, t.position, t.rotation );

        var healAmount = baseHealAmount * player.MagicEffectiveness();
        
        var colliders = Physics.OverlapSphere( t.position, healRadius );
        foreach( var c in colliders )
        {
            var e = c.GetComponent<Player>();
            if( e != null )
                e.Heal( healAmount, true, false, 16f );
        }
    }
}