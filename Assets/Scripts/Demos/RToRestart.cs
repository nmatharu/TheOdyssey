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
            // else
                // SceneManager.LoadScene( SceneManager.GetActiveScene().name );
        }
        
        // if( Input.GetKey( KeyCode.LeftShift ) && Input.GetKeyDown( KeyCode.L ))
            // WorldGenerator.Instance.BossStarted();
        
        // if( Input.GetKey( KeyCode.LeftAlt ) && Input.GetKeyDown( KeyCode.L ))
            // WorldGenerator.Instance.BossFinished();
        
        if( Input.GetKey( KeyCode.LeftShift ) && Input.GetKeyDown( KeyCode.G ))
            foreach( var p in GameManager.Instance.Players() )
                p.Init( "GERBERT" );
        
        // if( Input.GetKey( KeyCode.LeftShift ) && Input.GetKeyDown( KeyCode.D ))
        //     foreach( var p in GameManager.Instance.Players() )
        //         p.IncomingDamage( 2000f, 1 );

        // if( Input.GetKeyDown( KeyCode.Q ) )
        // {
        //     GameManager.Instance.Unpause();
        //     SceneManager.LoadScene( "Menu" );
        //     GlobalInputManager.Instance.ToMenu();
        // }
    }
}