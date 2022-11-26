using System;
using UnityEngine;

[ Serializable ]
public class NewRune : MonoBehaviour
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

        LegendaryGuardian,
        LegendaryCdOnHit,
        LegendaryLifeSteal,
        LegendaryExplode,
        LegendaryLooting
    }

    public enum Rarity
    {
        Common,
        Rare,
        Legendary
    }

    [ SerializeField ] public Rarity rarity;
    [ SerializeField ] public string runeName;
    [ SerializeField ] bool gives0Levels;
    [ TextArea( 1, 2 ) ]
    [ SerializeField ] public string description;
    [ SerializeField ] public Sprite icon;

    public int LevelsToAdd() => gives0Levels ? 0 : _rarityLevelMap[ (int) rarity ];
}