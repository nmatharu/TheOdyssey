using UnityEngine;

public class HideInTrailerMode : MonoBehaviour
{
    void Start()
    {
        var config = ConfigManager.Instance;
        if( config != null )
            gameObject.SetActive( !config.TrailerMode() );
    }
}