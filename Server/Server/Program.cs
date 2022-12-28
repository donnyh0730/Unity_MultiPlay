using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using Server.Data;
using Server.GameContents;
using ServerCore;

namespace Server
{
	
	public class Program
	{
		static Listener _listener = new Listener();
		static void FlushRoom()
		{
			JobTimer.Instance.Push(FlushRoom, 250);//자기 자신을 250ms마다 다시호출 하게끔 람다로 넣음.
		}

		static void Main(string[] args)
		{
			ConfigManager.LoadConfig();
			DataManager.LoadData();

            RoomManager.Instance.CreateAndAddRoom(1);//여기서 ID가 1번인 GameRoom이 생성된다.

			// DNS (Domain Name System)
			string host = Dns.GetHostName();
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

			_listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });
			Console.WriteLine("Listening...");

			//FlushRoom();
			//JobTimer.Instance.Push(FlushRoom);
			
			while (true)
			{
				RoomManager.Instance.Find(1).Update();
			}
		}
	}
}
