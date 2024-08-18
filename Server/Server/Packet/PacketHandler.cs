﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.DB;
using Server.GameContents;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PacketHandler
{
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        //아직은 잡큐시스템에의해서 메인쓰레드에서 처리되지 않는 부분이므로. 워커쓰레드에서 실행되는 코드
        
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;
        //TODO : 패킷이 올바른가? 검증해야하는 절차가 있지만 개인프로젝트이므로 생략한다.

        //Console.WriteLine($"C_Move({movePacket.PosInfo.PosX},{movePacket.PosInfo.PosY})");

        //↓↓다른쓰레드에 의해서 강제적으로 지워질 수 있으므로 레퍼런스카운트를 의도적으로 올린다.
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null)
            return;
        room.PushJob(room.HandleMove, player, movePacket);
        //▲▲JobSerializer가 순차실행을 보장하게된다. 
        //따라서 HandleMove내부에서 lock을 잡을 필요가 없어지므로 성능은 더 좋아진다.
    }

    public static void C_SkillHandler(PacketSession session, IMessage packet)
    {
        C_Skill skillPacket = packet as C_Skill;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null)
            return;

        room.PushJob(room.HandleSkill, player, skillPacket);
    }

	public static void C_LoginRequestHandler(PacketSession session, IMessage packet)
	{
		C_LoginRequest LoginRequest = packet as C_LoginRequest;
        
        ClientSession clientSession = session as ClientSession;
        Console.WriteLine($"UniqueId({LoginRequest.UniqueId})");
        //TODO 이런저런 보안체크 

        //
        using (AppDbContext db = new AppDbContext())
        {
            var Account = db.Accounts.Where(a => a.AccountName == LoginRequest.UniqueId).FirstOrDefault();

            if(Account != null)
            {
                S_LoginResult loginOk = new S_LoginResult() { LoginResult = 1 };
                clientSession.Send(loginOk);
            }
            else
            {
                AccountDb newAcc = new AccountDb() { AccountName = LoginRequest.UniqueId };
                db.Accounts.Add(newAcc);
                db.SaveChanges();

				S_LoginResult loginOk = new S_LoginResult() { LoginResult = 1 };
				clientSession.Send(loginOk);
			}
        }
	}
}
