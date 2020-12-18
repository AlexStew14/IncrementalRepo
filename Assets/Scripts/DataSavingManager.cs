using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class DataSavingManager : MonoBehaviour
{
    /* Saving Example Usage

    class YourClass
    {
        private DataSavingManager dataSavingManager;

        private int tempSaveValue;

        void Start()
        {
            dataSavingManager = GameObject.FindGameObjectWithTag("DataSavingManager").GetComponent<DataSavingManager>();
        }

        void LoadMySettings()
        {
            // Data type returned is "object", so depending upon data type it may be necessary to cast
            tempSaveValue = dataSavingManager.Get("String to map the value to");
        }

        void SaveMySettings()
        {
            // Data being stored must be [SERIALIZABLE], all native types (string, int, float, etc.) are.
            dataSavingManager.Set("String to map the value to", tempSaveValue)

            // File should be saved after making changes
            dataSavingManager.Save();
        }
    }
 */

    #region Fields

    // Path and file name used to load\save player configuration data
    private string saveFileName = "/GameData.xml";
    private string saveFilePath;

    private GameData gameData = new GameData();

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    void Start()
    {
        saveFilePath = Application.persistentDataPath + saveFileName;

        InitializeGameData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #endregion


    #region Plagiarized Public Methods
    // These functions were borrowed and altered from Asteroid Escape (Matt's Code)

    public void InitializeGameData()
    {
        // Set defaults as baseline for any values not loaded from file
        this.SetGameData();

        // If player config has been saved previously, load it
        if (File.Exists(saveFilePath))
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

    public object GetSkill(string key)
    {
        return
            (
                (gameData.Skills.ContainsKey(key))
                ? gameData.Skills[key]
                : null
            );
    }

    public void SetSkill(string key, Skill value)
    {
        if (gameData.Skills.ContainsKey(key))
            gameData.Skills[key] = value;
        else
            gameData.Skills.Add(key, value);
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
        Dictionary<string, Skill> skillDictionary = new Dictionary<string, Skill>()
                {
                     { "Damage", new Skill() }
                };

        this.gameData.Skills = skillDictionary;

        Dictionary<string, object> otherValueDictionary = new Dictionary<string, object>()
                {
                     { "TotalBlocksSpawned", 1 }
                };

        this.gameData.Skills = skillDictionary;
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

    public Dictionary<string, Skill> Skills;


    public GameData()
    {
        OtherValues = new Dictionary<string, object>();

        Skills = new Dictionary<string, Skill>();
    }
}

