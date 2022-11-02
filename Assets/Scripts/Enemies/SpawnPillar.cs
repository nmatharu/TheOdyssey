using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPillar : MonoBehaviour
{
    [ SerializeField ] ParticleSystem pfx;
    [ SerializeField ] float spawnDelay;

    GameObject _toSpawn;
    float _radius;

    public void Set( GameObject o, float r )
    {
        _toSpawn = o;
        _radius = r;
    }

    void Start()
    {
        pfx.Play();
        Invoke( nameof( Spawn ), spawnDelay );
    }

    void Spawn()
    {
        pfx.Stop();
        var o = Instantiate( _toSpawn, transform.position, Quaternion.identity, EnemySpawner.Instance.EnemiesParent() );
        o.GetComponent<Enemy>().Init( GameManager.Instance.EnemyLevel() );
        Destroy( gameObject, 2f );
    }
}
