using UnityEngine;

public class ItemDirector : MonoBehaviour
{
    [ SerializeField ] GameObject[] commonRuneShops;
    [ SerializeField ] GameObject[] rareRuneShops;
    [ SerializeField ] GameObject[] legendaryRuneShops;

    public static ItemDirector Instance { get; private set; }

    void Awake()
    {
        if( Instance != null && Instance != this )
            Destroy( this );
        else
            Instance = this;
    }

    public GameObject RuneShop()
    {
        var roll = Random.value;
        return roll switch
        {
            < 0.75f => commonRuneShops.RandomEntry(),
            < 0.95f => rareRuneShops.RandomEntry(),
            _ => legendaryRuneShops.RandomEntry()
        };
    }

    public enum RuneTiers
    {
        Common,
        Rare,
        Legendary
    }

    public enum Runes
    {
        CommonMaxHp,
        CommonA,
        CommonB,
        RareA,
        LegendaryA
    }

    public int CommonMaxHpCalc( int stacks, float baseMaxHp ) => (int) ( baseMaxHp * Mathf.Pow( 1.2f, stacks ) );
}