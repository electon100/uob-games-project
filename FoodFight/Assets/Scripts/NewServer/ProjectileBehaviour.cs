using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileBehaviour : MonoBehaviour {

    public string stationHit = "";
    char id;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("GameObject Hit: " + collision.gameObject.name);
        stationHit = collision.gameObject.name;
        id = stationHit[stationHit.Length-1];
        switch (id)
        {
            case '0':
                // send 0
                break;
            case '1':
                // send 1
                break;
            case '2':
                // send 2
                break;
            case '3':
                 // send 3
                 break;
            default:
                // send miss
                break;
        }
        Destroy(this.gameObject);
    }
}
