using UnityEngine;

public class HideInTrailerMode : MonoBehaviour
{
    void Start()
    {
        var config = GameManager.GameConfig;
        if( config != null )
            gameObject.SetActive( !config.TrailerMode );
    }
}