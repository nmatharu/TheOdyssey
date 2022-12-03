using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvertParentRot : MonoBehaviour
{
    Transform _parent;
    
    void Start() => _parent = transform.parent;

    void Update()
    {
        transform.rotation = Quaternion.Inverse( _parent.rotation );
    }
}
