using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [ SerializeField ] Toggle fullScreen;
    [ SerializeField ] Toggle smaaToggle;
    [ SerializeField ] Toggle trailerMode;
    [ SerializeField ] GameObject consoleToGui;

    [ SerializeField ] Slider masterVolume;
    [ SerializeField ] Slider musicVolume;
    [ SerializeField ] Slider sfxVolume;
    
    // public static SettingsManager Instance { get; private set; }

    void Start()
    {
        fullScreen.isOn = Screen.fullScreen;
        smaaToggle.isOn = GameManager.GameConfig.SMAA;
        trailerMode.isOn = GameManager.GameConfig.TrailerMode;
        
        this.Invoke( () =>
        {
            masterVolume.value = PlayerPrefs.GetFloat( AudioManager.PlayerPrefsMasterKey );
            musicVolume.value = PlayerPrefs.GetFloat( AudioManager.PlayerPrefsMusicKey );
            sfxVolume.value = PlayerPrefs.GetFloat( AudioManager.PlayerPrefsSfxKey );
        }, 0.5f );
        
        // if( Instance != null && Instance != this )
        // Destroy( this );
        // else
        // Instance = this;
    }

    void OnDisable()
    {
        PlayerPrefs.SetFloat( AudioManager.PlayerPrefsMasterKey, masterVolume.value );
        PlayerPrefs.SetFloat( AudioManager.PlayerPrefsMusicKey, musicVolume.value );
        PlayerPrefs.SetFloat( AudioManager.PlayerPrefsSfxKey, sfxVolume.value );
    }

    public void MasterVolChanged( float v ) => AudioManager.Instance.SetAudioListenerVolume( v );
    public void MusicVolChanged( float v ) => AudioManager.Instance.SetMusicVolume( v );
    public void SfxVolChanged( float v ) => AudioManager.Instance.SetSfxVolume( v );

    public void Fullscreen( bool f ) =>
        Screen.fullScreenMode = f ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
    public void SMAA( bool smaa ) => GameManager.GameConfig.SMAA = smaa;
    public void TrailerMode( bool tm ) => GameManager.GameConfig.TrailerMode = tm;

    public void DebugLog( bool dl )
    {
        if( dl )
            this.Invoke( () => Debug.Log( "Logging enabled." ), 0.5f );
        consoleToGui.SetActive( dl );
    }
}