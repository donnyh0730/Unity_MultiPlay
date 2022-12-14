using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using static Define;

public class MyPlayerController : PlayerController
{
    protected override void Init()
    {
        base.Init();
    }

    protected override void UpdateController()
    {
        switch (State)
        {
            case CreatureState.Idle:
                GetDirInput();
                break;
            case CreatureState.Moving:
                GetDirInput();
                break;
        }

        base.UpdateController();
    }

    protected override void UpdateIdle()
    {
        // 이동 상태로 갈지 확인
        if (Dir != MoveDir.None)
        {
            State = CreatureState.Moving;
            return;
        }

        // 스킬 상태로 갈지 확인
        if (Input.GetKey(KeyCode.Space))
        {
            State = CreatureState.Skill;
            //_coSkill = StartCoroutine("CoStartPunch");
            _coSkill = StartCoroutine("CoStartShootArrow");
        }
    }

    void LateUpdate()
    {
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    // 키보드 입력
    protected void GetDirInput()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Dir = MoveDir.Up;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Dir = MoveDir.Down;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Dir = MoveDir.Left;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Dir = MoveDir.Right;
        }
        else
        {
            Dir = MoveDir.None;
        }
    }

    //거의 도착했을 쯤 1회 불려지고 정수형 그리드좌표를 확정적으로 바꿔주는 함수이다. 
    protected override void MoveToNextPos()//따라서 이동패킷을 보내기에 가장 적잘타이밍이된다.
    {
        if (Dir == MoveDir.None)
        {
            State = CreatureState.Idle;
            CheckUpdatedSyncMoveStatus();
            return;
        }

        Vector3Int destPos = Vector3Int.zero;

        switch (Dir)
        {
            case MoveDir.Up:
                destPos = CellPos + Vector3Int.up;
                break;
            case MoveDir.Down:
                destPos = CellPos + Vector3Int.down;
                break;
            case MoveDir.Left:
                destPos = CellPos + Vector3Int.left;
                break;
            case MoveDir.Right:
                destPos = CellPos + Vector3Int.right;
                break;
        }

        if (Managers.Map.CanGo(destPos))
        {
            if (Managers.Object.Find(destPos) == null)
            {
                CellPos = destPos;
            }
        }
        CheckUpdatedSyncMoveStatus();
    }

    void CheckUpdatedSyncMoveStatus()
    {
        if (_updated)
        {
            C_Move movePacket = new C_Move();
            movePacket.PosInfo = PosInfo;
            Managers.Network.Send(movePacket);
            _updated = false;
        }
    }

}
