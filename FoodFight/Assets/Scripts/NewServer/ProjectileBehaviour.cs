using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Networking;

public class ProjectileBehaviour : MonoBehaviour {

    public string stationHit = "";
    char id;
    private WiimoteBehaviourBlue wiiBlue;
    private WiimoteBehaviourRed wiiRed;


    private void OnCollisionEnter(Collision collision)
    {
        wiiBlue = GameObject.Find("WiimoteManager").GetComponent<WiimoteBehaviourBlue>();
        wiiRed = GameObject.Find("WiimoteManager").GetComponent<WiimoteBehaviourRed>();

        Debug.Log("GameObject Hit: " + collision.gameObject.name);
        stationHit = collision.gameObject.name;
        id = stationHit[stationHit.Length-1];
        switch (id)
        {
            case '0':
                // send 0
                SetResultText("You hit the enemy cupboard");      
                break;
            case '1':
                // send 1
                SetResultText("You hit the enemy chopping board");
                break;
            case '2':
                // send 2
                SetResultText("You hit the enemy frying station");
                break;
            case '3':
                 // send 3
                 SetResultText("You hit the enemy plating station");
                 break;
            default:
                // send miss
                SetResultText("You missed the enemy stations");
                break;
        }
        Destroy(this.gameObject);
    }

    private void SetResultText(String objectHit){
        if(this.gameObject.name.Equals("BlueProjectile(Clone)")){
            wiiBlue.roundOver(objectHit);
        }
        else if (this.gameObject.name.Equals("RedProjectile(Clone)")){
            wiiRed.roundOver(objectHit);
        }
    }
}
