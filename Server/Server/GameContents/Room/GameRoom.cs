﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents
{
    public class GameRoom
    {
        object _lock = new object();
        public int RoomId { get; set; }

        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, Monster> _monsters = new Dictionary<int, Monster>();
        Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>();

        public Map Map { get; private set; } = new Map();
            

        public void Init(int mapId)
        {
            Map.LoadMap(mapId, "../../../../../Common/MapData");
        }

        public void Update()
        {
            lock(_lock)
            {
                foreach(Projectile projectile in _projectiles.Values)
                {
                    projectile.Update();
                }
            }
        }

        public void EnterGame(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {
                    Player player = gameObject as Player;
                    _players.Add(gameObject.Id, player);
                    player.Room = this;
                    Map.ApplyMove(player, new Vector2Int(player.PosInfo.PosX, player.PosInfo.PosY));
                    //본인 클라이언트에도 접속되었음을 알림.
                    {
                        S_EnterGame enterPacket = new S_EnterGame();
                        enterPacket.ObjectInfo = player.Info;
                        player.Session.Send(enterPacket);

                        S_Spawn spawnPacket = new S_Spawn();
                        //기존에 접속해서 스폰되어있던 플레이어들을 알아야 클라이언트에서 똑같이 스폰할 수 있기때문에,
                        //내가 들어왔을때 기존에 있었던 플레이어리스트를 나에게 전송.
                        foreach (Player p in _players.Values)
                        {
                            if (gameObject != p)
                                spawnPacket.ObjectInfos.Add(p.Info);
                        }

                        player.Session.Send(spawnPacket);
                    }
                }
                else if (type == GameObjectType.Monster)
                {
                    Monster monster = gameObject as Monster;
                    _monsters.Add(gameObject.Id, monster);
                    monster.Room = this;
                }
                else if (type == GameObjectType.Projectile)
                {
                    Projectile projectile = gameObject as Projectile;
                    _projectiles.Add(gameObject.Id, projectile);
                    projectile.Room = this;
                }

                //타인한테도 들어왔다는 것을 알림.
                {
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.ObjectInfos.Add(gameObject.Info);

                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != gameObject.Id)
                        {
                            p.Session.Send(spawnPacket);
                        }
                    }
                }
            }

        }

        public void LeaveGame(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            lock (_lock)
            {
                if (type == GameObjectType.Player)
                {
                    Player player;
                    if (_players.Remove(objectId, out player) == false)
                        return;
                    player.Room = null;
                    Map.ApplyLeave(player);
                    //본인에게 정보전송.
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                    
                }
                else if (type == GameObjectType.Monster)
                {
                    Monster monster = null;
                    if (_monsters.Remove(objectId, out monster) == false)
                        return;

                    monster.Room = null;
                    Map.ApplyLeave(monster);
                }
                else if (type == GameObjectType.Projectile)
                {
                    Projectile projectile = null;
                    if (_projectiles.Remove(objectId, out projectile) == false)
                        return;

                    projectile.Room = null;
                }
                //타인에게 정보 전송.
                {
                    S_Despawn despawnPacket = new S_Despawn();
                    despawnPacket.ObjectIds.Add(objectId);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != objectId)
                        {
                            p.Session.Send(despawnPacket);
                        }
                    }
                }
            }
        }

        public void HandleMove(Player player, C_Move movePacket)
        {
            //TODO : 이동 할 수 있는 위치인지 판정.
            if (player == null)
                return;
            //▼아래 로직은 공유데이터를 많이 접근하는 코드이므로 lock이 필요하다.
            //따라서 락을 소유하는 room에서 처리하는 것이 안전하다.
            lock (_lock)
            {
                PositionInfo movePosInfo = movePacket.PosInfo;

                ObjectInfo info = player.Info;

                //현재좌표와 다른 좌표로 가고싶다는 이동 패킷이 왔을때 갈 수 있는 위치인지 확인
                if (movePosInfo.PosX != info.PosInfo.PosX || movePosInfo.PosY != info.PosInfo.PosY)
                {
                    if (Map.CanGo(new Vector2Int(movePosInfo.PosX, movePosInfo.PosY)) == false)
                        return;
                }
                info.PosInfo.State = movePacket.PosInfo.State;
                info.PosInfo.MoveDir = movePacket.PosInfo.MoveDir;
                Map.ApplyMove(player, new Vector2Int(movePosInfo.PosX, movePosInfo.PosY));

                S_Move resMovePacket = new S_Move();
                resMovePacket.ObjectId = player.Info.ObjectId;
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
                ObjectInfo info = player.Info;
                if (info.PosInfo.State != CreatureState.Idle)
                    return;

                //TODO: 스킬 사용 가능 여부 체크.
                info.PosInfo.State = CreatureState.Skill;
                S_Skill ServerSkillPacket = new S_Skill() { Info = new SkillInfo() };

                ServerSkillPacket.ObjectId = info.ObjectId;
                ServerSkillPacket.Info.SkillId = skillPacket.Info.SkillId;
                Broadcast(ServerSkillPacket);

                SkillData skillData = null;
                if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
                    return;

                switch(skillData.skillType)
                {
                    case SkillType.SkillAuto:
                        {
                            //TODO 대미지 판정.
                            Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
                            GameObject target = Map.Find(skillPos);
                            if (target != null)
                            {
                                Console.WriteLine("Hit Player !");
                            }
                        }
                        break;
                    case SkillType.SkillProjectile:
                        {
                            //TODO : Arrow
                            Arrow arrow = ObjectManager.Instance.Add<Arrow>();
                            if (arrow == null)
                                return;
                            arrow.Owner = player;
                            arrow.skillData = skillData;
                            arrow.PosInfo.State = CreatureState.Moving;
                            arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
                            arrow.PosInfo.PosX = player.PosInfo.PosX;
                            arrow.PosInfo.PosY = player.PosInfo.PosY;
                            arrow.Speed = skillData.projectileInfo.speed;
                            EnterGame(arrow);
                        }
                        break;
                }

            }
        }

        public void Broadcast(IMessage packet)
        {
            lock (_lock)
            {
                foreach (Player p in _players.Values)
                {
                    p.Session.Send(packet);
                }
            }
        }
    }
}