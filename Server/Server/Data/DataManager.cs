﻿using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Server.Data
{
    public interface ILoader<Key, Value>
    {
        Dictionary<Key, Value> MakeDict();
    }

    public static class DataManager
    {
        public static Dictionary<int, StatInfo> StatDict { get; private set; } = new Dictionary<int, StatInfo>();
        public static Dictionary<int, SkillData> SkillDict { get; private set; } = new Dictionary<int, SkillData>();
        public static Dictionary<int, ItemData> ItemDict { get; private set; } = new Dictionary<int, ItemData>();

        public static void LoadData()
        {
            StatDict = LoadCommonJson<StatDataLoader, int, StatInfo>("S_StatData").MakeDict();
            SkillDict = LoadCommonJson<SkillDataLoader, int, SkillData>("S_SkillData").MakeDict();
            ItemDict = LoadCommonJson<ItemDataLoader, int, ItemData>("S_ItemData").MakeDict();
        }

		//이 함수는 Common/config.json 에 박혀있는 패스에서 데이터들을 읽어온다. 
		static Loader LoadCommonJson<Loader, Key, Value>(string fileName) where Loader : ILoader<Key, Value>
        {
            string text = File.ReadAllText($"{ConfigManager.Config.dataPath}/{fileName}.json");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(text);
        }
    }
}
