using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnNoodles : MonoBehaviour {
    public ARCupboard control;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update () {

    }

    void OnMouseDown()
    {
        control = GameObject.Find("ImageTarget").GetComponent<ARCupboard>();
        control.onNoodles();
    }
}
