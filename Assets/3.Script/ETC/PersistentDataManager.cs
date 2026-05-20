using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerDataJson
{
    public int MaxClearLevel = 0;
    public int MaxClearStage = 0;
}

public class PersistentDataManager : SingletonBehaviour<PersistentDataManager>
{
    private string dataPath;

    private string playerCharacterDataFileName = "playerCharacter.json";
    [SerializeField]
    private PlayerDataJson playerData = new PlayerDataJson();
    public PlayerDataJson PlayerData => playerData;

    public bool IsReplayed = false;

    public bool IsDefaultPlay = false;

    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        dataPath = Application.persistentDataPath;
        //playerData = LoadFromJson();
    }

    public void SaveToJson(PlayerDataJson playerCharacterDataJson)
    {
        string jsonData = JsonUtility.ToJson(playerCharacterDataJson, true);
        File.WriteAllText(Path.Combine(dataPath, playerCharacterDataFileName), jsonData);
        Debug.Log($"Save data to json path : {dataPath}");
    }

    public PlayerDataJson LoadFromJson()
    {
        string path = Path.Combine(dataPath, playerCharacterDataFileName);

        if (!File.Exists(path))
        { 
            SaveToJson(new PlayerDataJson() { MaxClearLevel = 0, MaxClearStage = 0 });
        }

        string jsonData = File.ReadAllText(path);
        PlayerDataJson playerDataJson = JsonUtility.FromJson<PlayerDataJson>(jsonData);

        return playerDataJson;
    }
}
