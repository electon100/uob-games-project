using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEggs : MonoBehaviour {
    public ARCupboard control;

    // Use this for initialization
    void Start()
    {
        control = GameObject.Find("ARCupboard").GetComponent<ARCupboard>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        control.onEggs();
    }
}
