using UnityEngine;

public class PlayerRunes : MonoBehaviour
{
    Player _player;
    int[] _runes;


    [ SerializeField ] MeshRenderer shieldMesh;
    bool _shieldUp;

    void Start()
    {
        _player = GetComponent<Player>();
        _runes = new int[ RuneIndex.Instance.runeIndex.Length ];
    }

    int Count( NewRune.Type type ) => _runes[ (int) type ];

    public void AcquireRune( NewRune rune )
    {
        Debug.Log( "Processing purchase of " + rune.runeName );
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

    public float OutgoingDamageCalc( float unscaledDmg, bool melee, bool magic )
    {
        var dmg = unscaledDmg;

        
        
        return dmg;
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
        // TODO implement
    }
}