﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using LeaguePackets.Common;

namespace LeaguePackets.CommonData
{
    public enum MovementDataType : byte
    {
        None = 0,
        WithSpeed = 1,
        Normal = 2,
        Stop = 3,
    }

    public abstract class MovementData
    {
        public abstract void Write(PacketWriter writer);
        public abstract MovementDataType Type { get; }
    }

    public static class MovementDataExtension
    {
        public static MovementData ReadMovementData(this PacketReader reader, MovementDataType type)
        {
            switch (type)
            {
                case MovementDataType.Stop:
                    return new MovementDataStop(reader);
                case MovementDataType.Normal:
                    return new MovementDataNormal(reader);
                case MovementDataType.WithSpeed:
                    return new MovementDataWithSpeed(reader);
                default:
                    return new MovementDataNone(reader);
            }
        }

        public static void WriteMovementData(this PacketWriter writer, MovementData data)
        {
            data.Write(writer);
        }
    }


    public class MovementDataNone : MovementData
    {
        public override MovementDataType Type => MovementDataType.None;

        public override void Write(PacketWriter writer)
        {
        }
        public MovementDataNone() {}
        public MovementDataNone(PacketReader reader)
        {
        }
    }

    public class MovementDataStop : MovementData
    {
        public override MovementDataType Type => MovementDataType.Stop;
        public Vector2 Position { get; set; }
        public Vector2 Forward { get; set; }

        public override void Write(PacketWriter writer)
        {
            writer.WriteVector2(Position);
            writer.WriteVector2(Forward);
        }
        public MovementDataStop() {}
        public MovementDataStop(PacketReader reader)
        {
            Position = reader.ReadVector2();
            Forward = reader.ReadVector2();
        }
    }

    public class MovementDataNormal : MovementData
    {
        public override MovementDataType Type => MovementDataType.Normal;
        public NetID TeleportNetID { get; set; }
        public bool HasTeleportID { get; set; }
        public byte TeleportID { get; set; }
        public List<Tuple<short, short>> Waypoints { get; set; }

        public override void Write(PacketWriter writer)
        {
            int waypointsSize = Waypoints.Count;
            if(waypointsSize > 0x7F)
            {
                throw new Exception("Too many paths > 0x7F!");
            }
            byte bitfield = 0;
            if(Waypoints != null)
            {
                bitfield |= (byte)(waypointsSize << 1);
            }
            if (HasTeleportID)
            {
                bitfield |= 1;
            }
            writer.WriteByte(bitfield);
            if(Waypoints != null)
            {
                writer.WriteNetID(TeleportNetID);
                if (HasTeleportID)
                {
                    writer.WriteByte(TeleportID);
                }
                writer.WriteCompressedWaypoints(Waypoints);
            }
        }

        public MovementDataNormal() {}

        public MovementDataNormal(PacketReader reader)
        {
            byte bitfield = reader.ReadByte();
            byte size = (byte)(bitfield >> 1);
            HasTeleportID = (bitfield & 1) != 0;
            if(size >= 2)
            {
                TeleportNetID = reader.ReadNetID();
                if (HasTeleportID)
                {
                    TeleportID = reader.ReadByte();
                }
                Waypoints = reader.ReadCompressedWaypoints(size / 2u);
            }
        }
    }

    public class MovementDataWithSpeed : MovementDataNormal
    {
        public override MovementDataType Type => MovementDataType.WithSpeed;
        public SpeedParams SpeedParams { get; set; } = new SpeedParams();
        public override void Write(PacketWriter writer)
        {
            int waypointsSize = Waypoints.Count;
            if (waypointsSize > 0x7F)
            {
                throw new Exception("Too many paths > 0x7F!");
            }
            byte bitfield = 0;
            if (Waypoints != null)
            {
                bitfield |= (byte)(waypointsSize  << 1);
            }
            if (HasTeleportID)
            {
                bitfield |= 1;
            }
            writer.WriteByte(bitfield);
            if (Waypoints != null)
            {
                writer.WriteNetID(TeleportNetID);
                if (HasTeleportID)
                {
                    writer.WriteByte(TeleportID);
                }
                writer.WriteWaypointSpeedParams(SpeedParams);
                writer.WriteCompressedWaypoints(Waypoints);
            }
        }
        public MovementDataWithSpeed() {}
        public MovementDataWithSpeed(PacketReader reader)
        {
            byte bitfield = reader.ReadByte();
            byte size = (byte)(bitfield >> 1);
            HasTeleportID = (bitfield & 1) != 0;
            if (size >= 2)
            {
                TeleportNetID = reader.ReadNetID();
                if(HasTeleportID)
                {
                    TeleportID = reader.ReadByte();
                }
                SpeedParams = reader.ReadWaypointSpeedParams();
                Waypoints = reader.ReadCompressedWaypoints(size / 2u);
            }
        }
    }


    public static class MovementExtension
    {
        public static MovementDataType ReadMovementDataType(this PacketReader reader)
        {
            return (MovementDataType)reader.ReadByte();
        }

        public static void WriteMovementDataType(this PacketWriter writer, MovementDataType type)
        {
            writer.WriteByte((byte)type);
        }

        public static List<Tuple<short, short>> ReadCompressedWaypoints(this PacketReader reader, uint size)
        {
            var data = new List<Tuple<short, short>>();
            BitArray flags;
            if(size >= 2)
            {
                byte[] flagsBuffer = reader.ReadBytes((int)((size - 2) / 4 + 1));
                flags = new BitArray(flagsBuffer);
            }
            else
            {
                flags = new BitArray(new byte[1]);
            }
            short lastX = reader.ReadInt16();
            short lastZ = reader.ReadInt16();
            data.Add(new Tuple<short, short>(lastX, lastZ));

            for (int i = 1, flag = 0; i < size; i++)
            {
                if (flags[flag])
                {
                    lastX += reader.ReadSByte();
                }
                else
                {
                    lastX = reader.ReadInt16();
                }
                flag++;
                if (flags[flag])
                {
                    lastZ += reader.ReadSByte();
                }
                else
                {
                    lastZ = reader.ReadInt16();
                }
                flag++;
                data.Add(new Tuple<short, short>(lastX, lastZ));
            }
            return data;
        }

        public static void WriteCompressedWaypoints(this PacketWriter writer, List<Tuple<short, short>> data)
        {
            int size = data.Count;
            if (size < 1)
            {
                throw new IOException("Need at least 1 waypoint!");
            }
            byte[] flagsBuffer;
            if(size >= 2)
            {
                flagsBuffer = new byte[(size - 2) / 4 + 1u];
            }
            else
            {
                flagsBuffer = new byte[0];
            }
            var flags = new BitArray(flagsBuffer);
            for (int i = 1, flag = 0; i < size; i++)
            {
                int relativeX = data[i].Item1 - data[i - 1].Item1;
                flags[flag] = (relativeX <= SByte.MaxValue && relativeX >= SByte.MinValue);
                flag++;

                int realtiveZ = data[i].Item2 - data[i - 1].Item2;
                flags[flag] = (realtiveZ <= SByte.MaxValue && realtiveZ >= SByte.MinValue);
                flag++;
            }
            flags.CopyTo(flagsBuffer, 0);
            writer.WriteBytes(flagsBuffer);
            writer.WriteInt16(data[0].Item1);
            writer.WriteInt16(data[0].Item2);
            for (int i = 1, flag = 0; i < size; i++)
            {
                if (flags[flag])
                {
                    writer.WriteSByte((SByte)(data[i].Item1 - data[i - 1].Item1));
                }
                else
                {
                    writer.WriteInt16(data[i].Item1);
                }
                flag++;
                if (flags[flag])
                {
                    writer.WriteSByte((SByte)(data[i].Item2 - data[i - 1].Item2));
                }
                else
                {
                    writer.WriteInt16(data[i].Item2);
                }
                flag++;
            }
        }
    }
}
