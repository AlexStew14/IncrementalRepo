using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using System.Linq;

public class DataSavingManager : MonoBehaviour
{
    #region Fields

    // Path and file name used to load\save player configuration data
    private string saveFileName = "/GameData.xml";

    private string saveFilePath;

    private GameData gameData;

    [SerializeField]
    private double startingMoney;

    [SerializeField]
    private double startingPrestigeMoney;

    [SerializeField]
    private double startingPendingPrestigeMoney;

    [SerializeField]
    private int startingMapLevel;

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
            //Load();
            // TODO this is only for debug.
            Delete();
        }
        else
        {
            SeedGameData();
        }

        InvokeRepeating("SetTimeStamp", 15, 15);
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
            //Debug.Log("Game data saved");
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
            //Debug.Log("Game data loaded");
        }
        catch (Exception)
        {
            Debug.Log("Game data file is corrupt and could not be loaded");
            Delete();
        }
    }

    private void SetTimeStamp()
    {
        SetOtherValue("TimeStamp", DateTime.Now);
        EventManager.TriggerEvent("MiscStatsUpdate");
        Save();
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

    public Dictionary<string, Ability> GetAbilityDictionary()
    {
        return gameData.skillDictionary.Where(kvp => kvp.Value.type == SkillType.ABILITY)
            .Select(kvp => new KeyValuePair<string, Ability>(kvp.Key, kvp.Value as Ability))
            .ToDictionary(x => x.Key, x => x.Value);
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

    public void ClearMapLevelDictionary()
    {
        gameData.mapLevelDictionary.Clear();
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
            baseAttackSpeed = 1.0,
            baseDamage = 1.0,
            finalAttackSpeed = 1.0,
            finalDamage = 1.0,
            prestigeAtkSpeedMult = 1.0,
            prestigeDmgMultiplier = 1.0,
            runAtkSpeedMult = 1.0,
            runDmgMultiplier = 1.0,
            baseMoveSpeed = 2.0
        };
    }

    public void ResetPlayerNonPrestige()
    {
        PlayerData resetData = new PlayerData
        {
            baseAttackSpeed = 1.0,
            baseDamage = 1.0,
            finalAttackSpeed = 1.0,
            finalDamage = 1.0,
            prestigeAtkSpeedMult = gameData.playerData.prestigeAtkSpeedMult,
            prestigeDmgMultiplier = gameData.playerData.prestigeDmgMultiplier,
            runAtkSpeedMult = 1.0,
            runDmgMultiplier = 1.0,
            baseMoveSpeed = 2.0
        };

        gameData.playerData = resetData;
    }

    private BlockSpawnData SeedBlockSpawnData()
    {
        return new BlockSpawnData
        {
            maxCurrentBlocks = 4,
            spawnTime = 2f
        };
    }

    private Dictionary<string, object> SeedOtherValues()
    {
        return new Dictionary<string, object>()
            {
                { "TotalBlocksSpawned", 0 },
                {"TotalBlocksKilled", 0 },
                {"Money", startingMoney},
                {"MoneyMultiplier", 1.0},
                {"TotalMoneyEarned", 0.0 },
                {"PrestigeMoney",  startingPrestigeMoney},
                {"PendingPrestigeMoney", startingPendingPrestigeMoney },
                {"TotalPrestigeMoneyEarned", 0.0},
                {"PrestigeCount", 0 },
                {"CurrentStage", 0 },
                {"CurrentMapLevel", startingMapLevel },
                {"BlocksPerLevel", 10 },
                {"LevelPerStage", 10 },
                {"HighestLevelReached", 0 },
                {"FirstTimeStamp", DateTime.Now },
                {"TimeStamp", DateTime.Now },
                {"OfflineMultiplier", .1 },
                {"UnlockedAuto", false },
                {"AutoMoveSpeedMultiplier", .33 },
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

        stageDictionary.Add(20,
            new Stage
            {
                stageKey = 20,
                animatorName = "Backgrounds/Stage3/Animator",
                blockSpritesPath = "Blocks/Stage3",
            });

        stageDictionary.Add(30,
            new Stage
            {
                stageKey = 30,
                animatorName = "Backgrounds/Stage4/Animator",
                blockSpritesPath = "Blocks/Stage4",
            });

        stageDictionary.Add(40,
            new Stage
            {
                stageKey = 40,
                animatorName = "Backgrounds/Stage5/Animator",
                blockSpritesPath = "Blocks/Stage5",
            });

        stageDictionary.Add(50,
            new Stage
            {
                stageKey = 50,
                animatorName = "Backgrounds/Stage6/Animator",
                blockSpritesPath = "Blocks/Stage6",
            });

        stageDictionary.Add(60,
            new Stage
            {
                stageKey = 60,
                animatorName = "Backgrounds/Stage7/Animator",
                blockSpritesPath = "Blocks/Stage7",
            });

        stageDictionary.Add(70,
            new Stage
            {
                stageKey = 70,
                animatorName = "Backgrounds/Stage8/Animator",
                blockSpritesPath = "Blocks/Stage8",
            });

        stageDictionary.Add(80,
            new Stage
            {
                stageKey = 80,
                animatorName = "Backgrounds/Stage9/Animator",
                blockSpritesPath = "Blocks/Stage9",
            });

        stageDictionary.Add(90,
            new Stage
            {
                stageKey = 90,
                animatorName = "Backgrounds/Stage10/Animator",
                blockSpritesPath = "Blocks/Stage10",
            });

        return stageDictionary;
    }

    private Dictionary<int, MapLevel> SeedMapLevelDictionary()
    {
        Dictionary<int, MapLevel> mapLevelDictionary = new Dictionary<int, MapLevel>();

        mapLevelDictionary.Add(startingMapLevel,
            new MapLevel
            {
                completed = false,
                currentCount = 0,
                maxCount = 10,
                mapLevelKey = startingMapLevel
            });

        return mapLevelDictionary;
    }

    private Dictionary<string, Skill> SeedSkills()
    {
        Dictionary<string, Skill> skillDictionary = new Dictionary<string, Skill>();
        ResetNonPrestigeSkills(skillDictionary, false);

        // ******************** Prestige Skills ********************

        skillDictionary.Add("KillReward",
            new Skill
            {
                name = "KillReward",
                currentStatIncrease = 1,
                nextStatIncrease = 2,
                totalStatIncrease = 0,
                level = 0,
                maxLevel = 25,
                type = SkillType.KILLREWARD,
                upgradeCost = 5,
                costFunction = (x) => (int)(x * 3.0f),
                improvementFunction = (x) => (x * 2),
                milestoneLevel = 10000,
                milestoneMultipler = 1,
                isPrestige = true
            });

        skillDictionary.Add("BlocksPerWave",
            new Skill
            {
                name = "BlocksPerWave",
                currentStatIncrease = 0,
                nextStatIncrease = 1,
                totalStatIncrease = 0,
                level = 0,
                maxLevel = 9,
                type = SkillType.BLOCKSPERLEVEL,
                upgradeCost = 5,
                costFunction = (x) => (int)(x * 3.0f),
                improvementFunction = (x) => x,
                milestoneLevel = 10000,
                milestoneMultipler = 1,
                isPrestige = true
            });

        skillDictionary.Add("DamageMultiplier",
            new Skill
            {
                name = "DamageMultiplier",
                currentStatIncrease = 1,
                nextStatIncrease = 2,
                totalStatIncrease = 0,
                level = 0,
                maxLevel = 25,
                type = SkillType.DAMAGEMULTIPLIER,
                upgradeCost = 5,
                costFunction = (x) => (int)(x * 3.0f),
                improvementFunction = (x) => x * 2,
                milestoneLevel = 10000,
                milestoneMultipler = 1,
                isPrestige = true
            });

        skillDictionary.Add("AutoMoveSpeed",
            new Skill
            {
                name = "AutoMoveSpeed",
                currentStatIncrease = 1,
                nextStatIncrease = 1.1,
                totalStatIncrease = 0,
                level = 0,
                maxLevel = 25,
                type = SkillType.AUTOMOVESPEED,
                upgradeCost = 5,
                costFunction = (x) => (int)(x * 3.0f),
                improvementFunction = (x) => x,
                milestoneLevel = 10000,
                milestoneMultipler = 1,
                isPrestige = true
            });

        return skillDictionary;
    }

    public void ResetNonPrestigeSkills(Dictionary<string, Skill> skillDictionary, bool isPrestige)
    {
        if (isPrestige)
        {
            skillDictionary = gameData.skillDictionary;
            foreach (Skill s in skillDictionary.Values.ToList())
            {
                if (!s.isPrestige)
                    skillDictionary.Remove(s.name);
            }
        }

        Func<int, double> dmgCost = (x) => (Math.Pow(1.08, x) * 5);

        Func<int, double> attkSpdCost = (x) => Math.Pow(1.08, x) * 10;

        Func<int, double> spawnCost = (x) => (Math.Pow(1.08, x) * 25);

        Func<int, double> moveSpeedCost = (x) => (Math.Pow(1.08, x) * 25);

        Func<double, double> dmgImprove = x => x;
        int milestoneLevel = 25;
        float milestoneMultiplier = 2f;

        skillDictionary.Add("Damage1",
            new Skill
            {
                name = "Damage1",
                currentStatIncrease = 0,
                nextStatIncrease = 1,
                totalStatIncrease = 0,
                level = 0,
                maxLevel = 10000,
                type = SkillType.DMG,
                upgradeCost = 5,
                costFunction = dmgCost,
                improvementFunction = dmgImprove,
                milestoneLevel = milestoneLevel,
                milestoneMultipler = 1.25f
            });

        skillDictionary.Add("AttackSpeed",
            new Skill
            {
                name = "AttackSpeed",
                currentStatIncrease = 0,
                nextStatIncrease = .99,
                totalStatIncrease = 0,
                level = 0,
                maxLevel = 200,
                type = SkillType.ATTKSPEED,
                upgradeCost = 10,
                costFunction = attkSpdCost,
                improvementFunction = (x) => x,
                milestoneLevel = 10000,
                milestoneMultipler = 1
            });

        skillDictionary.Add("SpawnSpeed",
            new Skill
            {
                name = "SpawnSpeed",
                currentStatIncrease = 0,
                nextStatIncrease = .99,
                totalStatIncrease = 0,
                level = 1,
                maxLevel = 500,
                type = SkillType.SPAWNSPEED,
                upgradeCost = 25,
                costFunction = spawnCost,
                improvementFunction = (x) => x,
                milestoneLevel = 10000,
                milestoneMultipler = 1
            });

        skillDictionary.Add("MovementSpeed",
            new Skill
            {
                name = "MovementSpeed",
                currentStatIncrease = 0,
                nextStatIncrease = .1,
                totalStatIncrease = 0,
                level = 0,
                maxLevel = 100,
                type = SkillType.MOVEMENTSPEED,
                upgradeCost = 20,
                costFunction = moveSpeedCost,
                improvementFunction = (x) => x,
                milestoneLevel = 10000,
                milestoneMultipler = 1
            });

        Func<int, double> abilCost = (x) => (Math.Pow(1.08, x) * 25);
        Func<int, double> abilCost2 = (x) => (Math.Pow(1.08, x) * 100);
        Func<int, double> abilCost3 = (x) => (Math.Pow(1.08, x) * 250);
        Func<int, double> abilCost4 = (x) => (Math.Pow(1.08, x) * 500);

        // ******************** Abilities **********************
        skillDictionary.Add("Ability3",
            new Ability
            {
                name = "Ability3",
                type = SkillType.ABILITY,
                abilityType = AbilityType.PASSIVE,
                abilitySubType = AbilitySubType.MOVEMENTSPEED,
                currentStatIncrease = 0,
                nextStatIncrease = 1.5f,
                nextChanceIncrease = .05f,
                maxActivationChance = .25f,
                duration = 2,
                cooldown = 0,
                level = 0,
                maxLevel = 100,
                prefabIndex = 0,
                upgradeCost = 25,
                activationFunction = (x) => .025f,
                costFunction = abilCost3,
                improvementFunction = (x) => .1f
            });

        skillDictionary.Add("Ability1",
            new Ability
            {
                name = "Ability1",
                type = SkillType.ABILITY,
                abilityType = AbilityType.PASSIVE,
                abilitySubType = AbilitySubType.DAMAGE,
                currentStatIncrease = 0,
                nextStatIncrease = 2,
                nextChanceIncrease = .05f,
                maxActivationChance = .35f,
                duration = 0,
                cooldown = 0,
                level = 0,
                maxLevel = 100,
                prefabIndex = 1,
                upgradeCost = 100,
                activationFunction = (x) => .025f,
                costFunction = abilCost,
                improvementFunction = (x) => .2f
            });

        skillDictionary.Add("Ability4",
            new Ability
            {
                name = "Ability4",
                type = SkillType.ABILITY,
                abilityType = AbilityType.PASSIVE,
                abilitySubType = AbilitySubType.AREADAMAGE,
                currentStatIncrease = 0,
                nextStatIncrease = .5f,
                nextChanceIncrease = .05f,
                maxActivationChance = .35f,
                radius = 2f,
                duration = 0,
                cooldown = 0,
                level = 0,
                maxLevel = 100,
                prefabIndex = 2,
                upgradeCost = 250,
                activationFunction = (x) => .025f,
                costFunction = abilCost4,
                improvementFunction = (x) => .1f
            });

        skillDictionary.Add("Ability2",
            new Ability
            {
                name = "Ability2",
                type = SkillType.ABILITY,
                abilityType = AbilityType.PASSIVE,
                abilitySubType = AbilitySubType.DAMAGEOVERTIME,
                currentStatIncrease = 0,
                nextStatIncrease = .5f,
                nextChanceIncrease = .05f,
                maxActivationChance = .35f,
                duration = 3,
                cooldown = 0,
                level = 0,
                maxLevel = 100,
                prefabIndex = 3,
                upgradeCost = 250,
                activationFunction = (x) => .025f,
                costFunction = abilCost2,
                improvementFunction = (x) => .1f
            });
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