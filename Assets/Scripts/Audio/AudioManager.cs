using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    const string PlayerPrefsMasterKey = "masterVol";
    const string PlayerPrefsMusicKey = "musicVol";
    const string PlayerPrefsSfxKey = "sfxVol";
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
    [ SerializeField ] public AudioClip lobbyCountdown;
    [ SerializeField ] public AudioClip[] lobbyFootsteps;
    
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
    [ SerializeField ] public AudioClip[] spawnPillars;

    [ SerializeField ] public AudioClip[] splatterSfx;
    [ SerializeField ] public AudioClip[] precision;
    [ SerializeField ] public AudioClip precisionReady;
    [ SerializeField ] public AudioClip bleedTick;
    [ SerializeField ] public AudioClip cashback;
    [ SerializeField ] public AudioClip guardian;
    [ SerializeField ] public AudioClip safeguardDown;
    [ SerializeField ] public AudioClip[] chronos;
    
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

        // menuMusicLoop.loop = true;
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