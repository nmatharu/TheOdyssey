using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnder : MonoBehaviour
{
    Enemy _enemy;

    void Start()
    {
        _enemy = GetComponent<Enemy>();
        RenderSettings.ambientLight = Color.black;
    }

    void OnDestroy()
    {
        if( GameManager.Instance != null )
        {
            GameManager.Instance.Victory();
            GameManager.Instance.KillAllEnemies();
        }
    }
}
