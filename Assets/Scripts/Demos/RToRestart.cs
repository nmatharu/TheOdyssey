using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RToRestart : MonoBehaviour
{
    void Update()
    {
        if( Input.GetKeyDown( KeyCode.R ) )
        {
            SceneManager.LoadScene( SceneManager.GetActiveScene().name );
        }
    }
}
