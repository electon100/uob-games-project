using System;
using System.IO;
using System.Xml.Serialization;

[Serializable]
public class Ingredient {

	public string Name { get; set; }

	public string Model { get; set; }

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
    public Ingredient() {
		Name = "";
		Model = null;
		panTosses = 0;
		isChopped = false;
		isCookable = true;
		isChoppable = true;
	}

    public Ingredient(string name, string model)
    {
        Name = name;
        Model = model;
        panTosses = 0;
        isChopped = false;
        isCookable = true;
        isChoppable = true;
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
