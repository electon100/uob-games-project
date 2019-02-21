using System;
using System.IO;
using System.Xml.Serialization;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEditor;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine.SceneManagement;

[Serializable]
public class Ingredient {

  public string Name { get; set; }

  public string Model { get; set; }

  public int numberOfPanFlips { get; set; }

  public int numberOfChops { get; set; }

  public Ingredient() {
		Name = "";
		Model = null;
		numberOfPanFlips = 0;
		numberOfChops = 0;
	}

	public Ingredient(string name, string model) {
		Name = name;
		Model = model;
		numberOfPanFlips = 0;
		numberOfChops = 0;
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

  public string ToString() {
    return Name.First().ToString().ToUpper() + Name.Replace('_', ' ').Substring(1);
  }

}
