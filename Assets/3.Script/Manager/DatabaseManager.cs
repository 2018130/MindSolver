using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using Firebase.Extensions;
using Firebase.Auth;

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

    public void SaveUserData(int stage)
    {
        FirebaseUser currentUser = FirebaseAuth.DefaultInstance.CurrentUser;

        if (currentUser != null)
        {
            string uid = currentUser.UserId;

            dbManager.Child("Users").Child(uid).Child("stage").SetValueAsync(stage).ContinueWithOnMainThread(
                (task) =>
                {
                    if (task.IsCompleted)
                    {
                        Debug.Log($"Save stage {stage} successfully for user UID: {uid}!!");
                    }
                    else
                    {
                        Debug.LogError($"Failed to save stage, reason : {task.Exception}");
                    }
                });
        }
        else
        {
            Debug.LogWarning("현재 파이어베이스에 로그인된 유저가 없습니다. 저장을 취소합니다.");
        }
    }

    public async void GetUserMaxStageDataInLocal(bool isDefault = false)
    {
        FirebaseUser currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
        Debug.Log($"Get user data from firebase");
        if(currentUser != null)
        {
            string uid = currentUser.UserId;

            await dbManager.Child("Users").Child(uid).Child("stage").GetValueAsync().ContinueWithOnMainThread((task) =>
            {
                if (task.IsCompleted && !isDefault)
                {
                    int maxStage = Convert.ToInt32(task.Result.Value);
                    Debug.Log($"Save stage {maxStage} successfully for user UID: {uid}!!");
                    PersistentDataManager.Singleton.PlayerData.MaxClearStage = maxStage;
                }
                else
                {
                    Debug.LogError($"Failed to save stage, reason : {task.Exception}");
                    PersistentDataManager.Singleton.PlayerData.MaxClearStage = 0;
                }
                _ = NetworkRelayManager.Singleton.QuickJoinGame(PersistentDataManager.Singleton.PlayerData.MaxClearStage);
            });
        }
        else
        {
            Debug.Log($"파이어베이스에 등록된 유저가 없음");
        }
    }
}
