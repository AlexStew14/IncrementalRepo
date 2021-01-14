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

    public Dictionary<int, Stage> GetStageDictionary()
    {
        return gameData.stageDictionary;
    }

    public void SetStage(int key, Stage value)
    {
        if (gameData.stageDictionary.ContainsKey(key))
            gameData.stageDictionary[key] = value;
        else
            Debug.LogError("Stage Not Found");
    }

    public Stage GetStage(int key)
    {
        if (gameData.stageDictionary.ContainsKey(key))
            return gameData.stageDictionary[key];
        return null;
    }

    public void SetMapLevel(int key, MapLevel value)
    {
        if (gameData.mapLevelDictionary.ContainsKey(key))
            gameData.mapLevelDictionary[key] = value;
        else
            Debug.LogError("Stage Not Found");
    }

    public MapLevel GetMapLevel(int key)
    {
        if (gameData.mapLevelDictionary.ContainsKey(key))
            return gameData.mapLevelDictionary[key];
        return null;
    }

    public void AddMapLevel(int key, MapLevel value)
    {
        gameData.mapLevelDictionary.Add(key, value);
    }

    public void SetSkill(string key, Skill value)
    {
        if (gameData.skillDictionary.ContainsKey(key))
            gameData.skillDictionary[key] = value;
        else
            Debug.LogError("Skill Not Found");
    }

    public object GetOtherValue(string key)
    {
        if (gameData.OtherValues.ContainsKey(key))
            return gameData.OtherValues[key];
        return null;
    }

    public void SetOtherValue(string key, object value)
    {
        if (gameData.OtherValues.ContainsKey(key))
            gameData.OtherValues[key] = value;
        else
            Debug.LogError("Other Value Not Found");
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

    #endregion Get/Set Value Methods

    #region Seed Data Methods

    private void SeedGameData()
    {
        gameData = new GameData
        {
            OtherValues = SeedOtherValues(),
            playerData = SeedPlayerData(),
            skillDictionary = SeedSkills(),
            blockSpawnData = SeedBlockSpawnData(),
            stageDictionary = SeedStageDictionary(),
            mapLevelDictionary = SeedMapLevelDictionary()
        };
    }

    private PlayerData SeedPlayerData()
    {
        return new PlayerData
        {
            level = 1000.0f,
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
                {"MoneyMultiplier", 1.0f},
                {"CurrentStage", 0 },
                {"CurrentMapLevel", 0 },
                {"MaxKillCount", 10 },
                {"LevelPerStage", 10 }
            };
    }

    private Dictionary<int, Stage> SeedStageDictionary()
    {
        Dictionary<int, Stage> stageDictionary = new Dictionary<int, Stage>();

        stageDictionary.Add(0,
            new Stage
            {
                stageKey = 0,
                animatorName = "Backgrounds/Stage1/Animator",
                blockSpritesPath = "Blocks/Stage1",
            });

        stageDictionary.Add(10,
            new Stage
            {
                stageKey = 10,
                animatorName = "Backgrounds/Stage2/Animator",
                blockSpritesPath = "Blocks/Stage2",
            });

        return stageDictionary;
    }

    private Dictionary<int, MapLevel> SeedMapLevelDictionary()
    {
        Dictionary<int, MapLevel> mapLevelDictionary = new Dictionary<int, MapLevel>();

        mapLevelDictionary.Add(0,
            new MapLevel
            {
                completed = false,
                currentCount = 0,
                maxCount = 10,
                mapLevelKey = 0
            });

        return mapLevelDictionary;
    }

    private Dictionary<string, Skill> SeedSkills()
    {
        Dictionary<string, Skill> skillDictionary = new Dictionary<string, Skill>();

        Func<int, int> dmgCost = (x) => (int)(x * 1.3f) + 1;
        Func<float, float> dmgImprove = (x) => (int)(x * 1.2f) + 1;

        skillDictionary.Add("Damage1",
            new Skill
            {
                name = "Damage1",
                currentStatIncrease = 0,
                nextStatIncrease = 1,
                totalStatIncrease = 0,
                level = 1,
                maxLevel = 10000,
                type = SkillType.DMG,
                upgradeCost = 1,
                costFunction = dmgCost,
                improvementFunction = dmgImprove
            });

        skillDictionary.Add("Damage2",
            new Skill
            {
                name = "Damage2",
                currentStatIncrease = 0,
                nextStatIncrease = 100,
                totalStatIncrease = 0,
                level = 1,
                maxLevel = 10000,
                type = SkillType.DMG,
                upgradeCost = 100,
                costFunction = dmgCost,
                improvementFunction = (x) => ((int)(x * 1.2f) + 1) * 10
            });

        skillDictionary.Add("Damage3",
            new Skill
            {
                name = "Damage3",
                currentStatIncrease = 0,
                nextStatIncrease = 10000,
                totalStatIncrease = 0,
                level = 1,
                maxLevel = 10000,
                type = SkillType.DMG,
                upgradeCost = 1000,
                costFunction = dmgCost,
                improvementFunction = (x) => ((int)(x * 1.2f) + 1) * 100
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
                upgradeCost = 5,
                costFunction = (x) => (int)(x * 3.0f),
                improvementFunction = (x) => (x * 2)
            });

        skillDictionary.Add("SpawnSpeed",
            new Skill
            {
                name = "SpawnSpeed",
                currentStatIncrease = 0,
                nextStatIncrease = .3f,
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

        skillDictionary.Add("Helper1",
            new Skill
            {
                name = "Helper1",
                currentStatIncrease = 0,
                nextStatIncrease = .1f,
                totalStatIncrease = 0,
                level = 1,
                maxLevel = 50,
                type = SkillType.HELPER,
                upgradeCost = 25,
                costFunction = (x) => (int)(x * 1.3f),
                improvementFunction = (x) => x,
                helperData = new HelperData
                {
                    attackDamage = 1.0f,
                    attackSpeed = 1.0f,
                    idleTime = 3.0f,
                    movementSpeed = 1.0f,
                }
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

    public Dictionary<int, Stage> stageDictionary;

    public Dictionary<int, MapLevel> mapLevelDictionary;

    public PlayerData playerData;

    public BlockSpawnData blockSpawnData;
}