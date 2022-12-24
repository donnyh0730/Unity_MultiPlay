using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [Serializable]
    public class SkillData
    {
        public int id;
        public string name;
        public float cooldown;
        public int damage;
        public SkillType skillType;
        public ProjectileInfo projectileInfo;
    }

    [Serializable]
    public class ProjectileInfo
    {
        public string name;
        public float speed;
        public int range;
        public string prefabpath;
    }

    [Serializable]
    public class SkillDataLoader : ILoader<int, SkillData>
    {
        //↓↓ JsonUtility.FromJson을 해주면 처음에 여기(stats) List형태로 들어와있음.
        public List<SkillData> skillInfos = new List<SkillData>();

        public Dictionary<int, SkillData> MakeDict()
        {
            Dictionary<int, SkillData> dict = new Dictionary<int, SkillData>();
            foreach (SkillData skill in skillInfos)
                dict.Add(skill.id, skill);
            return dict;
        }
    }
}