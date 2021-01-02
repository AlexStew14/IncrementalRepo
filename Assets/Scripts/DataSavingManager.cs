using System;
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

    private GameData gameData;

    public int startingMoney;

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

    #region Save/Load/Delete File Methods

    private void InitializeGameData()
    {
        // If player config has been saved previously, load it
        if (File.Exists(saveFilePath))
        {
            Load();
        }
        else
        {
            SeedGameData();
        }
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
        try
        {
            // Load serialized data from file, right into a deserialized data object
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(saveFilePath, FileMode.Open);
            gameData = (GameData)bf.Deserialize(file);
            file.Close();

            Debug.Log("Game data loaded");
        }
        catch (Exception)
        {
            Debug.Log("Game data file is corrupt and could not be loaded");
            Delete();
        }
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

    #endregion Save/Load/Delete File Methods

    #region Get/Set Value Methods

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

    public PlayerData GetPlayerData()
    {
        return gameData.playerData;
    }

    public void SetPlayerData(PlayerData playerData)
    {
        gameData.playerData = playerData;
    }

    public BlockSpawnData GetBlockSpawnData()
    {
        return gameData.blockSpawnData;
    }

    public void SetBlockSpawnData(BlockSpawnData blockSpawnData)
    {
        gameData.blockSpawnData = blockSpawnData;
    }

    public HelperData GetHelperData(string key)
    {
        return
            (
                (gameData.HelperDictionary.ContainsKey(key))
                ? gameData.HelperDictionary[key]
                : null
            );
    }

    public void SetHelperData(string key, HelperData value)
    {
        if (gameData.HelperDictionary.ContainsKey(key))
            gameData.HelperDictionary[key] = value;
        else
            gameData.HelperDictionary.Add(key, value);
    }

    #endregion Get/Set Value Methods

    #region Seed Data Methods

    private void SeedGameData()
    {
        gameData = new GameData
        {
            OtherValues = SeedOtherValues(),
            playerData = SeedPlayerData(),
            skillDictionary = SeedSkills(),
            blockSpawnData = SeedBlockSpawnData()
        };
    }

    private PlayerData SeedPlayerData()
    {
        return new PlayerData
        {
            baseAttackSpeed = 1.0f,
            baseDamage = 1.0f,
            finalAttackSpeed = 1.0f,
            finalDamage = 1.0f,
            prestigeAtkSpeedMult = 1.0f,
            prestigeDmgMultiplier = 1.0f,
            runAtkSpeedMult = 1.0f,
            runDmgMultiplier = 1.0f,
            moveSpeed = 1.0f
        };
    }

    private Dictionary<string, HelperData> SeedHelperData()
    {
        Dictionary<string, HelperData> helperDictionary = new Dictionary<string, HelperData>();

        helperDictionary.Add("Helper1",
            new HelperData
            {
                baseAttackSpeed = 1.0f,
                baseDamage = 1.0f,
                finalAttackSpeed = 1.0f,
                finalDamage = 1.0f,
                prestigeAtkSpeedMult = 1.0f,
                prestigeDmgMultiplier = 1.0f,
                runAtkSpeedMult = 1.0f,
                runDmgMultiplier = 1.0f,
                moveSpeed = 1.0f
            });

        return helperDictionary;
    }

    private BlockSpawnData SeedBlockSpawnData()
    {
        return new BlockSpawnData
        {
            maxCurrentBlocks = 3,
            spawnTime = 3.0f
        };
    }

    private Dictionary<string, object> SeedOtherValues()
    {
        return new Dictionary<string, object>()
            {
                { "TotalBlocksSpawned", 1 },
                {"Money", startingMoney},
                {"MoneyMultiplier", 1.0f }
            };
    }

    private Dictionary<string, Skill> SeedSkills()
    {
        Dictionary<string, Skill> skillDictionary = new Dictionary<string, Skill>();

        skillDictionary.Add("Damage",
            new Skill
            {
                name = "Damage",
                currentStatIncrease = 0,
                nextStatIncrease = 1,
                totalStatIncrease = 0,
                level = 1,
                maxLevel = 100,
                type = SkillType.DMG,
                upgradeCost = 5,
                costFunction = (x) => x * 2,
                improvementFunction = (x) => ++x
            });

        skillDictionary.Add("AttackSpeed",
            new Skill
            {
                name = "AttackSpeed",
                currentStatIncrease = 0,
                nextStatIncrease = .05f,
                totalStatIncrease = 0,
                level = 1,
                maxLevel = 15,
                type = SkillType.ATTKSPEED,
                upgradeCost = 10,
                costFunction = (x) => (int)(x * 1.7f),
                improvementFunction = (x) => x
            });

        skillDictionary.Add("KillReward",
            new Skill
            {
                name = "KillReward",
                currentStatIncrease = 0,
                nextStatIncrease = 1,
                totalStatIncrease = 0,
                level = 1,
                maxLevel = 10,
                type = SkillType.KILLREWARD,
                upgradeCost = 15,
                costFunction = (x) => (int)(x * 1.5f),
                improvementFunction = (x) => x
            });

        skillDictionary.Add("SpawnSpeed",
            new Skill
            {
                name = "SpawnSpeed",
                currentStatIncrease = 0,
                nextStatIncrease = .1f,
                totalStatIncrease = 0,
                level = 1,
                maxLevel = 25,
                type = SkillType.SPAWNSPEED,
                upgradeCost = 20,
                costFunction = (x) => (int)(x * 1.5f),
                improvementFunction = (x) => x
            });

        skillDictionary.Add("MovementSpeed",
            new Skill
            {
                name = "MovementSpeed",
                currentStatIncrease = 0,
                nextStatIncrease = .15f,
                totalStatIncrease = 0,
                level = 1,
                maxLevel = 25,
                type = SkillType.MOVEMENTSPEED,
                upgradeCost = 20,
                costFunction = (x) => (int)(x * 1.5f),
                improvementFunction = (x) => x
            });

        return skillDictionary;
    }

    #endregion Seed Data Methods
}

/// <summary>
/// Serializable class used to store player settings in a text file
/// </summary>
[Serializable]
public class GameData
{
    public Dictionary<string, object> OtherValues;

    public Dictionary<string, Skill> skillDictionary;

    public Dictionary<string, HelperData> HelperDictionary;

    public PlayerData playerData;

    public BlockSpawnData blockSpawnData;
}