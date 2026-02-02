using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;

public class DatabaseManager : SingletonBehaviour<DatabaseManager>
{
    private DatabaseReference dbManager;

    private void Start()
    {
        dbManager = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void CheckNicknameAndSave(string uid, string nickname, int level)
    {
        dbManager.Child("Nicknames").Child(nickname).
            GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.Result.Exists)
                {
                    Debug.Log($"Already exist nickname : {nickname}");
                }
                else
                {
                    SaveUserData(uid, nickname, level);
                }
            });
    }

    private void SaveUserData(string uid, string nickname, int level)
    {
        UserData newData = new UserData(nickname, level);
        string json = JsonUtility.ToJson(newData);

        dbManager.Child("Users").Child(uid).SetRawJsonValueAsync(json).ContinueWithOnMainThread(
            (task) =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log($"Save user {nickname} data sucessfully!!");
                }
                else
                {
                    Debug.Log($"Failed to save data, reason : {task.Exception}");
                }
            });

        dbManager.Child("Nicknames").Child(nickname).SetRawJsonValueAsync(JsonUtility.ToJson(new NicknameData(uid)));
    }
}
