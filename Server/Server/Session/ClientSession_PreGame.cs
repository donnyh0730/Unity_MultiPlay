﻿using Google.Protobuf;
using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DB;
using Server.GameContents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	public partial class ClientSession
	{
		public int AccountDbId {  get; private set; }
		public List<LobbyPlayerInfo> LobbyPlayers { get; set; } = new List<LobbyPlayerInfo>();

		public void HandleLogin(C_LoginRequest loginRequest)
		{
			//TODO 이런저런 보안체크 
			if (ServerState != PlayerServerState.ServerStateLogin)
				return;

			LobbyPlayers.Clear();

			using (AppDbContext db = new AppDbContext())
			{
				var Account = db.Accounts
					.Where(a => a.AccountName == loginRequest.UniqueId)
					.Include(a => a.Players)
					.FirstOrDefault();

				if (Account != null)
				{
					//AccountDbId 메모리에 기억
					AccountDbId = Account.AccountDbId;

					S_LoginResult loginResult = new S_LoginResult() { LoginResult = 1 };

					foreach(PlayerDb playerDb in Account.Players)
					{
						LobbyPlayerInfo lobbyPlayerInfo = new LobbyPlayerInfo()
						{
							Name = playerDb.PlayerName,
							StatInfo = new StatInfo()
							{
								Level = playerDb.Level,
								Hp = playerDb.Hp,
								MaxHp = playerDb.MaxHp,
								Attack = playerDb.Attack,
								Speed = playerDb.Speed,
								CurrentExp = playerDb.CurrentExp,
							}
						};
						//메모리에도 들고 있도록한다.
						LobbyPlayers.Add(lobbyPlayerInfo);
						//패킷에 넣어준다.
						loginResult.Players.Add(lobbyPlayerInfo);
					}

					Send(loginResult);
					//로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}
				else //Account가 없는 경우 아얘 새로 만들어준다.
				{
					AccountDb newAcc = new AccountDb() { AccountName = loginRequest.UniqueId };
					db.Accounts.Add(newAcc);
					db.SaveChanges();

					AccountDbId = newAcc.AccountDbId;

					S_LoginResult loginOk = new S_LoginResult() { LoginResult = 1 };
					Send(loginOk);
					//로비로 이동
					ServerState = PlayerServerState.ServerStateLobby;
				}
			}
		}

		public void HandleEnterGame(C_EnterGame enterGamePkt)
		{
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;

			LobbyPlayerInfo PlayerInfo = LobbyPlayers.Find(p => p.Name == enterGamePkt.Name);
			if (PlayerInfo == null)
				return;

			MyPlayer = ObjectManager.Instance.Add<Player>();
			{
				MyPlayer.Info.Name = PlayerInfo.Name;
				MyPlayer.Info.PosInfo.State = CreatureState.Idle;
				MyPlayer.Info.PosInfo.MoveDir = MoveDir.Down;
				MyPlayer.Info.PosInfo.PosX = 0;
				MyPlayer.Info.PosInfo.PosY = 0;
				MyPlayer.Stat.MergeFrom(PlayerInfo.StatInfo);//그냥 값복사 코드다.
				MyPlayer.Session = this;
			}

			ServerState = PlayerServerState.ServerStateGame;
			GameRoom room = RoomManager.Instance.Find(1);
			room.PushJob(room.EnterGame, MyPlayer);
		}

		public void HandleCreatePlayer(C_CreatePlayer createPlayerPkt)
		{
			//TODO 이런저런 보안체크 
			if (ServerState != PlayerServerState.ServerStateLobby)
				return;

			using (AppDbContext db = new AppDbContext())
			{
				PlayerDb findplayer = db.Players
					.Where(p => p.PlayerName == createPlayerPkt.Name)
					.FirstOrDefault();
				if(findplayer !=null )
				{
					//이미 있는 이름인 경우 빈깡통패킷을보낸다.
					Send(new S_CreatePlayer());
				}
				else
				{
					//1레벨 스탯정보 추출
					StatInfo stat = null;
					DataManager.StatDict.TryGetValue(1, out stat);

					//DB에 플레이어를 만들어줘야함.
					PlayerDb newPlayerDb = new PlayerDb()
					{
						PlayerName = createPlayerPkt.Name,
						Level = stat.Level,
						Hp = stat.Hp,
						MaxHp = stat.MaxHp,
						Attack = stat.Attack,
						Speed = stat.Speed,
						CurrentExp = 0,
						AccountDbId = AccountDbId
					};
					db.Players.Add(newPlayerDb);
					db.SaveChanges();

					LobbyPlayerInfo lobbyPlayerInfo = new LobbyPlayerInfo()
					{
						Name = createPlayerPkt.Name,
						StatInfo = new StatInfo()
						{
							Level = stat.Level,
							Hp = stat.Hp,
							MaxHp = stat.MaxHp,
							Attack = stat.Attack,
							Speed = stat.Speed,
							CurrentExp = stat.CurrentExp,
						}
					};
					//메모리에도 들고 있도록한다.
					LobbyPlayers.Add(lobbyPlayerInfo);
					//Client에 전송
					S_CreatePlayer CreatedPlayerPkt = new S_CreatePlayer() { Player = new LobbyPlayerInfo() };
					CreatedPlayerPkt.Player.MergeFrom(lobbyPlayerInfo);

					Send(CreatedPlayerPkt);
				}
			}
		}
	}
}
