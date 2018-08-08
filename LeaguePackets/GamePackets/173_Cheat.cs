﻿using LeaguePackets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.GamePackets
{
    public class Cheat : GamePacket, IUnusedPacket // 0xAD
    {
        public override GamePacketID ID => GamePacketID.Cheat;
        public static Cheat CreateBody(PacketReader reader, ChannelID channelID, NetID senderNetID)
        {
            var result = new Cheat();
            result.SenderNetID = senderNetID;
            result.ChannelID = channelID;

            return result;
        }
        public override void WriteBody(PacketWriter writer)
        {
        }
    }
}