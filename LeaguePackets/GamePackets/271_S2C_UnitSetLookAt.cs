﻿using LeaguePackets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace LeaguePackets.GamePackets
{
    public class S2C_UnitSetLookAt : GamePacket // 0x10F
    {
        public override GamePacketID ID => GamePacketID.S2C_UnitSetLookAt;
        public LookAtType LookAtType { get; set; }
        public Vector3 TargetPosition { get; set; }
        public NetID TargetNetID { get; set; }
        public S2C_UnitSetLookAt(){}

        public S2C_UnitSetLookAt(PacketReader reader, ChannelID channelID, NetID senderNetID)
        {
            this.SenderNetID = senderNetID;
            this.ChannelID = channelID;

            this.LookAtType = reader.ReadLookAtType();
            this.TargetPosition = reader.ReadVector3();
            this.TargetNetID = reader.ReadNetID();
        
            this.ExtraBytes = reader.ReadLeft();
        }
        public override void WriteBody(PacketWriter writer)
        {
            writer.WriteLookAtType(LookAtType);
            writer.WriteVector3(TargetPosition);
            writer.WriteNetID(TargetNetID);
        }
    }
}
