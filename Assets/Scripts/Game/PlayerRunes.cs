using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRunes : MonoBehaviour
{
    [ SerializeField ] Image precisionRadial;
    [ SerializeField ] ParticleSystem precisionPfx;
    [ SerializeField ] Material precisionDimMat;
    [ SerializeField ] Material precisionGlowMat;
    [ SerializeField ] ParticleSystem berserkPfx;
    [ SerializeField ] ParticleSystem shieldPfx;
    [ SerializeField ] AudioSource shieldLoop;
    [ SerializeField ] ParticleSystem guardianPfx;
    [ SerializeField ] Sprite guardianIcon;
    [ SerializeField ] GameObject splatterPfx;
    [ SerializeField ] GameObject chronoBladePfx;
    
    [ SerializeField ] float meleePctIncrease = 0.50f;
    [ SerializeField ] float magicPctIncrease = 0.50f;
    [ SerializeField ] float magicCdHasteAmount = 50f;
    [ SerializeField ] float healthPctIncrease = 0.50f;
    [ SerializeField ] float regenPerMinPerStack = 20f;
    [ SerializeField ] float bonusSpeedPerStack = 2f;
    [ SerializeField ] float rollCdHasteAmount = 100f;
    [ SerializeField ] int magicHealAmount = 4;
    [ SerializeField ] int bleedDamage = 3;
    [ SerializeField ] float fourthHitDmgIncreasePct = 2f;
    [ SerializeField ] float berserkNoHpDmgIncreasePct = 3f;
    [ SerializeField ] float cashbackDiscountPct = 0.2f;
    [ SerializeField ] float shieldBaseCd = 15f;
    [ SerializeField ] float shieldHasteAmount = 75f;
    [ SerializeField ] float guardianBurstRadius = 3f;
    [ SerializeField ] float guardianBurstDamage = 30f;
    
    [ SerializeField ] float splatterBaseDamage = 20f;
    [ SerializeField ] float splatterDamagePerStack = 15f;
    [ SerializeField ] float splatterRadius = 4f;
    [ SerializeField ] float splatterDmgDelayFrames = 2f;
    [ SerializeField ] float cdReduceOnHitPerStack = 1f;
    [ SerializeField ] float lifeStealOnHitPerStack = 1f;
    
    Player _player;
    int[] _runes;
    bool _shieldUp;
    bool _precision;
    bool _berserk;
    bool _splatter;
    int _precisionIndex = 0;
    HashSet<Guid> _hitGuids;
    HashSet<Guid> _fourthHitGuids;
    float _berserkPfxEmission;

    void Start()
    {
        _player = GetComponent<Player>();
        precisionRadial.enabled = false;
        _runes = new int[ RuneIndex.Instance.runeIndex.Length ];
        _hitGuids = new HashSet<Guid>();
        _fourthHitGuids = new HashSet<Guid>();
        _berserkPfxEmission = berserkPfx.emission.rateOverTime.constant;
        
        if( WorldGenerator.Instance.opMode )
            OP();
    }

    // For testing
    public void OP()
    {
        // TODO Remove
        // _runes[ (int) NewRune.Type.GoldEnemiesExplode ] = 221;
        _splatter = true;
        _runes[ (int) NewRune.Type.CommonMeleeDmg ] = 40;
    }

    int Count( NewRune.Type type ) => _runes[ (int) type ];

    public void AcquireRune( NewRune rune )
    {
        _runes[ (int) rune.type ]++;
        AudioManager.Instance.shopPurchase.PlaySfx( 1f );
        
        UpdateStats( rune );
    }

    void UpdateStats( NewRune rune )
    {
        switch( rune.type )
        {
            case NewRune.Type.CommonMaxHp:
                _player.UpdateMaxHealth( 1f + Count( NewRune.Type.CommonMaxHp ) * healthPctIncrease );
                break;
            case NewRune.Type.CommonHpRegen:
                _player.UpdateHealthRegen( regenPerMinPerStack * Count( NewRune.Type.CommonHpRegen ) );
                break;
            case NewRune.Type.CommonMoveSpeed:
                _player.UpdateSpeed( bonusSpeedPerStack * Count( NewRune.Type.CommonMoveSpeed ) );
                break;
            case NewRune.Type.CommonRollCd:
                _player.UpdateRollCd();
                break;
            case NewRune.Type.CommonMagicCd:
                _player.ReduceMagicCdPct( 1 - 100f / ( magicCdHasteAmount + 100f ) );
                break;
            case NewRune.Type.CommonBigHit:
                _precision = Count( NewRune.Type.CommonBigHit ) > 0;
                precisionRadial.enabled = _precision;
                break;
            case NewRune.Type.GoldLowHealthDmg:
                _berserk = Count( NewRune.Type.GoldLowHealthDmg ) > 0;
                berserkPfx.Play();
                break;
            case NewRune.Type.GoldShield:
                if( Count( NewRune.Type.GoldShield ) == 1 )
                    InitShield();
                break;
            case NewRune.Type.GoldEnemiesExplode:
                _splatter = Count( NewRune.Type.GoldEnemiesExplode ) > 0;
                break;
            case NewRune.Type.GoldReduceCdOnHit:
                chronoBladePfx.SetActive( Count( NewRune.Type.GoldReduceCdOnHit ) > 0 );
                break;
        }
    }

    public float IncomingDamageCalc( float unscaledDmg, int enemyLevel )
    {
        var dmg = unscaledDmg * GameManager.Instance.EnemyDamageMultiplier( enemyLevel );
        dmg = Mathf.Max( dmg, 1 );
        
        // If shield is up, dmg -> 0 and break shield
        if( _shieldUp )
        {
            dmg = 0;
            PutShieldOnCd();
        }

        return dmg;
    }

    public float OutgoingDamageCalc( Guid attackId, float unscaledDmg, bool melee, bool magic )
    {
        var dmg = unscaledDmg;

        if( melee )
            dmg *= 1f + meleePctIncrease * Count( NewRune.Type.CommonMeleeDmg );

        if( magic )
            dmg *= MagicEffectiveness();

        if( _precision )
            dmg *= PrecisionMultiplier( attackId );

        if( _berserk )
            dmg *= BerserkMultiplier();

        return dmg;
    }

    float PrecisionMultiplier( Guid attackId )
    {
        var mult = 1f;

        if( _fourthHitGuids.Contains( attackId ) )
            return 1f + fourthHitDmgIncreasePct * Count( NewRune.Type.CommonBigHit );

        if( _hitGuids.Contains( attackId ) )
            return mult;

        if( _precisionIndex == 2 )
            AudioManager.Instance.precisionReady.PlaySfx( 0.7f, 0.1f );
        
        if( _precisionIndex == 3 )
        {
            _fourthHitGuids.Add( attackId );
            AudioManager.Instance.precision.RandomEntry().PlaySfx( 0.7f, 0.2f );
            precisionPfx.Play();
            mult = 1f + fourthHitDmgIncreasePct * Count( NewRune.Type.CommonBigHit );
        }
        else
        {
            _hitGuids.Add( attackId );
        }

        _precisionIndex = ( _precisionIndex + 1 ) % 4;
        precisionRadial.fillAmount = _precisionIndex / 3f;
        precisionRadial.material = _precisionIndex == 3 ? precisionGlowMat : precisionDimMat;
        
        return mult;
    }

    void PutShieldOnCd()
    {
        _shieldUp = false;
        shieldPfx.Stop();
        shieldLoop.Stop();
        AudioManager.Instance.safeguardDown.PlaySfx( 0.2f );
        
        this.Invoke( () =>
        {
            if( _player.dead )  return;
            _shieldUp = true;
            shieldPfx.Play();
            shieldLoop.Play();
        }, shieldBaseCd * ( 100f / ( 100f + shieldHasteAmount * ( Count( NewRune.Type.GoldShield ) - 1 ) ) ) );
    }

    public void OnHit( int enemiesHit, bool melee, bool magic )
    {
        var magicCdStacks = Count( NewRune.Type.GoldReduceCdOnHit );
        var lifeStealStacks = Count( NewRune.Type.GoldLifeSteal );
        
        if( magicCdStacks > 0 && melee )
            _player.ReduceMagicCdFlat( cdReduceOnHitPerStack * magicCdStacks );
        
        if( lifeStealStacks > 0 ) 
            _player.LifeSteal( enemiesHit * lifeStealOnHitPerStack * lifeStealStacks );
    }

    public bool HasChronos() => Count( NewRune.Type.GoldReduceCdOnHit ) > 0;
    
    public void SandboxReset()
    {
        foreach( var rune in RuneIndex.Instance.runeIndex )
        {
            _runes[ (int) rune.type ] = 0;
            UpdateStats( rune );
        }
        berserkPfx.Stop();
    }

    public float MagicEffectiveness() => 1f + magicPctIncrease * Count( NewRune.Type.CommonMagicPower );

    public float MagicCdMultiplier() => 100f / ( magicCdHasteAmount * Count( NewRune.Type.CommonMagicCd ) + 100 );

    public float RollCooldown( float baseCd, float rollTime )
    {
        var minCd = rollTime + 0.2f;
        var reducable = baseCd - minCd;
        reducable *= 100f / ( rollCdHasteAmount * Count( NewRune.Type.CommonRollCd ) + 100f );
        return minCd + reducable;
    }
    
    float BerserkMultiplier()
    {
        // 1 + 3(1-x)^6 from 0 to 1
        return 1f + ( Count( NewRune.Type.GoldLowHealthDmg ) * berserkNoHpDmgIncreasePct ) *
            Mathf.Pow( 1 - _player.HpPct(), 6f );
    }

    public void UpdateBerserkPfx()
    {
        if( !_berserk )  return;
            
        var em = berserkPfx.emission;
        em.rateOverTime = new ParticleSystem.MinMaxCurve( 
            _berserkPfxEmission * Mathf.Pow( 1 - _player.HpPct(), 6f ) );
    }
    
    public bool Guardian()
    {
        if( Count( NewRune.Type.GoldGuardian ) < 1 )   return false;

        var pos = transform.position;
        
        _runes[ (int) NewRune.Type.GoldGuardian ]--;
        AudioManager.Instance.guardian.PlaySfx();
        // AudioManager.Instance.guardian.PlaySfx( 0.8f );
        
        _player.Heal( _player.MaxHp(), true, true, 8f );
        guardianPfx.Play();
        GameManager.Instance.SpawnFloatingIcon( pos + 3 * Vector3.up, 
            guardianIcon, Color.white, 40f, 1f, 3f );

        var guid = Guid.NewGuid();
        var dmg = guardianBurstDamage * Mathf.Sqrt( 0.5f + 0.5f * GameManager.Instance.EnemyLevel() );
        var colliders = Physics.OverlapSphere( pos, guardianBurstRadius );
        foreach( var c in colliders )
        {
            var e = c.GetComponent<Enemy>();
            if( e != null )
                e.TakeDamage( _player, dmg, guid );
        }
        
        return true;
    }

    public void Splatter( Enemy enemy )
    {
        if( !_splatter ) return;

        var pos = enemy.transform.position;
        Instantiate( splatterPfx, pos, Quaternion.identity );
        this.Invoke( () =>
        {
            var guid = Guid.NewGuid();
            var dmg = splatterBaseDamage + splatterDamagePerStack * ( Count( NewRune.Type.GoldEnemiesExplode ) - 1 );
            var colliders = Physics.OverlapSphere( pos, splatterRadius );
            foreach( var c in colliders )
            {
                var e = c.GetComponent<Enemy>();
                if( e != null && e != enemy )
                    e.TakeDamage( _player, dmg, guid );
            }
            
            AudioManager.Instance.splatterSfx.RandomEntry().PlaySfx( 1f, 0.1f );
            
        }, splatterDmgDelayFrames / 50f );
    }
    
    public int MagicHealAmount() => magicHealAmount * Count( NewRune.Type.CommonMagicHeal );

    public int BleedStacks() => Count( NewRune.Type.CommonBleed );

    public int BleedDamage() => bleedDamage;

    public void NextLevel()
    {
        _fourthHitGuids.Clear();
        _hitGuids.Clear();
        
        if( _berserk )  
            berserkPfx.Play();
        
        if( Count( NewRune.Type.GoldShield ) > 0 )
        {
            shieldPfx.Play();
            shieldLoop.Play();
            _shieldUp = true;
        }
        
        precisionRadial.enabled = _precision;
    }

    void InitShield()
    {
        shieldPfx.Play();
        shieldLoop.Play();
        _shieldUp = true;
    }

    public void Cashback( int cost )
    {
        var cashbackStacks = Count( NewRune.Type.GoldCashbackCard );
        if( cashbackStacks == 0 )   return;
        this.Invoke( () =>
        {
            // Max discount of 90%
            var discountedPrice = cost * JBB.ClampedMap( 
                Mathf.Pow( 1 - cashbackDiscountPct, cashbackStacks ), 0, 1, 0.1f, 1f );
            var rebate = Mathf.RoundToInt( cost - discountedPrice );
            
            AudioManager.Instance.cashback.PlaySfx( 1f );
            _player.AwardCurrency( rebate, true, 24f );
            
        }, 0.75f );
    }

    public void Die()
    {
        berserkPfx.Stop();
        shieldPfx.Stop();
        shieldLoop.Stop();
        precisionRadial.enabled = false;
    }
}