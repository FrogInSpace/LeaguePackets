﻿using LeaguePackets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.GamePackets
{
    public class S2C_SetAnimStates : GamePacket // 0x6B
    {
        public override GamePacketID ID => GamePacketID.S2C_SetAnimStates;
        public Dictionary<string, string> AnimationOverrides { get; set; } = new Dictionary<string, string>();
        public S2C_SetAnimStates(){}

        public S2C_SetAnimStates(PacketReader reader, ChannelID channelID, NetID senderNetID)
        {
            this.SenderNetID = senderNetID;
            this.ChannelID = channelID;

            int number = reader.ReadByte();
            for (int i = 0; i < number; i++)
            {
                var fromAnim = reader.ReadSizedString();
                var toAnim = reader.ReadSizedString();
                this.AnimationOverrides[fromAnim] = toAnim;
            }
            this.ExtraBytes = reader.ReadLeft();
        }
        public override void WriteBody(PacketWriter writer)
        {
            int number = AnimationOverrides.Count;
            if (number > 0xFF)
            {
                throw new IOException("AnimationOverrides list too big!");
            }
            foreach (var kvp in AnimationOverrides)
            {
                writer.WriteSizedString(kvp.Key);
                writer.WriteSizedString(kvp.Value);
            }
        }
    }
}
