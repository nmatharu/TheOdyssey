using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatusBar : MonoBehaviour
{
    [ SerializeField ] TextMeshProUGUI level;
    [ SerializeField ] TextMeshProUGUI playerName;
    [ SerializeField ] Image hpBar;
    [ SerializeField ] Image hpFollowBar;
    [ SerializeField ] Image rollCdBar;

    [ SerializeField ] Transform statusBarTransform;
    [ SerializeField ] Transform inventoryBarTransform;
    int _statusBarState; // 0 - hp bar, 1-3 - inventory indices 0-2

    [ SerializeField ] Transform notchesParent;
    [ SerializeField ] GameObject notch;
    [ SerializeField ] int notchDivision = 10;
    [ SerializeField ] float maxNotchWidth = 0.08f;
    [ SerializeField ] float minNotchWidth = 0.04f;
    [ SerializeField ] int maxNotchWidthThreshold = 50;
    [ SerializeField ] int minNotchWidthThreshold = 200;

    [ SerializeField ] TextMeshProUGUI currencyNumber;
    [ SerializeField ] TextMeshProUGUI crystalNumber;

    [ SerializeField ] GameObject magicCircleParent;
    [ SerializeField ] Image magicIcon;
    [ SerializeField ] GameObject cooldownParent;
    [ SerializeField ] Image cooldownRadial;
    [ SerializeField ] Image cooldownFlashReady;
    [ SerializeField ] TextMeshProUGUI cooldownSeconds;

    const float HpFollowBarSpeed = 0.6f;

    // We Want the follow bar to scale with max hp, so higher healths take longer to go down
    const float HpFollowBarSpeedDivisor = 18;
    Player _player;
    float _maxHpBarWidth;
    float _hpBarHeight;
    float _hpPct;
    float _hpFollowPct;

    WorldSpaceOverlayUI _overlayUI;

    void Start()
    {
        _player = GetComponentInParent<Player>();
        _hpPct = _player.HpPct();
        _hpFollowPct = _player.HpPct();

        var sizeDelta = hpBar.rectTransform.sizeDelta;
        _maxHpBarWidth = sizeDelta.x;
        _hpBarHeight = sizeDelta.y;
        
        magicCircleParent.SetActive( false );
        cooldownParent.SetActive( false );
        
        _overlayUI = GetComponent<WorldSpaceOverlayUI>();
        SetHpBarNotches( _player.baseMaxHp );
    }

    void Update()
    {
        if( float.IsNaN( _hpFollowPct ) )
            _hpFollowPct = _player.HpPct();

        _hpPct = _player.HpPct();
        _hpFollowPct = Mathf.MoveTowards( _hpFollowPct, _hpPct, HpFollowBarSpeed * Time.deltaTime );
        hpBar.rectTransform.sizeDelta = new Vector2( _hpPct * _maxHpBarWidth, _hpBarHeight );
        hpFollowBar.rectTransform.sizeDelta = new Vector2( _hpFollowPct * _maxHpBarWidth, _hpBarHeight );
    }

    public void SetLevel( int lvl ) => level.text = lvl.ToString();

    public void SetHpBarNotches( float maxHp )
    {
        foreach( Transform n in notchesParent )
            Destroy( n.gameObject );

        var notchWidth = JBB.ClampedMap( maxHp,
            minNotchWidthThreshold, maxNotchWidthThreshold,
            minNotchWidth, maxNotchWidth );

        for( var hp = notchDivision; hp < maxHp; hp += notchDivision )
        {
            var image = Instantiate( notch, notchesParent ).GetComponent<Image>();
            image.rectTransform.sizeDelta = new Vector2( notchWidth, _hpBarHeight );
            image.rectTransform.anchoredPosition =
                new Vector3( hp / maxHp * _maxHpBarWidth, 0, 0 );
        }

        _overlayUI.ReRun();
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

    public void UpdateMagicIcon( MagicSpell magic )
    {
        magicCircleParent.SetActive( true );
        magicIcon.sprite = magic.magicIcon;
    }

    public void StartMagicCd( int seconds )
    {
        cooldownParent.SetActive( true );
        cooldownSeconds.text = seconds.ToString();
        StartCoroutine( FlashCdReady() );
    }

    public void UpdateMagicCd( float secondsRemaining, float totalSeconds )
    {
        cooldownRadial.fillAmount = secondsRemaining / totalSeconds;
        cooldownSeconds.text = ( (int) ( secondsRemaining + 1f ) ).ToString();
    }

    public void EndMagicCd()
    {
        cooldownParent.SetActive( false );
        StartCoroutine( FlashCdReady() );
    }
    
    IEnumerator FlashCdReady()
    {
        var wait = new WaitForFixedUpdate();
        for( var i = 15; i > 0; i-- )
        {
            cooldownFlashReady.color = new Color( 1f, 1f, 1f, i * 0.05f );
            yield return wait;
        }
        cooldownFlashReady.color = Color.clear;
    }

    public void UpdateBag( int currency, int crystals )
    {
        currencyNumber.text = currency.ToString();
        crystalNumber.text = crystals.ToString();
    }

    public void ShowInventory( bool b )
    {
        var oldState = _statusBarState;
        _statusBarState = b ? 1 : 0;

        if( oldState == _statusBarState ) return;
        StartCoroutine( RotXTransition( statusBarTransform, _statusBarState == 0 ) );
        StartCoroutine( RotXTransition( inventoryBarTransform, _statusBarState == 1 ) );
    }
    
    IEnumerator RotXTransition( Transform t, bool rotateIn )
    {
        if( rotateIn ) t.gameObject.SetActive( true );
        t.localEulerAngles = new Vector3( rotateIn ? -90 : 0, 0, 0 );
        var wait = new WaitForEndOfFrame();
        for( var i = 0; i < 10; i++ )
        {
            t.Rotate( 9, 0, 0 );
            yield return wait;
        }

        t.localEulerAngles = new Vector3( rotateIn ? 0 : 90, 0, 0 );
        t.gameObject.SetActive( rotateIn );
    }

    public void SetPlayerName( string n ) => playerName.text = n;
}