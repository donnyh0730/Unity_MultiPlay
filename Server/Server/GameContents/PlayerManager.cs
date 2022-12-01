using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents
{
    public class PlayerManager
    {
        public static PlayerManager Instance { get; set; } = new PlayerManager();
        object _lock = new object();
        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        int GenPlayerId = 1; //TODO

        public Player CreateAndAddPlayer()
        {
            Player player = new Player();

            lock (_lock)
            {
                player.Info.PlayerId = GenPlayerId;
                //TODO info 채워야함.
                _players.Add(GenPlayerId, player);
                GenPlayerId++;
            }
            return player;
        }

        public bool Remove(int playerId)
        {
            lock (_lock)
            {
                return _players.Remove(playerId);
            }
        }

        public Player Find(int playerId)
        {
            lock (_lock)
            {
                Player player = null;
                if (_players.TryGetValue(playerId, out player))
                    return player;

                return null;
            }
        }
    }
}
