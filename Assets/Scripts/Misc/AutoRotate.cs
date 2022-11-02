using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AutoRotate : MonoBehaviour
{
    [ SerializeField ] Vector3 eulerRot;
    [ SerializeField ] float startDelayRandomRange;

    void Start()
    {
        if( startDelayRandomRange != 0f )
            this.Invoke( () => transform.rotation = Quaternion.identity, Random.value * startDelayRandomRange );
    }

    void Update() => transform.Rotate( eulerRot * Time.deltaTime );
}
