using Data;
using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public static Dictionary<int, SkillData> SkillDict { get; private set; } = new Dictionary<int, SkillData>();
    public static Dictionary<int, ItemData> ItemDict { get; private set; } = new Dictionary<int, ItemData>();

    public void Init()
    {
        SkillDataLoader skillDataLoader = LoadClientJson<SkillDataLoader, int, SkillData>("C_SkillData");
		SkillDict = skillDataLoader.MakeDict();

		ItemDataLoader itemDataLoader = LoadClientJson<ItemDataLoader, int, ItemData>("C_ItemData");
		ItemDict = itemDataLoader.MakeDict();
	}

    //클라이언트 클라이언트에서 사용될 데이터들을 읽어온다. 
    T LoadClientJson<T, Key, Value>(string path) where T : ILoader<Key, Value>
    {
		TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return JsonUtility.FromJson<T>(textAsset.text);
	}
    
    //TODO : LoadCommonJson
}
