using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPillar : MonoBehaviour
{
    [ SerializeField ] ParticleSystem pfx;
    [ SerializeField ] float spawnDelay;

    EnemyBarrier _barrier;
    GameObject _toSpawn;
    float _radius;

    public void Set( GameObject o, float r )
    {
        _toSpawn = o;
        _radius = r;
    }
    
    public void Set( GameObject o, float r, EnemyBarrier b )
    {
        _toSpawn = o;
        _radius = r;
        _barrier = b;
    }

    void Start()
    {
        pfx.Play();
        Invoke( nameof( Spawn ), spawnDelay );
        AudioManager.Instance.spawnPillars.RandomEntry().PlaySfx( 0.6f );
    }

    void Spawn()
    {
        pfx.Stop();
        var o = Instantiate( _toSpawn, transform.position, Quaternion.identity, EnemySpawner.Instance.EnemiesParent() );
        
        if( _barrier != null )
            _barrier.AddEnemy( o );
        
        o.GetComponent<Enemy>().Init( GameManager.Instance.EnemyLevel() );
        Destroy( gameObject, 2f );
    }
}
