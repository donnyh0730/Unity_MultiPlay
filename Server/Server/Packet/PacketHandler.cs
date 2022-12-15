using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using Server.GameContents;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        //아직은 잡큐시스템에의해서 메인쓰레드에서 처리되지 않는 부분이므로. 현재는 워커쓰레드에서 실행되는 코드이다
        //따라서 나중에는 잡큐를 메인쓰레드에서 하나씩 순차적으로 처리하는 기능으로 바꾸어야한다.
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;
        //TODO : 패킷이 올바른가? 검증해야하는 절차가 있지만 개인프로젝트이므로 생략한다.

        Console.WriteLine($"C_Move({movePacket.PosInfo.PosX},{movePacket.PosInfo.PosY})");

        //↓↓다른쓰레드에 의해서 강제적으로 지워질 수 있으므로 레퍼런스카운트를 의도적으로 올린다.
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;
        GameRoom room = player.Room;
        if (room == null)
            return;
        //일단 서버상에서의 좌표이동을 처리함.
        room.HandleMove(player, movePacket);
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

        room.HandleSkill(player, skillPacket);
    }
}
