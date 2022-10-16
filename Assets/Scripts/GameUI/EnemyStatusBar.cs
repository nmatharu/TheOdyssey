using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusBar : MonoBehaviour
{
    [ SerializeField ] TextMeshProUGUI enemyLevel;
    [ SerializeField ] Image hpBar;
    [ SerializeField ] Image hpFollowBar;

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
        
        
        // TODO Set HealthBar UI Notches etc.
        
        var sizeDelta = hpBar.rectTransform.sizeDelta;
        _maxHpBarWidth = sizeDelta.x;
        _hpBarHeight = sizeDelta.y;
    }

    void Update()
    {
        _hpPct = _enemy.HpPct();
        _hpFollowPct = Mathf.MoveTowards( _hpFollowPct, _hpPct, HpFollowBarSpeed * Time.deltaTime );
        hpBar.rectTransform.sizeDelta = new Vector2( _hpPct * _maxHpBarWidth, _hpBarHeight );
        hpFollowBar.rectTransform.sizeDelta = new Vector2( _hpFollowPct * _maxHpBarWidth, _hpBarHeight );
    }
}