using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [ SerializeField ] Vector3 eulerRot;

    void Update() => transform.Rotate( eulerRot * Time.deltaTime );
}
