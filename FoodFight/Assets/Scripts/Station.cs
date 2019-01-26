using UnityEngine;
using System.Collections.Generic;

public class Station
{

    public string Team { get; set; }

    public string Tag { get; set; }

    public List<Ingredient> Ingredients { get; set; }

    public Station(string team, string tag, Ingredient ingredient)
    {
        Team = team;
        Tag = tag;
        Ingredients.Add(ingredient);
    }

}
