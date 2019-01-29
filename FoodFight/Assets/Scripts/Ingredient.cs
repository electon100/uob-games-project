using System;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class Ingredient {

	public string Name { get; set; }

	public GameObject Model { get; set; }

	public int panTosses { get; set; }

	public bool isChopped { get; set; }

    public bool isCooked { get; set; }

	public bool isCookable { get; set; }

	public bool isChoppable { get; set; }

    /*
     * 0 - uncooked
     * 1 - cooked
     * 2 - unchopped
     * 3 - chopped
    */
    public Ingredient(string name, GameObject model) {
		Name = name;
		Model = model;
		panTosses = 0;
		isChopped = false;
		isCookable = true;
		isChoppable = true;
	}

    public string translateToString ()
    {
        string ingredient = this.Name + "^";
        if (!isCooked)
        {
            ingredient += "0^";
        }
        if (isCooked)
        {
            ingredient += "1^";
        }
        if (!isChopped)
        {
            ingredient += "2^";
        }
        if (isChopped)
        {
            ingredient += "3^";
        }

        return ingredient;
    }

    public void translateToIngredient(string ingredientAsString)
    {
        string[] ingredientValues = ingredientAsString.Split('^');
        this.Name = ingredientValues[0];
        for (int i = 1; i < ingredientValues.Length; i++)
        {
            switch (ingredientValues[i])
            {
                case "0":
                    this.isCooked = false;
                    break;
                case "1":
                    this.isCooked = true;
                    break;
                case "2":
                    this.isChopped = false;
                    break;
                case "3":
                    this.isChopped = true;
                    break;
            }
        }
       
    }

    public static string SerializeObject<Ingredient>(Ingredient toSerialize)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(toSerialize.GetType());

        using (StringWriter textWriter = new StringWriter())
        {
            xmlSerializer.Serialize(textWriter, toSerialize);
            return textWriter.ToString();
        }
    }

    public static Ingredient XmlDeserializeFromString<Ingredient>(string objectData, Type type)
    {
        var serializer = new XmlSerializer(type);
        Ingredient result;

        using (TextReader reader = new StringReader(objectData))
        {
            result = (Ingredient) serializer.Deserialize(reader);
        }

        return result;
    }
}
