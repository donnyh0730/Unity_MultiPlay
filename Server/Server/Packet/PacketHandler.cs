using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Text;

class PacketHandler
{
    public static void C_MoveHandler(PacketSession session, IMessage packet)
    {
        C_Move movePacket = packet as C_Move;
        ClientSession clientSession = session as ClientSession;

        Console.WriteLine($"C_Move({movePacket.PosInfo.PosX},{movePacket.PosInfo.PosY})");

        if (clientSession.MyPlayer == null)
            return;
        if (clientSession.MyPlayer.Room == null)
            return;

        //TODO :  패킷이 올바른가? 검증해야하는 절차가 있지만 개인프로젝트이므로 생략한다.

        //일단 서버상에서의 좌표이동을 처리함.
        clientSession.MyPlayer.Info.PosInfo = movePacket.PosInfo;

        //다른 플레이어들에게도 알린다.
        S_Move resMovePacket = new S_Move();
        resMovePacket.PlayerId = clientSession.MyPlayer.Info.PlayerId;
        resMovePacket.PosInfo = movePacket.PosInfo;

        clientSession.MyPlayer.Room.Broadcast(resMovePacket);
    }
}
