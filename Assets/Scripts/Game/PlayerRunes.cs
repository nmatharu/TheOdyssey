using System;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRunes : MonoBehaviour
{
    [ SerializeField ] MeshRenderer shieldMesh;
    [ SerializeField ] Image precisionRadial;
    [ SerializeField ] ParticleSystem precisionPfx;
    [ SerializeField ] Material precisionDimMat;
    [ SerializeField ] Material precisionGlowMat;
    [ SerializeField ] ParticleSystem berserkPfx;
    
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
    
    Player _player;
    int[] _runes;
    bool _shieldUp;
    bool _precision;
    bool _berserk;
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
    }

    int Count( NewRune.Type type ) => _runes[ (int) type ];

    public void AcquireRune( NewRune rune )
    {
        _runes[ (int) rune.type ]++;
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
        }
    }

    public float IncomingDamageCalc( float unscaledDmg, int enemyLevel )
    {
        var dmg = unscaledDmg * GameManager.Instance.EnemyDamageMultiplier( enemyLevel );

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
        
        if( _precisionIndex == 3 )
        {
            _fourthHitGuids.Add( attackId );
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
        shieldMesh.enabled = false;
        this.Invoke( () =>
        {
            _shieldUp = true;
            shieldMesh.enabled = true;
        }, 10f * Mathf.Pow( 0.8f, Count( NewRune.Type.GoldShield ) - 1 ) );
    }

    public void OnHit( int enemiesHit, bool melee, bool magic )
    {
        // throw new System.NotImplementedException();
    }

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

    public int MagicHealAmount() => magicHealAmount * Count( NewRune.Type.CommonMagicHeal );

    public int BleedStacks() => Count( NewRune.Type.CommonBleed );

    public int BleedDamage() => bleedDamage;

    public void NextLevel() => _hitGuids.Clear();
}