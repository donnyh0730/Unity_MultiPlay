using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameContents
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            //TODO
            base.OnDamaged(attacker, damage);
            Console.WriteLine($"Damage : {damage}");
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
        }
    }
}
