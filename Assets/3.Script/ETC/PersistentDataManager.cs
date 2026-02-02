using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerDataJson
{
    public int MaxClearStage = 0;
}

public class PersistentDataManager : SingletonBehaviour<PersistentDataManager>
{
    private string dataPath;

    private string playerCharacterDataFileName = "playerCharacter.json";
    private PlayerDataJson playerData = new PlayerDataJson();
    public PlayerDataJson PlayerData => playerData;

    private void Start()
    {
        dataPath = Application.persistentDataPath;
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

        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            PlayerDataJson playerDataJson = JsonUtility.FromJson<PlayerDataJson>(jsonData);

            return playerDataJson;
        }

        return null;
    }
}
