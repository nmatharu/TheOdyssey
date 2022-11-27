using UnityEngine;

[ CreateAssetMenu( fileName = "LightFire", menuName = "Scriptable Objects/LightFire" ) ]
public class LightFire : MagicSpell
{
    [ SerializeField ] GameObject lightBall;
    
    public override void Cast( Player player )
    {
        var t = player.transform;
        var o = Instantiate( lightBall, t.position, t.rotation );
        o.GetComponent<LightFireBall>().Init( player );
    }
}