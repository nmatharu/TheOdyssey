using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCanvasAtScreen : MonoBehaviour
{
    Transform _cameraTransform;
    RectTransform _transform;
    
    void Start()
    {
        _cameraTransform = Camera.main.transform;
        _transform = GetComponent<RectTransform>();
    }

    void Update()
    {
        _transform.rotation = _cameraTransform.rotation;
    }
}
