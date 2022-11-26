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
        CommonMagicHeal,
        CommonMoveSpeed,
        CommonRollCd,
        CommonBossZone,
        
        RareBleed,
        RareBigHit,
        RareShield,
        RareLowHpDmg,
        RareSiphon,
        RareCampfireHeal,
        RareCashbackCard,

        LegendaryGuardian,
        LegendaryCdOnHit,
        LegendaryLifeSteal,
        LegendaryExplode,
        LegendaryLooting,
        LegendaryOrbitDaggers
    }

    public enum Rarity
    {
        Common,
        Rare,
        Legendary
    }

    [ SerializeField ] Type type;
    [ SerializeField ] Rarity rarity;
    [ SerializeField ] string runeName;
    [ SerializeField ] bool gives0Levels;
    [ TextArea( 1, 2 ) ]
    [ SerializeField ] string description;
    [ SerializeField ] Sprite icon;

    public int LevelsToAdd() => gives0Levels ? 0 : _rarityLevelMap[ (int) rarity ];
}