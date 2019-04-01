using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class Kitchen {

  private static readonly string[] stations = {"0", "1", "2", "3"};

  public List<Station> Stations { get; }

	public Kitchen() {
		Stations = new List<Station>();
    initialiseKitchen();
	}

	private void initialiseKitchen() {
		foreach(string stationId in stations) {
      Station newStation = new Station(stationId);
      Stations.Add(newStation);
    }
	}

  public void addStationToKitchen(string id) {
		Station stationToAdd = new Station(id);
    Stations.Add(stationToAdd);
	}

  public Station getStationForId(string id) {
    foreach(Station station in Stations) {
      string stationId = station.Id;
      if (stationId.Equals(id)) return station;
    }
    return null;
  }

  public static bool isValidStation(string station) {
    return stations.Contains(station);
  }

  public override string ToString() {
		string toReturn = "Kitchen [stations=";
		foreach(Station station in Stations) {
			toReturn += ", " + station.ToString();
		}
		toReturn += "]";
    return toReturn;
  }
}