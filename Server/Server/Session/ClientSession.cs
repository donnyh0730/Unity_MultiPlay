using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ServerCore;
using System.Net;
using Google.Protobuf.Protocol;
using Google.Protobuf;
using Server.GameContents;
using Server.Data;

namespace Server
{
	public class ClientSession : PacketSession
	{
		public Player MyPlayer { get; set; }
		public int SessionId { get; set; }

		public void Send(IMessage packet)
        {
			string MessageName = packet.Descriptor.Name.Replace("_", string.Empty);//"SChat" 이런식으로 나옴.
			MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), MessageName);//MsgId중에서 MessageName과 이름이 같은 Enum값을 리턴해줌.
			ushort size = (ushort)packet.CalculateSize();
            byte[] sendBuffer = new byte[size + 4];
            Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort));
            //맨앞에 2바이트(ushort) size기입
            Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));
            //그다음 2바이트는(ushort) 는 MsgId기입. 
            Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);

            Send(new ArraySegment<byte>(sendBuffer));
        }

		public override void OnConnected(EndPoint endPoint)
		{
			Console.WriteLine($"OnConnected : {endPoint}");

			{
				S_Connected connectedPacket = new S_Connected();
				Send(connectedPacket);
			}

			//TODO : 로비에서 케릭터 선택
			MyPlayer = ObjectManager.Instance.Add<Player>();
            {
				MyPlayer.Info.Name = $"Player_{MyPlayer.Info.ObjectId}";
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;

				StatInfo stat = DataManager.StatDict[1];
				if(stat == null)
                    Console.WriteLine("Stat Data is not available.");
				MyPlayer.Stat.MergeFrom(stat);

				MyPlayer.Session = this;
            }

			//TODO : when comes enterence request than Enter game.
			GameRoom room = RoomManager.Instance.Find(1);
			room.PushJob(room.EnterGame, MyPlayer);
        }

		public override void OnRecvPacket(ArraySegment<byte> buffer)
		{
			PacketManager.Instance.OnRecvPacket(this, buffer);
		}

		public override void OnDisconnected(EndPoint endPoint)
		{
			GameRoom room = RoomManager.Instance.Find(1);
			room.PushJob(room.LeaveGame, MyPlayer.Info.ObjectId);
			
			SessionManager.Instance.Remove(this);
			Console.WriteLine($"OnDisconnected : {endPoint}");
		}

		public override void OnSend(int numOfBytes)
		{
			//Console.WriteLine($"Transferred bytes: {numOfBytes}");
		}
	}
}
