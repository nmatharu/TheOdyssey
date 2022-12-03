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
    [ SerializeField ] ParticleSystem deathPfx;
    
    Color[] _originalMatColors;
    EnemyStatusBar _statusBar;
    Vector3 _damageNumberPos;

    int _level = 1;
    float _hp;

    HashSet<Guid> _hitGuids;

    void Start()
    {
        _hp = Mathf.RoundToInt( maxHp );
        _originalMatColors = new Color[ renderers.Length ];
        for( var i = 0; i < renderers.Length; i++ )
            _originalMatColors[ i ] = renderers[ i ].material.color;

        _hitGuids = new HashSet<Guid>();
        InvokeRepeating( nameof( ClearGuids ), 0f, 1f );

        _damageNumberPos = transform.position;
    }

    void ClearGuids() => _hitGuids.Clear();

    public void Init( int lvl )
    {
        _level = lvl;
        maxHp *= GameManager.Instance.EnemyHealthMultiplier( _level );
        _statusBar = GetComponentInChildren<EnemyStatusBar>();
        _statusBar.SetLevel( lvl );
    }

    public void TakeDamage( Player p, float dmg, Guid guid )
    {
        if( _hitGuids.Contains( guid ) )
            return;
        _hitGuids.Add( guid );
        
        StartCoroutine( FlashMaterial() );

        var d = Mathf.RoundToInt( dmg );
        _hp -= d;
        var bleed = p.BleedStacks();
        if( bleed > 0 ) StartCoroutine( Bleed( p, bleed ) );
        
        GameManager.Instance.SpawnDamageNumber( _damageNumberPos, d, true );
        p.Statistics().DealDamage( dmg );
        CheckForDeath( p );
    }

    IEnumerator Bleed( Player p, int stacks )
    {
        var wait = new WaitForSeconds( 1f );
        for( var i = 0; i < 3; i++ )
        {
            yield return wait;
            _hp -= stacks;
            GameManager.Instance.SpawnGenericFloating( _damageNumberPos, stacks.ToString(), new Color( 1, 0.3f, 0.3f ), 8f );
            CheckForDeath( p );
        }
        
        /*
         *  var wait = new WaitForSeconds( 1f / stacks );
            for( var i = 0; i < stacks * 3; i++ )
            {
                yield return wait;
                _hp--;
                GameManager.Instance.SpawnGenericFloating( transform.position, "1", new Color( 1, 0.3f, 0.3f ), 8f );
                CheckForDeath( p );
            }
         */
    }

    void CheckForDeath( Player p )
    {
        if( _hp > 0 ) return;
        p.Statistics().KillEnemy();
        GameManager.Instance.AwardGold( spawnCost );
        Die();
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

    void Die()
    {
        Instantiate( deathPfx, transform.position, transform.rotation, GameManager.Instance.effectsParent ).Play();
        Destroy( gameObject );
    }

    public static Enemy FromCollider( Collider c )
    {
        var e = c.GetComponent<Enemy>();
        if( e == null )
            e = c.GetComponentInParent<Enemy>();
        return e;
    }

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
