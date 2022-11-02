using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [ SerializeField ] float seconds;
    void Start() => Destroy( gameObject, seconds );
}
