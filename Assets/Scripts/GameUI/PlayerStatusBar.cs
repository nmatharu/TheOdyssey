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
    [ SerializeField ] Image rollCdBar;

    [ SerializeField ] Transform statusBarTransform;
    [ SerializeField ] Transform inventoryBarTransform;
    int _statusBarState;    // 0 - hp bar, 1-3 - inventory indices 0-2
    
    [ SerializeField ] Transform notchesParent;
    [ SerializeField ] GameObject notch;
    [ SerializeField ] int notchDivision = 10;
    [ SerializeField ] float maxNotchWidth = 0.08f;
    [ SerializeField ] float minNotchWidth = 0.04f;
    [ SerializeField ] int maxNotchWidthThreshold = 50;
    [ SerializeField ] int minNotchWidthThreshold = 200;

    [ SerializeField ] TextMeshProUGUI currencyNumber;
    [ SerializeField ] TextMeshProUGUI inventoryText;
    [ SerializeField ] RectTransform inventoryHighlightSquare;
    [ SerializeField ] RectTransform[] inventoryDarkSquares;

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

    public void SetRollCdBar( float rollCd ) => StartCoroutine( RollCdBarAnimate( rollCd ) );

    IEnumerator RollCdBarAnimate( float rollCd )
    {
        var height = rollCdBar.rectTransform.sizeDelta.y;
        var remaining = rollCd;
        var wait = new WaitForEndOfFrame();
        while( remaining > 0 )
        {
            rollCdBar.rectTransform.sizeDelta = new Vector2( remaining * 2f, height );
            remaining -= Time.deltaTime;
            yield return wait;
        }

        rollCdBar.rectTransform.sizeDelta = new Vector2( 0, height );
    }

    public void UpdateCurrency( int currency ) => currencyNumber.text = currency.ToString();

    public void CycleInventory()
    {
        _statusBarState = ( _statusBarState + 1 ) % 4;

        if( _statusBarState is 0 or 1 )
        {
            StartCoroutine( RotXTransition( statusBarTransform, _statusBarState == 0 ) );
            StartCoroutine( RotXTransition( inventoryBarTransform, _statusBarState == 1 ) );
        }

        if( _statusBarState.In( 1, 2, 3 ) )
        {
            var itemIndex = _statusBarState - 1;
            inventoryText.text = "ITEM " + _statusBarState;
            inventoryHighlightSquare.position = inventoryDarkSquares[ itemIndex ].position;
        }
        // statusBarTransform.gameObject.SetActive( _statusBarState == 0 );
        // inventoryBarTransform.gameObject.SetActive( _statusBarState == 1 );
    }

    IEnumerator RotXTransition( Transform t, bool rotateIn )
    {
        if( rotateIn )  t.gameObject.SetActive( true );
        t.localEulerAngles = new Vector3( rotateIn ? -90 : 0, 0, 0 );
        var wait = new WaitForEndOfFrame();
        for( var i = 0; i < 18; i++ )
        {
            t.Rotate( 5, 0, 0 );
            yield return wait;
        }
        t.gameObject.SetActive( rotateIn );
    }
}
