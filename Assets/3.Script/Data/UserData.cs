using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UserData
{
    public string nickname;
    public int level;

    public UserData(string nickname, int level)
    {
        this.nickname = nickname;
        this.level = level;
    }
}

[Serializable]
public class NicknameData
{
    public string uid;

    public NicknameData(string uid)
    {
        this.uid = uid;
    }
}
