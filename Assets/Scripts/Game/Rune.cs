using System;
using UnityEngine;

[ Serializable ]
public class Rune
{
    static int[] _tierLevelMap = { 1, 2, 4, 8 };


    public enum RuneTier
    {
        Common,
        Rare,
        Legendary,
        Primordial
    }

    [ SerializeField ] RuneTier tier;
    [ SerializeField ] string name;

    [ Tooltip( "Levels to add to player when they get this rune: typically it's 1, 2, 4, 8 respectively for each of " +
               "the rarity tiers by default, but you may override this by specifying a non-negative value." ) ]
    [ SerializeField ] int overrideLevelsToAdd = -1;

    [ TextArea( 1, 2 ) ]
    [ SerializeField ] string description;

    [ SerializeField ] Sprite icon;

    public int LevelsToAdd() => overrideLevelsToAdd < 0 ? _tierLevelMap[ (int) tier ] : overrideLevelsToAdd;
    public RuneTier Tier() => tier;
    public string Name() => name;
    public string Desc() => description;
    public Sprite Icon() => icon;

    public override string ToString() => name;
}