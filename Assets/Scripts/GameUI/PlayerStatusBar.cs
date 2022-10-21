using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBar : MonoBehaviour
{
    [ SerializeField ] TextMeshProUGUI level;
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
    // We Want the follow bar to scale with max hp, so higher healths take longer to go down
    const float HpFollowBarSpeedDivisor = 18;
    Player _player;
    float _maxHp;
    float _maxHpBarWidth;
    float _hpBarHeight;
    float _hpPct;
    float _hpFollowPct;

    void Start()
    {
        _player = GetComponentInParent<Player>();
        _maxHp = _player.MaxHp();
        _hpPct = _player.HpPct();
        _hpFollowPct = _hpPct;
        
        var sizeDelta = hpBar.rectTransform.sizeDelta;
        _maxHpBarWidth = sizeDelta.x;
        _hpBarHeight = sizeDelta.y;
        
        SetHpBarNotches();
    }

    void Update()
    {
        _hpPct = _player.HpPct();
        _hpFollowPct = Mathf.MoveTowards( _hpFollowPct, _hpPct, HpFollowBarSpeedDivisor / _maxHp * Time.deltaTime );
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
