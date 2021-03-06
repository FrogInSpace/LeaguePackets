﻿using System;
using System.IO;
using System.Collections.Generic;
using ENet;
using LeaguePackets;
using LeaguePackets.Common;
using LeaguePackets.GamePackets;
using LeaguePackets.CommonData;
using LeaguePackets.PayloadPackets;
using Newtonsoft.Json;
using System.Numerics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;

namespace LeaguePacketsSender
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            var address = new Address(Address.IPv4HostAny, 5119);
            var key = Convert.FromBase64String("17BLOhi6KZsTtldTsizvHg==");
            var cids = new List<ClientID> { (ClientID)1 };
            var server = new LeagueServer(address, key, cids);
            var mapNum = 1;
            var playerLiteInfo = new PlayerLoadInfo
            {
                PlayerID = (PlayerID)1,
                SummonorLevel = 30,
                TeamId = TeamID.Order,
            };
            var skinID = 0u;
            var championName = "Annie";
            var playerName = "Test";
            var jSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            };
            var commandsList = new List<KeyValuePair<Regex, MethodInfo>>();
            foreach(var method in typeof(Commands).GetMethods(BindingFlags.Static | BindingFlags.Public))
            {
                var regex = new Regex(@"\." + method.Name + @"((?:\s(?:.*)|))?", RegexOptions.IgnoreCase);
                commandsList.Add(new KeyValuePair<Regex, MethodInfo>(regex, method));
            }


            server.OnPacket += (s, e) =>
            {
                var packet = e.Packet;
                var cid = e.ClientID;
                var channel = e.ChannelID;
                if(packet is IUnusedPacket)
                {
                    
                }
                else if (packet is C2S_QueryStatusReq statusReq)
                {
                    var answer = new S2C_QueryStatusAns();
                    answer.SenderNetID = (NetID)(uint)cid;
                    answer.Response = true;
                    server.SendEncrypted(cid, ChannelID.Broadcast, answer);
                }
                else if(packet is RequestJoinTeam reqJoinTeam)
                {
                    var answer2 = new TeamRosterUpdate();
                    answer2.TeamSizeOrder = 1;
                    answer2.OrderMembers[0] = (PlayerID)cid;
                    answer2.TeamSizeOrderCurrent = 1;
                    server.SendEncrypted(cid, ChannelID.LoadingScreen, answer2);

                    var answer1 = new RequestReskin();
                    answer1.PlayerID = (PlayerID)cid;
                    answer1.SkinID = (int)skinID;
                    answer1.SkinName = championName;
                    server.SendEncrypted(cid, ChannelID.LoadingScreen, answer1);


                    var answer3 = new RequestRename();
                    answer3.PlayerID = (PlayerID)cid;
                    answer3.SkinID = 0;
                    answer3.PlayerName = playerName;
                    server.SendEncrypted(cid, ChannelID.LoadingScreen, answer3);
                }
                else if(packet is C2S_Ping_Load_Info reqPingLoadInfo)
                {
                    var answer = new S2C_Ping_Load_Info();
                    answer.SenderNetID = (NetID)(uint)cid;
                    answer.ConnectionInfo = reqPingLoadInfo.ConnectionInfo;
                    answer.ConnectionInfo.PlayerID = (PlayerID)cid;
                    server.SendEncrypted(cid, ChannelID.Broadcast, answer);
                }
                else if (packet is SynchVersionC2S syncReq)
                {
                    var answer = new SynchVersionS2C();
                    answer.VersionMatches = true;
                    answer.VersionString = syncReq.Version;
                    answer.MapToLoad = mapNum;
                    answer.PlayerInfo[0] = playerLiteInfo;
                    answer.MapMode = "CLASSIC";
                    answer.PlatformID = "NA1";
                    answer.GameFeatures |= GameFeatures.FoundryOptions;
                    answer.GameFeatures |= GameFeatures.EarlyWarningForFOWMissiles;
                    answer.GameFeatures |= GameFeatures.NewPlayerRecommendedPages;
                    answer.GameFeatures |= GameFeatures.HighlightLineMissileTargets;
                    server.SendEncrypted(cid, ChannelID.Broadcast, answer);
                }
                else if(packet is C2S_CharSelected reqSelected)
                {
                    var startSpawn = new S2C_StartSpawn();
                    server.SendEncrypted(cid, ChannelID.Broadcast, startSpawn);


                    var spawnHero = new S2C_CreateHero();
                    spawnHero.SenderNetID = (NetID)0x40000001;
                    spawnHero.Name = playerName;
                    spawnHero.Skin = championName;
                    spawnHero.SkinID = 0;
                    spawnHero.NetNodeID = NetNodeID.Spawned;
                    spawnHero.NetID = (NetID)0x40000001;
                    spawnHero.TeamIsOrder = true;
                    spawnHero.CreateHeroDeath = CreateHeroDeath.Alive;
                    spawnHero.PlayerUID = cid;
                    spawnHero.SpawnPositionIndex = 2;
                    server.SendEncrypted(cid, ChannelID.Broadcast, spawnHero);


                    var avatarInfo = new AvatarInfo_Server();
                    avatarInfo.SenderNetID = (NetID)0x40000001;
                    server.SendEncrypted(cid, ChannelID.Broadcast, avatarInfo);


                    var endSpawn = new S2C_EndSpawn();
                    server.SendEncrypted(cid, ChannelID.Broadcast, endSpawn);
                }
                else if(packet is C2S_ClientReady reqReady)
                {
                    var startGame = new S2C_StartGame();
                    startGame.EnablePause = true;
                    server.SendEncrypted(cid, ChannelID.Broadcast, startGame);
                }
                else if(packet is World_SendCamera_Server reqCamerPosition)
                {
                    
                }
                else if(packet is World_LockCamera_Server reqLockCameraServer)
                {
                    
                }
                else if(packet is Chat reqChat)
                {
                    foreach(var kvp in commandsList)
                    {
                        var match = kvp.Key.Match(reqChat.Message);
                        if(match.Groups.Count == 2)
                        {
                            object value = match.Groups[1].Value;
                            object result = kvp.Value.Invoke(null, new object[] {
                                s, e.ClientID, value 
                            });
                            if(result != null && result is string strResult)
                            {

                                var response = new Chat();
                                response.Localized = false;
                                response.Message = strResult;
                                response.ChatType = ChatType.Team;
                                response.ClientID = e.ClientID;
                                //response.Params = "Command";
                                server.SendEncrypted(e.ClientID, ChannelID.Chat, response);
                            }
                            break;
                        }
                    }
                }
                else
                {
                    Console.WriteLine(JsonConvert.SerializeObject(e, jSettings));
                    if(packet is NPC_IssueOrderReq movReq && movReq.OrderType == OrderType.MoveTo)
                    {
                        var resWaypoints = new WaypointGroup();
                        resWaypoints.SenderNetID = (NetID)0x40000001;
                        resWaypoints.SyncID = (int)Environment.TickCount;
                        resWaypoints.Movements.Add(new MovementDataNormal
                        {
                            TeleportNetID = (NetID)0x40000001,
                            Waypoints = movReq.Waypoints
                        });
                        server.SendEncrypted(e.ClientID, ChannelID.Broadcast, resWaypoints);
                    }
                }
            };
            server.OnBadPacket += (s, e) =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(e, jSettings));
            };
            server.OnConnected += (s, e) =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(e, jSettings));
            };
            server.OnDisconnected += (s, e) =>
            {
                Console.WriteLine(JsonConvert.SerializeObject(e, jSettings));
            };
            while(true)
            {
                server.RunOnce();
            }
        }
    }
}
