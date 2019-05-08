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
    private NewServer server;
    private string team;

    private void OnCollisionEnter(Collision collision)
    {
        wiiBlue = GameObject.Find("WiimoteManager").GetComponent<WiimoteBehaviourBlue>();
        wiiRed = GameObject.Find("WiimoteManager").GetComponent<WiimoteBehaviourRed>();
        server = GameObject.Find("Server").GetComponent<NewServer>();

        if(this.gameObject.name.Equals("BlueProjectile")){
            team = "blue";
        }
        else if (this.gameObject.name.Equals("RedProjectile")){
            team = "red";
        }

        Destroy(this.gameObject);
        //Debug.Log("GameObject Hit: " + collision.gameObject.name);
        stationHit = collision.gameObject.name;
        id = stationHit[stationHit.Length-1];
        switch (id)
        {
            case '0':
                // send 0
                SetResultText("You hit the enemy cupboard");
                // if(collision.gameObject.GetComponent<AudioSource>() != null){
                //     AudioSource source = collision.gameObject.GetComponent<AudioSource>();
                //     // source.PlayOneShot(source.clip, 1.0f);
                //     source.Play();
                // }
                server.OnStationHit(team, id+"");
                break;
            case '1':
                // send 1
                SetResultText("You hit the enemy chopping board");
                server.OnStationHit(team, id+"");
                break;
            case '2':
                // send 2
                SetResultText("You hit the enemy frying station");
                server.OnStationHit(team, id+"");
                break;
            case '3':
                 // send 3
                 SetResultText("You hit the enemy plating station");
                 server.OnStationHit(team, id+"");
                break;
            default:
                // send miss
                SetResultText("You missed the enemy stations");
                break;
        }
    }

    private void SetResultText(string objectHit){
        Debug.Log(team);
        if(team.Equals("blue")){
            wiiBlue.roundOver(objectHit);
        }
        else if (team.Equals("red")){
            wiiRed.roundOver(objectHit);
        }
    }
}
