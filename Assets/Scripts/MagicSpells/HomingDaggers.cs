using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ CreateAssetMenu( fileName = "HomingDaggers", menuName = "Scriptable Objects/HomingDaggers" ) ]
public class HomingDaggers : MagicSpell
{
    [ SerializeField ] GameObject homingDagger;
    [ SerializeField ] int numDaggers = 8;
    [ SerializeField ] float delayBetweenDaggers = 0.1f;
    [ SerializeField ] float lockOnRange = 6f;
    [ SerializeField ] float baseDamage = 3f;
    [ SerializeField ] float timeToTarget = 0.5f;
    
    public override bool Cast( Player player )
    {
        var pos = player.transform.position;

        var colliders = Physics.OverlapSphere( pos, lockOnRange );
        var enemies = colliders.Select( c => c.GetComponent<Enemy>() ).Where( e => e != null ).ToList();

        if( enemies.Empty() )
            return false;

        var dmg = baseDamage * player.MagicEffectiveness();
        
        player.StartCoroutine( FireDaggers() );
        IEnumerator FireDaggers()
        {
            var wait = new WaitForSeconds( delayBetweenDaggers );
            for( var i = 0; i < numDaggers && !player.dead; i++ )
            {
                var o = Instantiate( homingDagger, player.transform.position, Quaternion.identity );
                o.GetComponent<HomingDagger>().Init( player, enemies[ 0 ], dmg, timeToTarget );
                
                yield return wait;
            }
        }

        return true;
    }
}