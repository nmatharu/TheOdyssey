using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [ SerializeField ] float maxHp = 20;
    [ SerializeField ] int spawnCost = 10;
    
    int _level;
    float _hp;
    Material _material;

    void Start()
    {
        _hp = maxHp;
        _material = GetComponentInChildren<Renderer>().material;
    }

    public void SetLevel( int lvl ) => _level = lvl;
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
        for( var i = 0f; i < 1; i += 0.1f )
        {
            _material.color = new Color( i, i, i );
            yield return delay;
        }
        _material.color = Color.white;
    }

    void Die() => Destroy( gameObject );

    public int Level() => _level;
    public float MaxHp() => maxHp;
    public float HpPct() => _hp / maxHp;
}
