using UnityEngine;

[ CreateAssetMenu( fileName = "AOEHeal", menuName = "Scriptable Objects/AOEHeal" ) ]
public class AOEHeal : MagicSpell
{
    [ SerializeField ] float healRadius = 8f;
    [ SerializeField ] float baseHealAmount = 5f;
    
    public override bool Cast( Player player )
    {
        var t = player.transform;

        AudioManager.Instance.healMagic.RandomEntry().PlaySfx( 1f, 0.1f );
        player.Magic().PlayAoeHealPfx();
        var healAmount = baseHealAmount * player.MagicEffectiveness();
        
        var colliders = Physics.OverlapSphere( t.position, healRadius );
        foreach( var c in colliders )
        {
            var e = c.GetComponent<Player>();
            if( e != null )
                e.Heal( healAmount, true, false, 16f );
        }

        return true;
    }
}