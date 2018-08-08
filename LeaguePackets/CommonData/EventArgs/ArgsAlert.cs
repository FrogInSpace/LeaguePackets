﻿using System;
namespace LeaguePackets.CommonData.EventArgs
{
    public abstract class ArgsAlert : ArgsBase
    {
        public override void ReadArgs(PacketReader reader)
        {
             reader.ReadPad(4);
        }
        public override void WriteArgs(PacketWriter writer)
        {
            writer.WritePad(4);
        }
    }
}
