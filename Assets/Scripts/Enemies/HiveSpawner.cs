using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveSpawner : MonoBehaviour
{
    [ SerializeField ] GameObject boss;
    [ SerializeField ] ParticleSystem spawnSystem;
    [ SerializeField ] ParticleSystem spawnExplosion;
    [ SerializeField ] ParticleSystem deathExplosion;
    [ SerializeField ] float spawnDelay = 3f;
    [ SerializeField ] float gameEndDelay = 5f;
    [ SerializeField ] float spawnPfxStartRadius = 0.5f;
    [ SerializeField ] float spawnPfxEndRadius = 6f;

    void Start() => StartCoroutine( Spawn() );
    Enemy _hive;

    IEnumerator Spawn()
    {
        this.Invoke( () => spawnExplosion.Play(), spawnDelay - 0.05f );

        for( var elapsed = 0f; elapsed < spawnDelay; elapsed += Time.deltaTime )
        {
            var sh = spawnSystem.shape;
            sh.radius = Mathf.Lerp( spawnPfxStartRadius, spawnPfxEndRadius, elapsed / spawnDelay );
            yield return null;
        }
        
        SpawnHive();

        for( ;; )
        {
            if( _hive == null )
            {
                deathExplosion.Play();
                GameManager.Instance.KillAllEnemies();
                Destroy( gameObject, gameEndDelay );
                yield break;
            }
            yield return null;
        }
    }


    void SpawnHive()
    {
        var t = transform;
        var o = Instantiate( boss, t.position, t.rotation, EnemySpawner.Instance.EnemiesParent() );
        _hive = o.GetComponent<Enemy>();
        _hive.Init( GameManager.Instance.EnemyLevel() );
    }
    
}
