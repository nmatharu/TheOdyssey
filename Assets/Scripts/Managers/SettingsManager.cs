using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [ SerializeField ] Toggle fullScreen;
    [ SerializeField ] Toggle smaaToggle;
    [ SerializeField ] Toggle trailerMode;
    
    // public static SettingsManager Instance { get; private set; }

    void Start()
    {
        fullScreen.isOn = Screen.fullScreen;
        smaaToggle.isOn = GameManager.GameConfig.SMAA;
        trailerMode.isOn = GameManager.GameConfig.TrailerMode;

        // if( Instance != null && Instance != this )
        // Destroy( this );
        // else
        // Instance = this;
    }

    public void Fullscreen( bool f ) =>
        Screen.fullScreenMode = f ? FullScreenMode.ExclusiveFullScreen : FullScreenMode.Windowed;
    public void SMAA( bool smaa ) => GameManager.GameConfig.SMAA = smaa;
    public void TrailerMode( bool tm ) => GameManager.GameConfig.TrailerMode = tm;
}