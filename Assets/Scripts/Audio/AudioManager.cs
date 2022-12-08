using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    public const string PlayerPrefsMasterKey = "masterVol";
    public const string PlayerPrefsMusicKey = "musicVol";
    public const string PlayerPrefsSfxKey = "sfxVol";
    [ SerializeField ] public float defaultMasterVol = 0.6f;
    [ SerializeField ] public float defaultMusicVol = 1.0f;
    [ SerializeField ] public float defaultSfxVol = 0.8f;

    [ SerializeField ] AudioMixer mixer;
    [ SerializeField ] AudioSource sourcePrefab;
    [ SerializeField ] int maxSfxSources = 32;
    AudioSource[] _sfxSources;

    // 0.0 - 1.0
    public bool muteMusic;
    public bool muteSfx;

    public static AudioManager Instance { get; private set; }

    [ Header( "SFX Clips" ) ]
    [ Header( "Pre-Game" ) ]
    [ SerializeField ] public AudioClip uiClick;
    [ SerializeField ] public AudioClip uiBig;
    [ SerializeField ] public AudioClip lobbyReady;
    [ SerializeField ] public AudioClip lobbyCountdown;
    [ SerializeField ] public AudioClip[] lobbyFootsteps;
    [ SerializeField ] public AudioClip transition;
    [ SerializeField ] public AudioClip[] intermissionType;
    [ SerializeField ] public AudioClip[] swordSwings;
    [ SerializeField ] public AudioClip[] bigSwings;
    [ SerializeField ] public AudioClip[] enemyHit;
    [ SerializeField ] public AudioClip[] grassFootsteps;
    [ SerializeField ] public AudioClip[] logFootsteps;
    [ SerializeField ] public AudioClip[] sandFootsteps;
    [ SerializeField ] public AudioClip[] stoneFootsteps;
    [ SerializeField ] public AudioClip rollSfx;
    [ SerializeField ] public AudioClip[] enemyDeath;
    [ SerializeField ] public AudioClip[] playerHit;
    [ SerializeField ] public AudioClip playerDeath;
    [ SerializeField ] public AudioClip shopPurchase;
    [ SerializeField ] public AudioClip magicReady;
    [ SerializeField ] public AudioClip playerDamage;

    [ Header( "Enemy" ) ]
    [ SerializeField ] public AudioClip wormAtk;
    [ SerializeField ] public AudioClip[] nightmareDash;
    [ SerializeField ] public AudioClip[] nightmareSlash;
    [ SerializeField ] public AudioClip skullFireballHit;
    [ SerializeField ] public AudioClip sandSkullFireballHit;
    [ SerializeField ] public AudioClip golemSmashFalls;
    [ SerializeField ] public AudioClip golemSmashSands;
    [ SerializeField ] public AudioClip golemSmashFires;
    [ SerializeField ] public AudioClip golemSmashBoss;
    [ SerializeField ] public AudioClip golemBossIntroSmash;
    [ SerializeField ] public AudioClip golemBossIntroFade;
    [ SerializeField ] public AudioClip[] golemFootsteps;
    [ SerializeField ] public AudioClip bossWormIntro;
    [ SerializeField ] public AudioClip[] pyramidCharges;
    [ SerializeField ] public AudioClip[] pyramidBlast;

    [ Header( "Hive" ) ]
    [ SerializeField ] public AudioClip hiveIntro;
    [ SerializeField ] public AudioClip hiveIntroBurst;
    [ SerializeField ] public AudioClip hiveChargeUp;
    [ SerializeField ] public AudioClip hiveExploderIndicator;
    [ SerializeField ] public AudioClip hiveExploderBurst;
    [ SerializeField ] public AudioClip hiveGameWinExplosion;

    [ Header( "Environment" ) ]
    [ SerializeField ] public AudioClip[] spawnPillars;
    [ SerializeField ] public AudioClip chestOpen;
    [ SerializeField ] public AudioClip[] magicLearn;
    [ SerializeField ] public AudioClip campfireIgnite;
    [ SerializeField ] public AudioClip spikes;

    [ Header( "Runes" ) ]
    [ SerializeField ] public AudioClip[] splatterSfx;

    [ SerializeField ] public AudioClip[] precision;
    [ SerializeField ] public AudioClip precisionReady;
    [ SerializeField ] public AudioClip bleedTick;
    [ SerializeField ] public AudioClip cashback;
    [ SerializeField ] public AudioClip guardian;
    [ SerializeField ] public AudioClip safeguardDown;
    [ SerializeField ] public AudioClip[] chronos;

    [ Header( "Magic" ) ]
    [ SerializeField ] public AudioClip[] lightfireShoot;
    [ SerializeField ] public AudioClip lightfirePassthrough;
    [ SerializeField ] public AudioClip[] healMagic;
    [ SerializeField ] public AudioClip[] flurryLaunch;
    [ SerializeField ] public AudioClip[] flurryBolt;

    [ Header( "Music" ) ]
    [ SerializeField ] public AudioClip menuTheme;
    [ SerializeField ] public AudioClip[] levelMainLoops;
    [ SerializeField ] public AudioClip[] levelBossLoops;
    [ SerializeField ] public AudioClip[] levelBossEndRiffs;

    AudioSource _music;

    void Awake()
    {
        if( Instance != null && Instance != this )
        {
            Destroy( gameObject );
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad( this );

            _sfxSources = new AudioSource[ maxSfxSources ];
            for( var i = 0; i < maxSfxSources; i++ )
            {
                _sfxSources[ i ] = Instantiate( sourcePrefab );
                _sfxSources[ i ].transform.parent = transform;
            }
        }
    }

    void Start()
    {
        SetAudioListenerVolume( PlayerPrefs.GetFloat( PlayerPrefsMasterKey, defaultMasterVol ) );
        SetMusicVolume( PlayerPrefs.GetFloat( PlayerPrefsMusicKey, defaultMusicVol ) );
        SetSfxVolume( PlayerPrefs.GetFloat( PlayerPrefsSfxKey, defaultSfxVol ) );

        _music = GetComponent<AudioSource>();
        switch( SceneManager.GetActiveScene().name )
        {
            case "Menu":
            case "Lobby":
                PlayMusic( menuTheme, true, true );
                break;
            case "Game":
                PlayMusic( levelMainLoops[ 0 ], true, true );
                break;
        }
        
        // menuMusicLoop.loop = true;
    }

    public AudioSource Music() => _music;
    
    public void PlayMusic( AudioClip clip, bool loop, bool interrupt, bool setToFullVolume = true )
    {
        if( !interrupt && _music.isPlaying ) return;
        _music.volume = setToFullVolume ? 1f : _music.volume;
        _music.Stop();
        _music.clip = clip;
        _music.loop = loop;
        _music.Play();
    }

    public void FadeOutMusic( float time, bool stopMusic )
    {
        StartCoroutine( FadeOut() );
        IEnumerator FadeOut()
        {
            for( var elapsed = 0f; elapsed < time; elapsed += Time.deltaTime )
            {
                _music.volume = Mathf.Lerp( 1f, 0f, elapsed / time );
                yield return null;
            }
            _music.Stop();
        }
    }

    public void StopMusic() => _music.Stop();

    Coroutine _toTargetMusicVolumeRoutine;
    public void ToTargetMusicVolume( float time, float volume )
    {
        if( _toTargetMusicVolumeRoutine != null )
            StopCoroutine( _toTargetMusicVolumeRoutine );
        _toTargetMusicVolumeRoutine = StartCoroutine( TargetVolume() );
        IEnumerator TargetVolume()
        {
            var currentVolume = _music.volume;
            for( var elapsed = 0f; elapsed < time; elapsed += Time.deltaTime )
            {
                _music.volume = Mathf.Lerp( currentVolume, volume, elapsed / time );
                yield return null;
            }

            _music.volume = volume;
        }
    }

    public AudioClip[] FootstepsSfx( int index )
    {
        return index switch
        {
            0 => grassFootsteps,
            1 => logFootsteps,
            2 => sandFootsteps,
            3 => stoneFootsteps,
            _ => grassFootsteps
        };
    }

    AudioSource GetSfxSource()
    {
        foreach( var source in _sfxSources )
        {
            if( source != null && !source.isPlaying ) return source;
        }

        return null;
    }

    public void SetAudioListenerVolume( float vol )
    {
        PlayerPrefs.SetFloat( PlayerPrefsMasterKey, vol );
        AudioListener.volume = vol;

        float compressorThreshold = -60;
        if( vol != 0 )
            compressorThreshold = 20.0f * Mathf.Log10( vol );
        compressorThreshold = Math.Max( -60, compressorThreshold );
        mixer.SetFloat( "CompressorThreshold", compressorThreshold );
    }

    public void SetMusicVolume( float vol )
    {
        PlayerPrefs.SetFloat( PlayerPrefsMusicKey, vol );

        float db = -80;
        if( vol != 0 )
            db = 20.0f * Mathf.Log10( vol );
        mixer.SetFloat( "MusicVolume", db );
    }

    public void SetSfxVolume( float vol )
    {
        PlayerPrefs.SetFloat( PlayerPrefsSfxKey, vol );

        float db = -80;
        if( vol != 0 )
            db = 20.0f * Mathf.Log10( vol );
        mixer.SetFloat( "SfxVolume", db );

        // float sfxCompressorThreshold = -60;
        // if( vol != 0 )
        //     sfxCompressorThreshold = 20.0f * Mathf.Log10( vol );
        // sfxCompressorThreshold = Math.Max( -60, sfxCompressorThreshold );
        // mixer.SetFloat( "SfxCompressorThreshold", sfxCompressorThreshold );
    }

    /* example usage
     
     * mixer.GetFloat( "MusicVolume", out var musicVol );
        mixer.GetFloat( "SfxVolume", out var sfxVol );
        musicVolSlider.value = AudioManager.DbToLinear( musicVol );
        sfxVolSlider.value = AudioManager.DbToLinear( sfxVol );
     */

    public static float DbToLinear( float dB ) => Mathf.Pow( 10.0f, dB / 20.0f );

    public void PlaySfx( AudioClip clip ) => PlaySfx( clip, 1f, 1f, 1f, 1f );
    public void PlaySfx( AudioClip clip, float vol ) => PlaySfx( clip, vol, vol, 1f, 1f );

    public void PlaySfx( AudioClip clip, float minVol, float maxVol, float minPitch, float maxPitch )
    {
        if( muteSfx ) return;
        var source = GetSfxSource();
        if( source == null ) return;
        source.volume = Random.Range( minVol, maxVol );
        source.pitch = Random.Range( minPitch, maxPitch );
        source.PlayOneShot( clip );
    }
}

public static class AudioHelper
{
    public static void PlaySfx( this AudioClip clip ) => AudioManager.Instance.PlaySfx( clip );
    public static void PlaySfx( this AudioClip clip, float vol ) => AudioManager.Instance.PlaySfx( clip, vol );

    public static void PlaySfx( this AudioClip clip, float minVol, float maxVol, float minPitch, float maxPitch ) =>
        AudioManager.Instance.PlaySfx( clip, minVol, maxVol, minPitch, maxPitch );

    public static void PlaySfx( this AudioClip clip, float vol, float pitchVar ) =>
        AudioManager.Instance.PlaySfx( clip, vol, vol, 1 - pitchVar, 1 + pitchVar );
}