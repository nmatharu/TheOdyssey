using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [ SerializeField ] float maxHp = 20;
    [ SerializeField ] public int spawnCost = 10;
    [ SerializeField ] List<Renderer> renderers;
    [ SerializeField ] float hitFlashIntensity = 1f;
    [ SerializeField ] ParticleSystem deathPfx;
    [ SerializeField ] bool deathPfxIdentityRot;
    
    List<Color> _originalMatColors = new();
    EnemyStatusBar _statusBar;

    int _level = 1;
    float _hp;

    HashSet<Guid> _hitGuids;

    void Start()
    {
        _hp = Mathf.RoundToInt( maxHp );
        
        foreach( var r in renderers )
            _originalMatColors.Add( r.material.color );

        _hitGuids = new HashSet<Guid>();
        // InvokeRepeating( nameof( ClearGuids ), 0f, 1f );
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
        
        GameManager.Instance.SpawnDamageNumber( transform.position, d, true );
        p.Statistics().DealDamage( dmg );
        CheckForDeath( p );
    }

    public void Bleed( Player p, int stacks, int bleedDamage ) => 
        StartCoroutine( BleedCoroutine( p, stacks, bleedDamage ) );

    IEnumerator BleedCoroutine( Player p, int stacks, int dmg )
    {
        var wait = new WaitForSeconds( 1f );
        for( var i = 0; i < dmg; i++ )
        {
            yield return wait;
            _hp -= stacks;
            GameManager.Instance.SpawnGenericFloating( transform.position, stacks.ToString(), new Color( 1, 0.3f, 0.3f ), 8f );
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
        p.EnemyKilled( this );
        
        GameManager.Instance.AwardGold( spawnCost );
        if( GetComponent<Boss>() != null )
            GameManager.Instance.AwardCrystal();

        Die();
    }

    IEnumerator FlashMaterial()
    {
        var delay = new WaitForFixedUpdate();
        var original = renderers[ 0 ].material.color;
        for( var i = 1f + hitFlashIntensity; i >= 1; i -= hitFlashIntensity / 3f )
        {
            foreach( var r in renderers )
                r.material.color = new Color( original.r * i, original.g * i, original.b * i );
            yield return delay;
        }

        for( var i = 0; i < renderers.Count; i++ )
            renderers[ i ].material.color = _originalMatColors[ i ];
    }

    void Die()
    {
        Instantiate( deathPfx, transform.position, 
            deathPfxIdentityRot ? Quaternion.identity : transform.rotation, 
            GameManager.Instance.effectsParent ).Play();
        Destroy( gameObject );
    }

    public static Enemy FromCollider( Collider c )
    {
        var e = c.GetComponent<Enemy>();
        if( e != null )
            return e;
        
        var sn = c.GetComponent<SandWormNode>();
        if( sn != null && sn.childNode )
            e = sn.Enemy();
        
        return e;
    }

    public void AddRenderer( Renderer r )
    {
        renderers.Add( r );
        _originalMatColors.Add( r.material.color );
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
