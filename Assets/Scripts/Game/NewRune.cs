using System;
using UnityEngine;

[ Serializable ]
public class NewRune
{
    static int[] _rarityLevelMap = { 1, 2, 4 };

    public enum Type
    {
        CommonMeleeDmg,
        CommonMagicPower,
        CommonMagicCd,
        CommonMaxHp,
        CommonHpRegen,
        CommonMoveSpeed,
        CommonRollCd,
        CommonMagicHeal,
        CommonBleed,
        CommonBigHit,
        
        GoldLowHealthDmg,
        GoldSiphon,
        GoldEnemiesExplode,
        GoldCashbackCard,
        GoldShield,
        GoldGuardian,
        GoldReduceCdOnHit,
        GoldLifeSteal,
        GoldLightning
        
        // CommonMeleeDmg,
        // CommonMagicPower,
        // CommonMagicCd,
        // CommonMaxHp,
        // CommonHpRegen,
        // CommonMagicHeal,
        // CommonMoveSpeed,
        // CommonRollCd,
        // CommonBossZone,
        //
        // RareBleed,
        // RareBigHit,
        // RareShield,
        // RareLowHpDmg,
        // RareSiphon,
        // RareCampfireHeal,
        // RareCashbackCard,
        // RareLightning,
        //
        // LegendaryGuardian,
        // LegendaryCdOnHit,
        // LegendaryLifeSteal,
        // LegendaryExplode,
        // LegendaryLooting,
        // LegendaryOrbitDaggers
    }

    public enum Rarity
    {
        Common,
        Rare,
        Legendary
    }

    [ SerializeField ] public Type type;
    [ SerializeField ] public Rarity rarity;
    [ SerializeField ] public string runeName;
    [ SerializeField ] public bool gives0Levels;
    [ TextArea( 1, 2 ) ]
    [ SerializeField ] public string description;
    [ SerializeField ] public Sprite icon;
    [ SerializeField ] bool disabled;

    public int LevelsToAdd() => gives0Levels ? 0 : _rarityLevelMap[ (int) rarity ];
}