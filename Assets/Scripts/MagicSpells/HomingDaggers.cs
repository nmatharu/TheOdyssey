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
        var enemies = colliders.Select( c => c.GetComponent<Enemy>() ).Where( e => e != null ).ToArray();

        if( enemies.Empty() )
            return false;
        
        var totalTime = numDaggers * delayBetweenDaggers;
        var scaledNumDaggers = (int) ( numDaggers * 1f );
        var scaledTime = totalTime / scaledNumDaggers;
        
        var targets = new Enemy[ scaledNumDaggers ];
        for( var i = 0; i < scaledNumDaggers; i++ )
            targets[ i ] = enemies[ i % enemies.Length ];

        player.StartCoroutine( FireDaggers() );
        IEnumerator FireDaggers()
        {
            // var randPitch = Random.Range( 1.0f, 1.5f );
            
            var wait = new WaitForSeconds( scaledTime );
            for( var i = 0; i < scaledNumDaggers && !player.dead; i++ )
            {
                var o = Instantiate( homingDagger, player.transform.position, Quaternion.identity );
                o.GetComponent<HomingDagger>().Init( player, targets[ i ], baseDamage, timeToTarget );
                
                yield return wait;
            }
        }

        return true;
    }
}