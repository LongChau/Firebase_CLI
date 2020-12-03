using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public class Clan
{
    #region Fields
    public string id;
    public string name;
    public string decs;
    public string tag;
    public int rank;
    public string logoPath;

    public string leaderID;
    public string coLeaderID;

    [DictionaryDrawerSettings]
    public Dictionary<string, string> memberIDs;
    
    public int requestCardResetTime;    // once per 12h
    public int requestCardLimitTime;   // Can have 1 request at  time.
    #endregion

    #region Property
    public string Id { get => id; set => id = value; }
    public string Name { get => name; set => name = value; }
    public string Decs { get => decs; set => decs = value; }
    public string Tag { get => tag; set => tag = value; }
    public int Rank { get => rank; set => rank = value; }
    public string LogoPath { get => logoPath; set => logoPath = value; }
    public string LeaderID { get => leaderID; set => leaderID = value; }
    public string CoLeaderID { get => coLeaderID; set => coLeaderID = value; }
    public Dictionary<string, string> DictMemberIDs { get => memberIDs; set => memberIDs = value; }
    public int RequestCardResetTime { get => requestCardResetTime; set => requestCardResetTime = value; }
    public int RequestCardLimitTime { get => requestCardLimitTime; set => requestCardLimitTime = value; }
    #endregion

    #region Constructor
    public Clan()
    {
        this.id = "";
        this.name = "";
        this.decs = "";
        this.tag = "";
        this.rank = 0;
        this.logoPath = "";
        this.leaderID = "";
        this.coLeaderID = "";
        this.memberIDs = null;
        this.requestCardResetTime = 0;
        this.requestCardLimitTime = 0;
    }

    public Clan(string id, string name, string decs, string tag, int rank, string logoPath, string leaderID, string coLeaderID, Dictionary<string, string> dictMemberIDs, int requestCardResetTime, int requestCardLimitTime)
    {
        this.id = id;
        this.name = name;
        this.decs = decs;
        this.tag = tag;
        this.rank = rank;
        this.logoPath = logoPath;
        this.leaderID = leaderID;
        this.coLeaderID = coLeaderID;
        this.memberIDs = dictMemberIDs;
        this.requestCardResetTime = requestCardResetTime;
        this.requestCardLimitTime = requestCardLimitTime;
    }
    #endregion

    /// <summary>
    /// Convert this data to dictionary.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, object> ToDictionary()
    {
        Dictionary<string, object> result = new Dictionary<string, object>();
        result["id"] = id;
        result["name"] = name;
        result["decs"] = decs;
        result["tag"] = tag;
        result["rank"] = rank;
        result["logoPath"] = logoPath;
        result["leaderID"] = leaderID;
        result["coLeaderID"] = coLeaderID;
        result["memberIDs"] = memberIDs;
        result["requestCardResetTime"] = requestCardResetTime;
        result["requestCardLimitTime"] = requestCardLimitTime;

        return result;
    }
}
