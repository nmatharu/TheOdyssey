using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

public class Player : MonoBehaviour
{
    [ SerializeField ] public float baseMaxHp = 30f;
    [ SerializeField ] float baseRollSpeedMultiplier = 1.5f;
    [ SerializeField ] float speed = 8f;
    [ SerializeField ] float baseHpRegenPerMinute = 6;

    [ SerializeField ] Vector3 levelStartPosition;
    
    [ SerializeField ] Transform meshParent;
    
    [ SerializeField ] ParticleSystem deathFx;
    [ SerializeField ] ParticleSystem magicLearnFx;
    [ SerializeField ] ParticleSystem itemBuyFx;
    
    [ SerializeField ] GameObject[] costumes;

    public float rollDuration = 0.3f;
    public float rollCooldown = 1f;

    Renderer[] _renderers;
    InteractPrompt _interactPrompt;
    PlayerMoves _playerMoves;
    PlayerStatusBar _statusBar;

    Rigidbody _body;
    Animator _animator;
    Material _material;

    string _playerName;
    int _level = 1;
    float _maxHp;
    float _hp;
    float _rollSpeedMultiplier;
    int _currency;
    int _crystals;
    int[] _runeMap;
    Dictionary<string, int> _runes;

    PlayerMoves.SpecialAttackType _specialAttack;

    HashSet<Interactable> _interactables;
    Interactable _closestInteractable;

    MagicSpell _magic = null;
    bool _magicOnCooldown;
    float _magicCdCountdown;

    InGameStatistics _statistics;

    // For sequences like the intro run-in and outro run-out where we don't want the player to control
    public bool inputDisabled = false;

    public bool showOnCamera = true;
    public bool dead = false;

    public bool rolling = false;
    public bool locked = false; // Locked in attack animation
    public bool stunned = false;
    public bool weaponless = false; // For special attacks that involve throwing the sword
    public bool rollOnCd = false;

    static Vector3 _cameraUp;
    Vector2 _moveInput;
    Vector3 _moveDir;
    Vector3 _lastPos;

    float _hpRegenPerMin;

    static readonly int IsRunning = Animator.StringToHash( "IsRunning" );

    void Start()
    {
        _maxHp = baseMaxHp;
        _hp = _maxHp;
        _rollSpeedMultiplier = baseRollSpeedMultiplier;
        _material = GetComponentInChildren<Renderer>().material;

        _renderers = meshParent.GetComponentsInChildren<Renderer>();
        _interactPrompt = GetComponentInChildren<InteractPrompt>();
        _playerMoves = GetComponent<PlayerMoves>();
        _statusBar = GetComponentInChildren<PlayerStatusBar>();
        _specialAttack = PlayerMoves.SpecialAttackType.Slash;
        _lastPos = transform.position;

        _body = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _interactables = new HashSet<Interactable>();
        _statistics = new InGameStatistics();

        if( Camera.main == null ) return;
        var fwd = Camera.main.transform.forward;
        _cameraUp = new Vector3( fwd.x, 0, fwd.z );

        _runeMap = new int[ Enum.GetNames( typeof( ItemDirector.Runes ) ).Length ];
        _runes = new Dictionary<string, int>();
        foreach( Rune rune in RuneIndex.Instance.AllRunes() )
            _runes.Add( rune.Name(), 0 );

        _hpRegenPerMin = GameManager.Instance.BaseHpRegenPerMin();
        StartCoroutine( RegenerateHealth() );

        _currency = GameManager.Instance.GetInitGold();
        _statusBar.UpdateBag( _currency, _crystals );

        _statusBar.SetPlayerName( _playerName );
        
        var x = GetComponentsInChildren<Renderer>();
    }

    public void Init( string playerName )
    {
        _playerName = playerName;
        _statusBar.SetPlayerName( _playerName );
    }

    void Update()
    {
        FindClosestInteractable();
        UpdateInteractablePrompt();
        
        // TODO remove
        // _playerMoves.SetRunSpeed( Mathf.Sqrt( speed / 8f ) );
    }

    void FixedUpdate()
    {
        if( dead ) return;
        if( rolling )
        {
            _body.velocity = transform.forward * ( _rollSpeedMultiplier * speed );
            return;
        }

        var pos = transform.position;

        _moveDir = locked ? _moveDir : CameraCompensation( _moveInput );
        _body.velocity = locked ? Vector3.zero : _moveDir * speed;

        _animator.SetBool( IsRunning, _moveDir != Vector3.zero );
        transform.LookAt( pos + _moveDir );

        // Statistics junk
        _statistics.Move( Vector3.Distance( _lastPos, pos ) );
        _statistics.AliveForFixedTimeStep();
        _lastPos = transform.position;
    }

    public void InputMovement( Vector2 v ) => _moveInput = v;

    public void LightAttack() => _playerMoves.LightAttack();

    public void SpecialAttack() => _playerMoves.SpecialAttack( _specialAttack );

    public void Roll() => _playerMoves.Roll();

    public void IncomingDamage( float unscaledDmg, int enemyLevel )
    {
        if( rolling ) return;
        TakeDamage( unscaledDmg * GameManager.Instance.EnemyDamageMultiplier( enemyLevel ) );
    }

    void TakeDamage( float dmg )
    {
        StartCoroutine( FlashMaterial() );

        var d = Mathf.RoundToInt( dmg );
        GameManager.Instance.SpawnDamageNumber( transform.position, d, false );
        _statistics.TakeDamage( d );

        _hp -= d;
        if( _hp <= 0 )
            Die();
    }

    IEnumerator FlashMaterial()
    {
        var delay = new WaitForFixedUpdate();
        for( var i = 0f; i < 1; i += 0.05f )
        {
            _material.color = new Color( 1f, i, i );
            yield return delay;
        }

        _material.color = Color.white;
    }

    static Vector3 CameraCompensation( Vector2 dir )
    {
        if( dir == Vector2.zero ) return Vector3.zero;
        var rotDegrees = Mathf.Atan2( dir.x, dir.y ) * Mathf.Rad2Deg;
        var v3Dir = Quaternion.Euler( 0, rotDegrees, 0 ) * _cameraUp;
        return v3Dir.normalized;
    }

    public void AddInteractable( Interactable interactable ) => _interactables.Add( interactable );
    public void RemoveInteractable( Interactable interactable ) => _interactables.Remove( interactable );

    void FindClosestInteractable()
    {
        var pos = transform.position;

        Interactable closest = null;
        var closestDSqr = Mathf.Infinity;
        foreach( var interactable in _interactables.Where( interactable => interactable != null && !interactable.Disabled() ) )
        {
            if( closest == null )
            {
                closest = interactable;
                closestDSqr = JBB.DistXZSquared( pos, interactable.transform.position );
                continue;
            }

            var dSqr = JBB.DistXZSquared( pos, interactable.transform.position );
            if( dSqr < closestDSqr )
            {
                closest = interactable;
                closestDSqr = dSqr;
            }
        }

        _closestInteractable = closest;
    }

    void UpdateInteractablePrompt()
    {
        if( _closestInteractable == null )
        {
            _interactPrompt.Hide();
            return;
        }

        var interactionLocked = _closestInteractable.InteractionLocked( this );
        _interactPrompt.SetInteractable( interactionLocked, _closestInteractable.Prompt( interactionLocked ) );
    }

    public void Interact()
    {
        if( GameManager.Instance.ReadyToExitGame() )
            GameManager.Instance.QuitToLobby();
        
        if( dead || locked || rolling ) return;

        if( _closestInteractable != null && !_closestInteractable.InteractionLocked( this ) )
            _closestInteractable.Interact( this );
    }

    void Die()
    {
        dead = true;

        _statistics.Die();
        Hide();
        deathFx.Play();
        GameManager.Instance.CheckIfGameOver();

        this.Invoke( () =>
        {
            if( dead )
                showOnCamera = false;
        }, 3f );
        // gameObject.SetActive( false );
    }

    public void Respawn()
    {
        if( !dead ) return;
        dead = false;
        showOnCamera = true;
        Hide( false );

        _hp = _maxHp * GameManager.Instance.respawnHealthPct;
    }

    void Hide( bool hide = true )
    {
        _statusBar.gameObject.SetActive( !hide );
        _body.isKinematic = hide;
        _body.detectCollisions = !hide;
        foreach( var r in _renderers )
            r.enabled = !hide;
    }

    public void AwardCurrency( int spawnCost, bool showAnimation = true )
    {
        _currency += spawnCost;
        _statusBar.UpdateBag( _currency, _crystals );
        if( showAnimation )
            GameManager.Instance.SpawnGenericFloating( transform.position, $"+{spawnCost}", Color.yellow, 8f );
    }

    IEnumerator RegenerateHealth()
    {
        for( ;; )
        {
            if( _hp < _maxHp && gameObject.activeInHierarchy && !dead )
            {
                _hp = Mathf.Clamp( _hp + 1, 0, _maxHp );
                GameManager.Instance.SpawnGenericFloating( transform.position + Vector3.up, "+", Color.green, 6 );
            }

            yield return new WaitForSeconds( 60f / _hpRegenPerMin );
        }
    }

    public float MaxHp() => _maxHp;
    public float HpPct() => _hp / _maxHp;
    public Animator Mator() => _animator;
    public InGameStatistics Statistics() => _statistics;

    public bool CantAfford( int cost ) => _currency < cost;

    public void BuyRune( Rune rune, int cost )
    {
        _currency -= cost;
        _statusBar.UpdateBag( _currency, _crystals );
        GameManager.Instance.SpawnCostNumber( transform.position, cost );
        Debug.Log( $"Bought the {rune.Name()} rune" );
        
        itemBuyFx.Play();
        AcquireRune( rune );
    }

    public void AcquireRune( Rune rune )
    {
        _runes[ rune.Name() ]++;
        _level += rune.LevelsToAdd();
        _statusBar.SetLevel( _level );

        var count = _runes[ rune.Name() ];
        switch( rune.Name() )
        {
            case "vitality":
                UpdateMaxHealth( count );
                break;
            case "evasion":
                UpdateRollSpeed( count );
                break;
            case "resolve":
                UpdateHpRegen( count );
                break;
        }
    }

    int StacksOfRune( ItemDirector.Runes rune ) => _runeMap[ (int) rune ];

    void UpdateMaxHealth( int count )
    {
        var oldMaxHp = _maxHp;
        _maxHp = baseMaxHp * GameManager.Scale0( count );
        var diff = _maxHp - oldMaxHp;
        _hp = Mathf.Clamp( _hp + diff, 0, _maxHp );
        _statusBar.SetHpBarNotches( _maxHp );
    }

    void UpdateRollSpeed( int count ) => _rollSpeedMultiplier = baseRollSpeedMultiplier + 0.5f * count;

    public float DamageMultiplier()
    {
        var multiplier = 1f;
        multiplier *= GameManager.Scale0( _runes[ "fury" ] );
        return multiplier;
    }

    public float MeleeMultiplier()
    {
        return 1f;
    }

    public float MagicMultiplier()
    {
        return 1f;
    }

    public void BackToLevelStart()
    {
        transform.position = levelStartPosition;
        _hp = _maxHp;
    }

    public int BleedStacks() => _runes[ "hemorrhage" ];

    void UpdateHpRegen( int count )
    {
        _hpRegenPerMin = baseHpRegenPerMinute + count * 12f;
        // StopCoroutine( nameof( RegenerateHealth ) );
        // StartCoroutine( RegenerateHealth() );
    }

    public void LifeSteal()
    {
        var lifeSteal = _runes[ "vampirism" ];
        _hp = Mathf.Clamp( _hp + lifeSteal, 0, _maxHp );
        if( lifeSteal > 0 )
            GameManager.Instance.SpawnGenericFloating( transform.position + Vector3.up, "+", Color.green, 12 );
    }

    public void CampfireHeal()
    {
        if( _hp < _maxHp )
            GameManager.Instance.SpawnGenericFloating( transform.position + Vector3.up, "+", Color.green, 16 );
        _hp = Mathf.Clamp( _hp + 1, 0, _maxHp );
    }

    public void ShowInventory( bool b ) => _statusBar.ShowInventory( b );

    public bool HasCrystal() => _crystals > 0;

    public void ConsumeCrystal()
    {
        _crystals--;
        _statusBar.UpdateBag( _currency, _crystals );
    }

    public void LearnMagic( MagicSpell magicSpell )
    {
        if( _magic == null )
        {
            _level++;
            _statusBar.SetLevel( _level );
        }

        _magic = magicSpell;
        _statusBar.UpdateMagicIcon( _magic );
        
        magicLearnFx.Play();
        GameManager.Instance.SpawnGenericFloating( transform.position, 
            $"LEARNED\n{_magic.magicName}", Color.magenta, 8f );
        
        if( !_magicOnCooldown )
            _statusBar.EndMagicCd();
        
        // TODO Celebration particles/text on learn magic
    }

    public void CastMagic()
    {
        if( dead || rolling || locked ) return;
    
        if( _magic == null )
        {
            GameManager.Instance.SpawnGenericFloating( transform.position + 4 * Vector3.down,
                "NO MAGIC!", Color.white, 4f );
            return;
        }

        if( _magicOnCooldown )
        {
            GameManager.Instance.SpawnGenericFloating( transform.position + 4 * Vector3.down,
                "ON COOLDOWN", Color.white, 4f );
            return;
        }

        _magic.Cast( this );
        StartCoroutine( MagicCooldown( _magic.cooldownSeconds ) );
    }

    IEnumerator MagicCooldown( float cooldownSeconds )
    {
        _magicOnCooldown = true;
        _statusBar.StartMagicCd( (int) cooldownSeconds );

        _magicCdCountdown = cooldownSeconds;
        while( _magicCdCountdown > 0 )
        {
            _magicCdCountdown -= Time.deltaTime;
            _statusBar.UpdateMagicCd( _magicCdCountdown, cooldownSeconds );

            yield return null;
        }

        _magicOnCooldown = false;
        _statusBar.EndMagicCd();
    }

    public void ReduceMagicCd( float amount ) => _magicCdCountdown -= amount;

    public void SetPlayerName( string playerName ) => _playerName = playerName;
    public string PlayerName() => _playerName;

    public void SetCostume( int costumeIndex )
    {
        foreach( var c in costumes )
            c.SetActive( false );
        if( costumeIndex >= 0 && costumeIndex < costumes.Length )
            costumes[ costumeIndex ].SetActive( true );
    }

    public MagicSpell MagicSpell() => _magic;
}