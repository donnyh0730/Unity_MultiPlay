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
        Managers.Object.AddPlayer(enterGamePacket.Playerinfo, bMyPlayer: true);

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
        foreach (PlayerInfo info in SpawnPacket.Playerinfos)
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

        GameObject go = Managers.Object.FindById(MovePacket.PlayerId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc == null)
            return;

        cc.PosInfo = MovePacket.PosInfo;//여기서 좌표가 Set되는 순간에 이동을 시작한다.
        //키보드 입력방식도 방향키에 따라서 한칸 앞의 좌표를 Set하는 방식이었다. 
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill SkillPacket = packet as S_Skill;

        GameObject go = Managers.Object.FindById(SkillPacket.PlayerId);
        if (go == null)
            return;

        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.UseSkill(SkillPacket.Info.SkillId);
        }

    }
}
