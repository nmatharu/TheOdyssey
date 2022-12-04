using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetDummyShooter : MonoBehaviour
{
    [ SerializeField ] GameObject fireball;
    [ SerializeField ] Transform spawnPos;
    [ SerializeField ] float shootDelay;
    
    void Start() => InvokeRepeating( nameof( Shoot ), 2f, shootDelay );
    void Shoot()
    {
        var o = Instantiate( fireball, spawnPos.position, Quaternion.identity, transform );
        var f = o.GetComponent<SkullFireball>();
        
        f.Init( 1, transform.position + -1 * transform.forward );
    }
}
