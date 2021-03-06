﻿using LeaguePackets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.GamePackets
{
    public class C2S_TutorialAudioEventFinished : GamePacket // 0x5
    {
        public override GamePacketID ID => GamePacketID.C2S_TutorialAudioEventFinished;
        public NetID AudioEventNetID { get; set; }

        public C2S_TutorialAudioEventFinished(){}

        public C2S_TutorialAudioEventFinished(PacketReader reader, ChannelID channelID, NetID senderNetID)
        {
            this.SenderNetID = senderNetID;
            this.ChannelID = channelID;

            this.AudioEventNetID = reader.ReadNetID();

            this.ExtraBytes = reader.ReadLeft();
        }
        public override void WriteBody(PacketWriter writer)
        {
            writer.WriteNetID(AudioEventNetID);
        }
    }
}
