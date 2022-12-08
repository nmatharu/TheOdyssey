using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : MonoBehaviour
{
    [ SerializeField ] public float baseMaxHp = 30f;
    [ SerializeField ] float baseRollSpeedMultiplier = 1.5f;
    [ SerializeField ] float speed = 8f;

    [ SerializeField ] Vector3 levelStartPosition;
    
    [ SerializeField ] Transform meshParent;
    
    [ SerializeField ] ParticleSystem deathFx;
    [ SerializeField ] ParticleSystem magicLearnFx;
    [ SerializeField ] ParticleSystem itemBuyFx;
    
    [ SerializeField ] GameObject[] costumes;

    public float rollDuration = 0.3f;
    public float rollCooldown = 1f;
    float _baseRollCd;
    float _baseSpeed;

    Renderer[] _renderers;
    [ SerializeField ] Renderer[] hitFlashRenderers;
    Color[] _hitFlashOgColors;
    
    InteractPrompt _interactPrompt;
    PlayerMoves _playerMoves;
    PlayerStatusBar _statusBar;
    PlayerRunes _playerRunes;
    PlayerMagic _playerMagic;

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

    float _baseHpRegen;
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
        _playerRunes = GetComponent<PlayerRunes>();
        _playerMagic = GetComponent<PlayerMagic>();
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

        _baseRollCd = rollCooldown;
        _baseSpeed = speed;
        
        _baseHpRegen = GameManager.Instance.BaseHpRegenPerMin();
        _hpRegenPerMin = _baseHpRegen;
        StartCoroutine( RegenerateHealth() );

        _currency = GameManager.Instance.GetInitGold();
        _statusBar.UpdateBag( _currency, _crystals );

        _statusBar.SetPlayerName( _playerName );
        
        var x = GetComponentsInChildren<Renderer>();

        _hitFlashOgColors = new Color[ hitFlashRenderers.Length ];
        for( var i = 0; i < hitFlashRenderers.Length; i++ )
            _hitFlashOgColors[ i ] = hitFlashRenderers[ i ].material.color;
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

        if( inputDisabled )
        {
            _moveInput = Vector2.zero;
            _moveDir = Vector3.zero;
            _body.velocity = Vector3.zero;
        }

        _playerRunes.UpdateBerserkPfx();
        
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
        var dmg = _playerRunes.IncomingDamageCalc( unscaledDmg, enemyLevel );
        if( dmg > 0 )
            TakeDamage( dmg );
    }

    void TakeDamage( float dmg )
    {
        StartCoroutine( FlashMaterial() );

        var d = Mathf.RoundToInt( dmg );
        GameManager.Instance.SpawnDamageNumber( transform.position, d, false );
        _statistics.TakeDamage( d );

        AudioManager.Instance.playerDamage.PlaySfx( 0.8f );
        GameManager.Instance.HitStop();
        
        _hp -= d;

        if( _hp <= 0 )
        {
            if( _playerRunes.Guardian() )
                return;
            
            Die();
        }
    }

    IEnumerator FlashMaterial()
    {
        var delay = new WaitForFixedUpdate();
        // for( var i = 0f; i < 1; i += 0.05f )
        // {
        //     _material.color = new Color( 1f, i, i );
        //     yield return delay;
        // }

        for( var i = 0; i < hitFlashRenderers.Length; i++ )
            hitFlashRenderers[ i ].material.color = _hitFlashOgColors[ i ];
        
        const float hitFlashIntensity = 30f;
        for( var i = 1f + hitFlashIntensity; i >= 1; i -= hitFlashIntensity / 10f )
        {
            for( var j = 0; j < hitFlashRenderers.Length; j++ )
            {
                var r = hitFlashRenderers[ j ];
                r.material.color = _hitFlashOgColors[ j ] + Color.red * i;
            }
            yield return delay;
        }

        for( var i = 0; i < hitFlashRenderers.Length; i++ )
            hitFlashRenderers[ i ].material.color = _hitFlashOgColors[ i ];
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
        _playerRunes.Die();
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

    public void AwardCurrency( int spawnCost, bool showAnimation = true, float textSize = 8f )
    {
        _currency += spawnCost;
        _statusBar.UpdateBag( _currency, _crystals );
        if( showAnimation )
            GameManager.Instance.SpawnGenericFloating( transform.position, $"+{spawnCost}", Color.yellow, textSize );
    }

    public void AwardCrystal()
    {
        _crystals++;
        _statusBar.UpdateBag( _currency, _crystals );
        GameManager.Instance.SpawnGenericFloating( transform.position, "+1", Color.cyan, 24f );
    }

    IEnumerator RegenerateHealth()
    {
        var elapsed = 0f;
        for( ;; )
        {
            if( elapsed > 60f / _hpRegenPerMin )
            {
                if( _hp < _maxHp && gameObject.activeInHierarchy && !dead )
                {
                    _hp = Mathf.Clamp( _hp + 1, 0, _maxHp );
                    GameManager.Instance.SpawnGenericFloating( transform.position + Vector3.up, "+", Color.green, 6 );
                }

                elapsed -= 60f / _hpRegenPerMin;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public float MaxHp() => _maxHp;
    public float HpPct() => _hp / _maxHp;
    public Animator Mator() => _animator;
    public InGameStatistics Statistics() => _statistics;

    public bool CantAfford( int cost ) => _currency < cost;

    public void BuyRune( NewRune rune, int cost )
    {
        _currency -= cost;
        _statusBar.UpdateBag( _currency, _crystals );
        GameManager.Instance.SpawnCostNumber( transform.position, cost );
        
        // Debug.Log( $"Bought the {rune.runeName} rune" );
        
        itemBuyFx.Play();

        _level += rune.LevelsToAdd();
        _statusBar.SetLevel( _level );

        _playerRunes.Cashback( cost );
        _playerRunes.AcquireRune( rune );
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
        }
    }

    int StacksOfRune( ItemDirector.Runes rune ) => _runeMap[ (int) rune ];

    public void UpdateMaxHealth( float baseMultiplier )
    {
        var oldMaxHp = _maxHp;
        _maxHp = baseMaxHp * baseMultiplier;
        var diff = _maxHp - oldMaxHp;
        _hp = Mathf.Clamp( _hp + diff, 0, _maxHp );
        _statusBar.SetHpBarNotches( _maxHp );
    }

    void UpdateRollSpeed( int count ) => _rollSpeedMultiplier = baseRollSpeedMultiplier + 0.5f * count;
    
    public void DamageEnemy( Guid attackId, Enemy enemy, float baseDamage, bool melee, bool magic )
    {
        var damage = (int) _playerRunes.OutgoingDamageCalc( attackId, baseDamage, melee, magic );
        enemy.TakeDamage( this, damage, attackId );

        var bleedStacks = _playerRunes.BleedStacks();
        if( bleedStacks > 0 )
            enemy.Bleed( this, bleedStacks, _playerRunes.BleedDamage() );
    }

    public void BackToLevelStart()
    {
        transform.position = levelStartPosition;
        _hp = _maxHp;
        if( _playerRunes != null )
            _playerRunes.NextLevel();
    }

    public void LifeSteal( float amountToHeal = 1f )
    {
        _hp = Mathf.Clamp( _hp + amountToHeal, 0, _maxHp );
        GameManager.Instance.SpawnGenericFloating( transform.position + Vector3.up, "+", Color.green, 12 );
    }

    public void CampfireHeal() => Heal( 1, true, false, 16f );

    public void Heal( float amount, bool showText, bool showTextIfFull, float textSize )
    {
        if( showText )
        {
            if( showTextIfFull || _hp < _maxHp )
            {
                GameManager.Instance.SpawnGenericFloating( transform.position + Vector3.up, "+", Color.green, textSize );
            }
        }
        _hp = Mathf.Clamp( _hp + amount, 0, _maxHp );
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
        
        AudioManager.Instance.magicLearn.RandomEntry().PlaySfx( 1f, 0.05f );

        _magic = magicSpell;
        _statusBar.UpdateMagicIcon( _magic );
        
        magicLearnFx.Play();
        GameManager.Instance.SpawnGenericFloating( transform.position, 
            $"LEARNED\n{_magic.magicName}", Color.magenta, 8f );
        
        _statusBar.FlashMagicIcon();
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

        var success = _magic.Cast( this );
        if( !success )
        {
            GameManager.Instance.SpawnGenericFloating( transform.position + 4 * Vector3.down,
                "NO TARGETS", Color.white, 4f );
            return;
        }

        var magicHeal = _playerRunes.MagicHealAmount();
        if( magicHeal > 0 )
            Heal( magicHeal, true, true, 20f );
        
        StartCoroutine( MagicCooldown( _magic.cooldownSeconds * _playerRunes.MagicCdMultiplier() ) );
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

        AudioManager.Instance.magicReady.PlaySfx( 0.8f );
        _magicOnCooldown = false;
        _statusBar.EndMagicCd();
    }

    public void ReduceMagicCdFlat( float amount )
    {
        _magicCdCountdown -= amount;
        if( _magicOnCooldown )
            _statusBar.FlashMagicIcon( 5f );
    }

    public void ReduceMagicCdPct( float pct )
    {
        _magicCdCountdown *= 1 - pct;
        if( _magicOnCooldown )
            _statusBar.FlashMagicIcon( 5f );
    }

    public void SetPlayerName( string playerName ) => _playerName = playerName;
    public string PlayerName() => _playerName;

    public void SetCostume( int costumeIndex )
    {
        foreach( var c in costumes )
            c.SetActive( false );
        if( costumeIndex >= 0 && costumeIndex < costumes.Length )
            costumes[ costumeIndex ].SetActive( true );

        // Become Santa
        this.Invoke( () =>
        {
            if( costumes[ costumeIndex ].name.Contains( "Santa" ) && _statusBar != null )
                _statusBar.BecomeSanta();
        }, 2f );
    }

    public MagicSpell MagicSpell() => _magic;
    
    public float MagicEffectiveness() => _playerRunes.MagicEffectiveness();
    
    public void UpdateHealthRegen( float bonusRegen ) => _hpRegenPerMin = _baseHpRegen + bonusRegen;
    public void UpdateRollCd() => rollCooldown = _playerRunes.RollCooldown( _baseRollCd, rollDuration );
    
    public void UpdateSpeed( float bonusSpeed )
    {
        speed = _baseSpeed + bonusSpeed;
        _playerMoves.SetRunSpeed( Mathf.Pow( speed / 8f, 2 / 3f ) );
    }

    public void EnemyKilled( Enemy enemy ) => _playerRunes.Splatter( enemy );
    
    public void OnHit( int enemiesHit, bool melee, bool magic )
    {
        if( melee )
            AudioManager.Instance.enemyHit.RandomEntry().PlaySfx( 0.8f, 0.3f );

        _playerRunes.OnHit( enemiesHit, melee, magic );
    }

    public PlayerRunes Runes() => _playerRunes;
    public PlayerMagic Magic() => _playerMagic;
    
    // SANDBOX
    
    public void InitSandbox()
    {
        _currency = 0;
        AwardCurrency( 999 );
    }

    public void SandboxControl( SandboxInteractable.SandboxControl control )
    {
        switch( control )
        {
            case SandboxInteractable.SandboxControl.Reset:
                _currency = 999;
                _level = 1;
                _magic = null;
                _hp = _maxHp;
                _statusBar.UpdateBag( _currency, _crystals );
                _statusBar.SetLevel( _level );
                _statusBar.ResetMagic();
                _playerRunes.SandboxReset();
                break;
            case SandboxInteractable.SandboxControl.To1Hp:
                TakeDamage( _hp - 1 );
                break;
            case SandboxInteractable.SandboxControl.ToFull:
                GameManager.Instance.SpawnGenericFloating( transform.position + Vector3.up, "+", Color.green, 24 );
                _hp = _maxHp;
                break;
            case SandboxInteractable.SandboxControl.Die:
                TakeDamage( _hp );
                break;
        }
    }
}