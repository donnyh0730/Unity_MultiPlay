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
                    //내가 들어왔을때 기존에 있었던 플레이어리스트를 나에게 전송.
                    foreach (Player p in _players)
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

        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;
            //▼아래 로직은 공유데이터를 많이 접근하는 코드이므로 lock이 필요하다.
            //따라서 락을 소유하는 room에서 처리하는 것이 안전하다.
            lock (_lock)
            {  
                player.Info.PosInfo = movePacket.PosInfo;

                S_Move resMovePacket = new S_Move();
                resMovePacket.PlayerId = player.Info.PlayerId;
                resMovePacket.PosInfo = movePacket.PosInfo;

                Broadcast(resMovePacket);
            }
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
        {
            if (player == null)
                return;
            lock (_lock)
            {
                PlayerInfo info = player.Info;
                if (info.PosInfo.State != CreatureState.Idle)
                    return;

                //TODO: 스킬 사용 가능 여부 체크.
                info.PosInfo.State = CreatureState.Skill;
                S_Skill ServerSkillPacket = new S_Skill() { Info = new SkillInfo() };

                ServerSkillPacket.PlayerId = info.PlayerId;
                ServerSkillPacket.Info.SkillId = skillPacket.Info.SkillId;
                Broadcast(ServerSkillPacket);
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
