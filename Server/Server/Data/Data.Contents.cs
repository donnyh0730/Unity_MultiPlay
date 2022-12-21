using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Data
{
    [Serializable]
    public class StatData
    {
        public int level;
        public int maxHp;
        public int attack;
        public int totalExp;
    }

    [Serializable]
    public class StatDataLoader : ILoader<int, StatData>
    {
        //↓↓ JsonUtility.FromJson을 해주면 처음에 여기(stats) List형태로 들어와있음.
        public List<StatData> statInfos = new List<StatData>();

        public Dictionary<int, StatData> MakeDict()
        {
            Dictionary<int, StatData> dict = new Dictionary<int, StatData>();
            foreach (StatData stat in statInfos)
                dict.Add(stat.level, stat);
            return dict;
        }
    }

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
        public float range;
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
