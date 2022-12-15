using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
{
	public MyPlayerController MyPlayer { get; set; }
	Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();
	
	public void AddPlayer(PlayerInfo info, bool bMyPlayer = false)
    {
		if(bMyPlayer)//내 플레이어를 소환해야 하는 경우.
        {
			GameObject go = Managers.Resource.Instantiate("Creature/MyPlayer");
			go.name = info.Name;
			_objects.Add(info.PlayerId, go);//PlayerId는 서버가 발급하게 되어있다.

			MyPlayer = go.GetComponent<MyPlayerController>();
			MyPlayer.Id = info.PlayerId;
			MyPlayer.PosInfo = info.PosInfo;
			MyPlayer.SyncGridPosToWorldPos();
        }
        else//다른 유저플레이어를 소환해야하는 경우.
        {
            GameObject go = Managers.Resource.Instantiate("Creature/Player");
            go.name = info.Name;
            _objects.Add(info.PlayerId, go);

			PlayerController pc = go.GetComponent<PlayerController>();
			pc.Id = info.PlayerId;
			pc.PosInfo = info.PosInfo;
			pc.SyncGridPosToWorldPos();
		}
    }

	public void Add(int id, GameObject go)
	{
		_objects.Add(id,go);
	}

	public void Remove(int id)
	{
        GameObject go = _objects[id];
        if (go == null)
            return;

        Managers.Resource.Destroy(go);
        _objects.Remove(id);
	}

	public void RemoveMyPlayer()
    {
		if (MyPlayer == null)
			return;

		Remove(MyPlayer.Id);
		MyPlayer = null;
    }

	public GameObject FindById(int id)
    {
		GameObject go = null;
		_objects.TryGetValue(id, out go);
		return go;
    }

	public GameObject Find(Vector3Int cellPos)
	{
		foreach (GameObject obj in _objects.Values)
		{
			CreatureController cc = obj.GetComponent<CreatureController>();
			if (cc == null)
				continue;

			if (cc.CellPos == cellPos)
				return obj;
		}

		return null;
	}

	public GameObject Find(Func<GameObject, bool> condition)
	{
		foreach (GameObject obj in _objects.Values)
		{
			if (condition.Invoke(obj))
				return obj;
		}

		return null;
	}

	public void Clear()
	{
		foreach (GameObject obj in _objects.Values)
		{
			Managers.Resource.Destroy(obj);
		}
		_objects.Clear();
	}
}
