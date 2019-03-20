using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedPlayer {

  public int ConnectionId { get; }

	public GameObject PlayerPrefab { get; }

	public Station CurrentStation{ get; set; }

	public ConnectedPlayer(int connectionId, GameObject playerPrefab) {
		ConnectionId = connectionId;
		PlayerPrefab = playerPrefab;
		CurrentStation = null;
	}

	public override string ToString() {
		return "ConnectedPlayer [connectionId=" + ConnectionId + ", CurrentStation=" + CurrentStation.ToString() + "]";
	}

}