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
        
        
        // TODO Set HealthBar UI Notches etc.
        
        var sizeDelta = hpBar.rectTransform.sizeDelta;
        _maxHpBarWidth = sizeDelta.x;
        _hpBarHeight = sizeDelta.y;
    }

    void Update()
    {
        _hpPct = _player.HpPct();
        _hpFollowPct = Mathf.MoveTowards( _hpFollowPct, _hpPct, HpFollowBarSpeedDivisor / _maxHp * Time.deltaTime );
        hpBar.rectTransform.sizeDelta = new Vector2( _hpPct * _maxHpBarWidth, _hpBarHeight );
        hpFollowBar.rectTransform.sizeDelta = new Vector2( _hpFollowPct * _maxHpBarWidth, _hpBarHeight );
    }
}
