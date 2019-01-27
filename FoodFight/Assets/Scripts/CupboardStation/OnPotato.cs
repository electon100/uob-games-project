using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OnPotato : MonoBehaviour {
    public ARCupboard control;

    // Use this for initialization
    void Start()
    {
        control = GameObject.Find("ARCupboard").GetComponent<ARCupboard>();
    }

    // Update is called once per frame
    void Update () {

    }

    void OnMouseDown()
    {
        control.onPotato();
    }
}
