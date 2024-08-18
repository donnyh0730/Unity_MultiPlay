using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = packet as S_EnterGame;
        Managers.Object.Add(enterGamePacket.ObjectInfo, bMyPlayer: true);

        Debug.Log("S_EnterGame");
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame LeaveGamePacket = packet as S_LeaveGame;
        Managers.Object.Clear();

        Debug.Log("S_LeaveGameHandler");
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn SpawnPacket = packet as S_Spawn;
        foreach (ObjectInfo info in SpawnPacket.ObjectInfos)
        {
            Managers.Object.Add(info, bMyPlayer: false);
        }
        Debug.Log("S_SpawnHandler");
        //Debug.Log(SpawnPacket.ObjectInfos);
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn DespawnPacket = packet as S_Despawn;
        foreach (int id in DespawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
        }

        Debug.Log("S_DespawnHandler");
    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move MovePacket = packet as S_Move;
        ServerSession serverSession = session as ServerSession;

        GameObject go = Managers.Object.FindById(MovePacket.ObjectId);
        if (go == null)
            return;

        //서버응답은 서버Tick주기가 끝나야만 응답이 오므로 자기플레이어 컨트롤에 대한 응답이 늦다.
        //따라서 MMORPG의경우에 자기 케릭터 클라이언트에서는 서버응답을 받아서 움직이게하지 않는다.
        if (Managers.Object.MyPlayer.Id == MovePacket.ObjectId)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        bc.PosInfo = MovePacket.PosInfo;//여기서 좌표가 Set되는 순간에 이동을 시작한다.
        //키보드 입력방식도 방향키에 따라서 한칸 앞의 좌표를 Set하는 방식이었다. 
    }

    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {
        S_Skill SkillPacket = packet as S_Skill;

        GameObject go = Managers.Object.FindById(SkillPacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc != null)
        {
            cc.UseSkill(SkillPacket.Info.SkillId);
        }
    }

    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp hpPacket = packet as S_ChangeHp;

        GameObject go = Managers.Object.FindById(hpPacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc != null)
        {
            //GameObject causer = Managers.Object.FindById(hpPakcet.causerId);
            //CreatureController causerCC = go.GetComponent<CreatureController>();
            //cc.OnDamaged(causerCC, hpPakcet.Hp);
            cc.Hp = hpPacket.Hp;
        }

    }


    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePacket = packet as S_Die;

        GameObject go = Managers.Object.FindById(diePacket.ObjectId);
        if (go == null)
            return;

        CreatureController cc = go.GetComponent<CreatureController>();
        if (cc != null)
        {
            cc.Hp = 0;
            cc.OnDead();
        }
    }

	public static void S_ConnectedHandler(PacketSession session, IMessage packet)
	{
        Debug.Log("S_ConnectedHandler");
        C_LoginRequest LoginRequest = new C_LoginRequest();

        LoginRequest.UniqueId = SystemInfo.deviceUniqueIdentifier;
        Managers.Network.Send(LoginRequest);

	}

	public static void S_LoginResultHandler(PacketSession session, IMessage packet)
	{
		S_LoginResult LoginResult = (S_LoginResult)packet;
        Debug.Log($"LoginResult : {LoginResult.LoginResult}");
	}
}
