using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerLocationState
{
    长尾鸟岛,
    十字星岛,
    热火山岛,
    无名花岛,
    海上
}

[System.Serializable]
public class PlayerStateData
{
    public PlayerLocationState currentIsland;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public List<TeamMemberData> teamMembers = new List<TeamMemberData>();
    public bool isOnRaft;
    public string currentRaftName;
    public int saveVersion = 1;
}

[System.Serializable]
public class TeamMemberData
{
    public string personName;
    public string profession;
    public bool isRecruited;
    public string status;
    
    public TeamMemberData(string name, string prof, bool recruited, string stat)
    {
        personName = name;
        profession = prof;
        isRecruited = recruited;
        status = stat;
    }
}
