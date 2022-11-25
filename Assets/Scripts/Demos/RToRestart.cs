using UnityEngine;
using UnityEngine.SceneManagement;

public class RToRestart : MonoBehaviour
{
    void Update()
    {
        if( Input.GetKeyDown( KeyCode.R ) )
        {
            if( Input.GetKey( KeyCode.LeftShift ) )
                GameManager.Instance.RespawnAll();
            else
                SceneManager.LoadScene( SceneManager.GetActiveScene().name );
        }
        
        if( Input.GetKey( KeyCode.LeftShift ) && Input.GetKeyDown( KeyCode.L ))
            WorldGenerator.Instance.BossStarted();
        
        if( Input.GetKey( KeyCode.LeftAlt ) && Input.GetKeyDown( KeyCode.L ))
            WorldGenerator.Instance.BossFinished();
    }
}