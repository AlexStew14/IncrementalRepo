using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataSavingManager : MonoBehaviour
{
    #region Fields

    // Path and file name used to load\save player configuration data
    private string saveFileName = "/GameData.xml";

    private string saveFilePath;

    private GameData gameData = new GameData();

    public bool NewGame = false;

    #endregion Fields

    #region Unity Methods

    // Start is called before the first frame update
    private void Awake()
    {
        saveFilePath = Application.persistentDataPath + saveFileName;

        InitializeGameData();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    #endregion Unity Methods

    #region Plagiarized Public Methods

    // These functions were borrowed and altered from Asteroid Escape (Matt's Code)

    public void InitializeGameData()
    {
        // Set defaults as baseline for any values not loaded from file
        this.SetGameData();

        // If player config has been saved previously, load it
        if (File.Exists(saveFilePath) && !NewGame)
            this.Load();
    }

    public void Save()
    {
        if ((this.saveFilePath != null) && (this.saveFilePath != string.Empty))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(this.saveFilePath);

            // Serialize data into file
            bf.Serialize(file, this.gameData);
            file.Close();
            Debug.Log("Game data saved");
        }
    }

    private void Load()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                // Load serialized data from file, right into a deserialized data object
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(saveFilePath, FileMode.Open);
                this.gameData = (GameData)bf.Deserialize(file);
                file.Close();

                Debug.Log("Game data loaded");
            }
            catch (Exception)
            {
                Debug.Log("Game data file is corrupt and could not be loaded");
            }
        }
        else
            Debug.Log("There is no save data!");
    }

    /// <summary>
    /// Wipes out local values, and deletes configuration file (drastic, and should only be done if file is corrupted)
    /// </summary>
    public void Delete()
    {
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            this.InitializeGameData();

            Debug.Log("Data reset complete!");
        }
        else
            Debug.Log("No save data to delete.");
    }

    public Skill GetSkill(string key)
    {
        return
            (
                (gameData.skillDictionary.ContainsKey(key))
                ? gameData.skillDictionary[key]
                : null
            );
    }

    public Dictionary<string, Skill> GetSkillDictionary()
    {
        return gameData.skillDictionary;
    }

    public void SetSkill(string key, Skill value)
    {
        if (gameData.skillDictionary.ContainsKey(key))
            gameData.skillDictionary[key] = value;
        else
            gameData.skillDictionary.Add(key, value);
    }

    public object GetOtherValue(string key)
    {
        return
            (
                (gameData.OtherValues.ContainsKey(key))
                ? gameData.OtherValues[key]
                : null
            );
    }

    public void SetOtherValue(string key, object value)
    {
        if (gameData.OtherValues.ContainsKey(key))
            gameData.OtherValues[key] = value;
        else
            gameData.OtherValues.Add(key, value);
    }

    public void SetGameData()
    {
        Dictionary<string, Skill> skillDictionary = new Dictionary<string, Skill>();

        skillDictionary.Add("Damage",
            new Skill
            {
                name = "Damage",
                currentValue = 1,
                level = 1,
                maxLevel = 100,
                oldValue = 0,
                type = SkillType.DMG,
                upgradeCost = 5,
                costFunction = (x) => x * 2,
                improvementFunction = (x) => ++x
            });

        this.gameData.skillDictionary = skillDictionary;

        Dictionary<string, object> otherValueDictionary = new Dictionary<string, object>()
                {
                     { "TotalBlocksSpawned", 1 },
                     {"Money", 0 }
                };

        this.gameData.OtherValues = otherValueDictionary;
    }

    #endregion Plagiarized Public Methods
}

/// <summary>
/// Serializable class used to store player settings in a text file
/// </summary>
[Serializable]
public class GameData
{
    public Dictionary<string, object> OtherValues;

    public Dictionary<string, Skill> skillDictionary;

    public GameData()
    {
        OtherValues = new Dictionary<string, object>();

        skillDictionary = new Dictionary<string, Skill>();
    }
}