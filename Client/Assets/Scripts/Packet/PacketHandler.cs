using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{
	public static void S_EnterGameHandler(PacketSession session, IMessage packet)
	{
		S_EnterGame enterGamePacket = packet as S_EnterGame;
        Managers.Object.AddPlayer(enterGamePacket.Playerinfo, bMyPlayer : true);

        Debug.Log("S_EnterGame");
        Debug.Log(enterGamePacket.Playerinfo);
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame LeaveGamePacket = packet as S_LeaveGame;
        Managers.Object.RemoveMyPlayer();

        Debug.Log("S_LeaveGameHandler");
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn SpawnPacket = packet as S_Spawn;
        foreach(PlayerInfo info in SpawnPacket.Playerinfos)
        {
            Managers.Object.AddPlayer(info, bMyPlayer: false);
        }
        Debug.Log("S_SpawnHandler");
        Debug.Log(SpawnPacket.Playerinfos);
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn DespawnPacket = packet as S_Despawn;
        foreach (int id in DespawnPacket.Playerids)
        {
            Managers.Object.Remove(id);
        }

        Debug.Log("S_DespawnHandler");
    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move MovePacket = packet as S_Move;
        ServerSession serverSession = session as ServerSession;

        Debug.Log("S_MoveHandler");
    }
}
