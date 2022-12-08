using UnityEngine;

[ CreateAssetMenu( fileName = "LightFire", menuName = "Scriptable Objects/LightFire" ) ]
public class LightFire : MagicSpell
{
    [ SerializeField ] GameObject lightBall;
    
    public override bool Cast( Player player )
    {
        AudioManager.Instance.lightfireShoot.RandomEntry().PlaySfx( 1f, 0.1f );
        var t = player.transform;
        var o = Instantiate( lightBall, t.position, t.rotation );
        o.GetComponent<LightFireBall>().Init( player );
        return true;
    }
}