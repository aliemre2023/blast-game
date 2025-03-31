using UnityEngine;
using System.IO;
using Newtonsoft.Json;

public class JsonService
{
    private readonly string filePath;

    // Default constructor (optional, but useful)
    public JsonService()
    {
        filePath = Application.dataPath + "/Assets/CaseStudyAssets2025/level_1.json"; // Default path
    }

    // Constructor to accept a custom path
    public JsonService(string path)
    {
        filePath = path;
    }

    public LevelData LoadLevelData()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"JSON file not found at: {filePath}");
            return null;
        }
        //Debug.Log(filePath);

        string jsonText = File.ReadAllText(filePath); 

        //Debug.Log(jsonText);

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError("JSON file is empty!");
            return null;
        }

        LevelData levelData = JsonConvert.DeserializeObject<LevelData>(jsonText);
        //Debug.Log(levelData);

        if (levelData == null)
        {
            Debug.LogError("Failed to parse JSON!");
        }
        return levelData;
    }
}
