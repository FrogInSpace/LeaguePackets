﻿using LeaguePackets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.GamePackets
{
    public class S2C_ReattachFollowerObject : GamePacket // 0xF2
    {
        public override GamePacketID ID => GamePacketID.S2C_ReattachFollowerObject;
        public NetID NewOwnerId { get; set; }
        public S2C_ReattachFollowerObject(){}

        public S2C_ReattachFollowerObject(PacketReader reader, ChannelID channelID, NetID senderNetID)
        {
            this.SenderNetID = senderNetID;
            this.ChannelID = channelID;

            this.NewOwnerId = reader.ReadNetID();
        
            this.ExtraBytes = reader.ReadLeft();
        }
        public override void WriteBody(PacketWriter writer)
        {
            writer.WriteNetID(NewOwnerId);
        }
    }
}
