using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEnder : MonoBehaviour
{
    void OnDestroy()
    {
        if( GameManager.Instance != null )
        {
            GameManager.Instance.Victory();
            GameManager.Instance.KillAllEnemies();
        }
    }
}
