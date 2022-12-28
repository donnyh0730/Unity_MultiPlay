﻿using Google.Protobuf.Protocol;
using Server.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents
{
    public class Monster : GameObject
    {
        Vector2Int _destCellPos;
        int _skillRange = 1; //TODO 패킷에 필드 추가.
        bool _isRange = false; //TODO 패킷에 필드 추가.
        public Monster()
        {
            ObjectType = GameObjectType.Monster;
            //Temp
            Stat.Level = 1;
            Stat.Hp = 100;
            Stat.MaxHp = 100;
            Stat.Speed = 5.0f;

            State = CreatureState.Idle;
        }

        public override void Update()
        {
            switch(State)
            {
                case CreatureState.Idle:
                    UpdateIdle();
                    break;
                case CreatureState.Moving:
                    UpdateMoving();
                    break;
                case CreatureState.Skill:
                    UpdateSkill();
                    break;
                case CreatureState.Dead:
                    UpdateDead();
                    break;
            }
        }
        Player _target;
        int _SearchCellDist = 8;
        long _nextSearchTick = 0;
        protected virtual void UpdateIdle()
        {
            if (_nextSearchTick > Environment.TickCount64)
                return;
            _nextSearchTick = Environment.TickCount64 + 1000;

            Player target = Room.FindPlayer(p =>
            {
                int CellDist = Vector2Int.GetCellDist(p.CellPos, CellPos);
                return CellDist <= _SearchCellDist;
            });

            if (target == null)
                return;

            _target = target;
            State = CreatureState.Moving;
        }

        long _nextMoveTick = 0;
        int _chaseCellDist = 15;
        protected virtual void UpdateMoving()
        {
            if (_nextMoveTick > Environment.TickCount64)
                return;
            long moveTick = (long)(1000 / Speed);
            _nextMoveTick = Environment.TickCount64 + moveTick;

            if(_target == null || _target.Room == null)
            {
                Terminate();
                return;
            }
            int CellDist = Vector2Int.GetCellDist(_target.CellPos, CellPos);
            if(CellDist == 0 || CellDist > _chaseCellDist)
            {
                Terminate();
                return;
            }
            List<Vector2Int> path = Room.Map.FindPath(CellPos, _target.CellPos, false);
            if (path.Count < 2 || path.Count > _chaseCellDist)
            {
                Terminate();
                return;
            }
            
            if (SkillRangeCheck())
            {
                _coolTime = 0;
                State = CreatureState.Skill;
                return;
            }

            //path[1] A*서치 의 첫번째인자는 현재위치에서 바로 1칸 떨어져있다.
            Dir = GetDirFromVec(path[1] - CellPos);
            Room.Map.ApplyMove(this, path[1]);

            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(movePacket);

            void Terminate()
            {
                _target = null;
                State = CreatureState.Idle;
                BroadcastMove();
            }
        }

        long _coolTime = 0;
        protected virtual void UpdateSkill()
        {
            if(_coolTime == 0)
            {
                //유효한 타겟인지
                if (_target == null || _target.Room == null || _target.Hp == 0)
                {
                    _target = null;
                    State = CreatureState.Moving;
                    BroadcastMove();
                    return;
                }
                //스킬이 현재 사용 가능한지
                if (!SkillRangeCheck())
                {
                    State = CreatureState.Moving;
                    BroadcastMove();
                }
                //타게팅 방향 주시하도록 Dir설정.
                Vector2Int dir = _target.CellPos - CellPos;
                MoveDir LookDir = GetDirFromVec(dir);
                if(Dir != LookDir)
                {
                    Dir = LookDir;
                    BroadcastMove();
                }
                //데미지 적용
                SkillData skilldata = null;
                DataManager.SkillDict.TryGetValue(1, out skilldata);
                  _target.OnDamaged(this, skilldata.damage + Stat.Attack);
                //스킬 사용 Broadcast
                S_Skill skillPacket = new S_Skill() { Info = new SkillInfo() };
                skillPacket.ObjectId = Id;
                skillPacket.Info.SkillId = skilldata.id;
                Room.Broadcast(skillPacket);

                //스킬 쿨타임 적용
                int coolTick = (int)(1000 * skilldata.cooldown);
                _coolTime = Environment.TickCount64 + coolTick;
            }
            if (_coolTime > Environment.TickCount64)
                return;
            _coolTime = 0;
        }

        protected virtual void UpdateDead()
        {

        }

        protected virtual bool SkillRangeCheck()
        {
            if (_target != null)
            {
                Vector2Int dir = _target.CellPos - CellPos;
                //사정거리 안에 있고, 일직선 상에 있으면
                if (dir.Magnitude <= _skillRange && (dir.x == 0 || dir.y == 0))
                {
                    //Dir = GetDirFromVec(dir);
                    //State = CreatureState.Skill;
                    return true;
                }
            }
            return false;
        }

        protected void BroadcastMove()
        {
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosInfo = PosInfo;
            Room.Broadcast(movePacket);
        }
    }
}
