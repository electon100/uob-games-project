using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnVegetables : MonoBehaviour {
    public ARCupboard control;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnMouseDown()
    {
        control = GameObject.Find("ImageTarget").GetComponent<ARCupboard>();
        control.onVegetables();
    }
}
