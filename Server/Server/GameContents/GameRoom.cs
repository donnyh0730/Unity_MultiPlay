using Google.Protobuf;
using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        List<Player> _players = new List<Player>();

        public void EnterGame(Player newPlayer)
        {
            if (newPlayer == null)
                return;
            lock(_lock)
            {
                _players.Add(newPlayer);
                newPlayer.Room = this;

                //본인 클라이언트에도 접속되었음을 알림.
                {
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.Playerinfo = newPlayer.Info;
                    newPlayer.Session.Send(enterPacket);

                    S_Spawn spawnPacket = new S_Spawn();
                    //기존에 접속해서 스폰되어있던 플레이어들을 알아야 클라이언트에서 똑같이 스폰할 수 있기때문에,
                    foreach(Player p in _players)//내가 들어왔을때 기존에 있었던 플레이어리스트를 전송.
                    {
                        if (newPlayer != p)
                            spawnPacket.Playerinfos.Add(p.Info);
                    }

                    newPlayer.Session.Send(spawnPacket);
                }
                //타인한테도 들어왔다는 것을 알림.
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Playerinfos.Add(newPlayer.Info);

                    foreach(Player p in _players)
                    {
                        if(newPlayer != p)
                        {
                            p.Session.Send(spawnPacket);
                        }
                    }
                }
            }
            
        }

        public void LeaveGame(int playerId)
        {
            lock(_lock)
            {
                Player player = _players.Find(p => p.Info.PlayerId == playerId);
                if (player == null)
                    return;

                _players.Remove(player);
                player.Room = null;

                //본인에게 정보전송.
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
                //타인에게 정보 전송.
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.Playerids.Add(player.Info.PlayerId);
                    foreach(Player p in _players)
                    {
                        p.Session.Send(despawnPacket);
                    }
                }
            }
        }

        public void Broadcast(IMessage packet)
        {
            lock(_lock)
            {
                foreach(Player p in _players)
                {
                    p.Session.Send(packet);
                }
            }
        }
    }
}
