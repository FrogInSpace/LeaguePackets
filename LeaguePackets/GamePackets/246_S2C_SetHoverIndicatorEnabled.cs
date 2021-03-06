﻿using LeaguePackets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.GamePackets
{
    public class S2C_SetHoverIndicatorEnabled : GamePacket // 0xF6
    {
        public override GamePacketID ID => GamePacketID.S2C_SetHoverIndicatorEnabled;
        public bool Enabled { get; set; }
        public S2C_SetHoverIndicatorEnabled(){}

        public S2C_SetHoverIndicatorEnabled(PacketReader reader, ChannelID channelID, NetID senderNetID)
        {
            this.SenderNetID = senderNetID;
            this.ChannelID = channelID;

            this.Enabled = reader.ReadBool();
        
            this.ExtraBytes = reader.ReadLeft();
        }
        public override void WriteBody(PacketWriter writer)
        {
            writer.WriteBool(Enabled);
        }
    }
}
