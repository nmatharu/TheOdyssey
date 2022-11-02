using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [ SerializeField ] float maxHp = 20;
    [ SerializeField ] public int spawnCost = 10;
    [ SerializeField ] Renderer[] renderers;
    [ SerializeField ] float hitFlashIntensity = 1f;
    
    Color[] _originalMatColors;

    int _level;
    float _hp;

    void Start()
    {
        _hp = maxHp;
        _originalMatColors = new Color[ renderers.Length ];
        for( var i = 0; i < renderers.Length; i++ )
            _originalMatColors[ i ] = renderers[ i ].material.color;
    }

    public void Init( int lvl )
    {
        _level = lvl;
        maxHp *= GameManager.Instance.EnemyHealthMultiplier( _level );
        // TODO Set Status Bar Level
    }

    public void TakeDamage( Player p, float dmg )
    {
        StartCoroutine( FlashMaterial() );

        var d = Mathf.RoundToInt( dmg );
        _hp -= d;
        GameManager.Instance.SpawnDamageNumber( transform.position, d, true );
        p.Statistics().DealDamage( dmg );
        
        if( _hp <= 0 )
        {
            p.Statistics().KillEnemy();
            GameManager.Instance.AwardGold( spawnCost );
            Die();
        }
    }

    IEnumerator FlashMaterial()
    {
        var delay = new WaitForFixedUpdate();
        for( var i = 1f + hitFlashIntensity; i >= 1; i -= hitFlashIntensity / 3f )
        {
            foreach( var r in renderers )
                r.material.color = new Color( i, i, i );
            yield return delay;
        }

        for( var i = 0; i < renderers.Length; i++ )
            renderers[ i ].material.color = _originalMatColors[ i ];
    }

    void Die() => Destroy( gameObject );

    // void Die()
    // {
    //     deathPfx.Play();
    //     deathPfx.transform.parent = gameObject.transform.parent;
    //     deathPfx.transform.position = gameObject.transform.position;
    //     deathPfx.transform.rotation = gameObject.transform.rotation;
    //     Destroy( gameObject );
    //     this.Invoke( () => Destroy( gameObject ), 2f );
    // }

    public int Level() => _level;
    public float MaxHp() => maxHp;
    public float HpPct() => _hp / maxHp;
}
