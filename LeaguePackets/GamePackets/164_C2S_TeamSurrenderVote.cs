﻿using LeaguePackets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.GamePackets
{
    public class C2S_TeamSurrenderVote : GamePacket // 0xA4
    {
        public override GamePacketID ID => GamePacketID.C2S_TeamSurrenderVote;
        public bool VotedYes { get; set; }
        public C2S_TeamSurrenderVote(){}

        public C2S_TeamSurrenderVote(PacketReader reader, ChannelID channelID, NetID senderNetID)
        {
            this.SenderNetID = senderNetID;
            this.ChannelID = channelID;

            byte bitfield = reader.ReadByte();
            this.VotedYes = (bitfield & 1) != 0;
        
            this.ExtraBytes = reader.ReadLeft();
        }
        public override void WriteBody(PacketWriter writer)
        {
            byte bitfield = 0;
            if (VotedYes)
                bitfield |= 1;
            writer.WriteByte(bitfield);
        }
    }
}
