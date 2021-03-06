﻿using LeaguePackets.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LeaguePackets.PayloadPackets
{
    public class Chat : PayloadPacket // 0x68
    {
        public override PayloadPacketID ID => PayloadPacketID.Chat;
        public ClientID ClientID { get; set; }
        public NetID NetID { get; set; }
        public bool Localized { get; set; }
        public ChatType ChatType { get; set; }
        public string Params { get; set; } = "";
        public string Message { get; set; } = "";

        public Chat(){}

        public Chat(PacketReader reader, ChannelID channelID)
        {
            ChannelID = channelID;
            ClientID = reader.ReadClientID();
            NetID = reader.ReadNetID();
            Localized = reader.ReadBool();
            ChatType = reader.ReadChatType();
            var paramsSize = reader.ReadInt32();
            var messageSize = reader.ReadInt32();
            if (paramsSize > 32)
                throw new IOException("Params size too big!");
            if (messageSize > 1024)
                throw new IOException("Message size too big!");
            var pars = reader.ReadBytes(32).Take(paramsSize).ToArray();
            var msg = reader.ReadBytes(messageSize);
            if (Localized)
            {
                Params = Encoding.UTF8.GetString(pars);
                Message = Encoding.UTF8.GetString(msg);
            }
            else
            {
                Params = Encoding.ASCII.GetString(pars);
                Message = Encoding.ASCII.GetString(msg);
            }
            ExtraBytes = reader.ReadLeft();
        }

        public override void WriteBody(PacketWriter writer)
        {
            writer.WriteClientID(ClientID);
            writer.WriteNetID(NetID);
            writer.WriteBool(Localized);
            writer.WriteChatType(ChatType);
            byte[] pars;
            byte[] message;
            if (Localized)
            {
                pars = Encoding.UTF8.GetBytes(Params);
                message = Encoding.UTF8.GetBytes(Message);
            }
            else
            {
                pars = Encoding.ASCII.GetBytes(Params);
                message = Encoding.ASCII.GetBytes(Message);
            }
            var paramsSize = pars.Length;
            if (paramsSize > 32)
                throw new IOException("Params size too big!");
            var messageSize = message.Length;
            if (messageSize > 1024)
                throw new IOException("Message size too big!");
            writer.WriteInt32(paramsSize);
            writer.WriteInt32(messageSize);
            writer.WriteBytes(pars);
            writer.WritePad(32 - paramsSize);
            writer.WriteBytes(message);
            writer.WritePad(1);
        }
    }
}
