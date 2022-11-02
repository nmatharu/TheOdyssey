using UnityEngine;
using UnityEngine.SceneManagement;

public class RToRestart : MonoBehaviour
{
    void Update()
    {
        if( Input.GetKeyDown( KeyCode.R ) )
        {
            if( Input.GetKey( KeyCode.LeftShift ) )
                SceneManager.LoadScene( SceneManager.GetActiveScene().name );
            else
                GameManager.Instance.RespawnAll();
        }
    }
}