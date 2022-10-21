using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusBar : MonoBehaviour
{
    // TODO This class is nearly identical to the PlayerStatusBar,
    // Should maybe refactor Player and Enemy into a superclass
    [ SerializeField ] TextMeshProUGUI enemyLevel;
    [ SerializeField ] Image hpBar;
    [ SerializeField ] Image hpFollowBar;

    [ SerializeField ] Transform notchesParent;
    [ SerializeField ] GameObject notch;
    [ SerializeField ] int notchDivision = 10;
    [ SerializeField ] float maxNotchWidth = 0.08f;
    [ SerializeField ] float minNotchWidth = 0.04f;
    [ SerializeField ] int maxNotchWidthThreshold = 50;
    [ SerializeField ] int minNotchWidthThreshold = 200;
    
    const float HpFollowBarSpeed = 0.6f;
    Enemy _enemy;
    float _maxHp;
    float _maxHpBarWidth;
    float _hpBarHeight;
    float _hpPct;
    float _hpFollowPct;

    void Start()
    {
        _enemy = GetComponentInParent<Enemy>();
        _maxHp = _enemy.MaxHp();
        _hpPct = _enemy.HpPct();
        _hpFollowPct = _hpPct;
        
        var sizeDelta = hpBar.rectTransform.sizeDelta;
        _maxHpBarWidth = sizeDelta.x;
        _hpBarHeight = sizeDelta.y;
        
        SetHpBarNotches();
    }

    void Update()
    {
        _hpPct = _enemy.HpPct();
        _hpFollowPct = Mathf.MoveTowards( _hpFollowPct, _hpPct, HpFollowBarSpeed * Time.deltaTime );
        hpBar.rectTransform.sizeDelta = new Vector2( _hpPct * _maxHpBarWidth, _hpBarHeight );
        hpFollowBar.rectTransform.sizeDelta = new Vector2( _hpFollowPct * _maxHpBarWidth, _hpBarHeight );
    }

    void SetHpBarNotches()
    {
        foreach( Transform n in notchesParent )
            Destroy( n.gameObject );

        var notchWidth = JBB.ClampedMap( _maxHp, 
            minNotchWidthThreshold, maxNotchWidthThreshold,
            minNotchWidth, maxNotchWidth );

        for( var hp = notchDivision; hp < _maxHp; hp += notchDivision )
        {
            var image = Instantiate( notch, notchesParent ).GetComponent<Image>();
            image.rectTransform.sizeDelta = new Vector2( notchWidth, _hpBarHeight );
            image.rectTransform.anchoredPosition = 
                new Vector3( hp / _maxHp * _maxHpBarWidth, 0, 0 );
        }
    }
}