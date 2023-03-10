using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace AnimationEngine.Networking
{

    /// <summary>
    /// Author: Math0424
    /// Version 1.3
    /// Feel free to use in your own projects
    /// </summary>
    public static class EasyNetworker
    {

        /// <summary>
        /// Before sending to players, serverside only
        /// Use this method to verify packets and make 
        /// sure no funny business is happening. 
        /// </summary>
        public static Action<Type, PacketIn> ProcessPacket;

        public enum TransitType
        {
            Final = 0,
            ToServer = 1,
            ToAll = 2,
            ExcludeSender = 4,
        }

        private static ushort CommsId;
        private static List<IMyPlayer> tempPlayers;
        private static Dictionary<Type, Action<PacketIn>> registry;

        public static void Init(ushort commsId)
        {
            CommsId = commsId;
            tempPlayers = new List<IMyPlayer>();
            registry = new Dictionary<Type, Action<PacketIn>>();

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(CommsId, RecivedPacket);
            MyAPIGateway.Entities.OnCloseAll += UnRegister;
        }

        private static void UnRegister()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(CommsId, RecivedPacket);
        }

        public static void RegisterPacket(Type type, Action<PacketIn> callee)
        {
            if (!registry.ContainsKey(type))
            {
                registry.Add(type, callee);
            }
            else
            {
                throw new Exception($"Already registered type {type.Name}");
            }
        }

        private static bool Validate(object obj)
        {
            if (registry.ContainsKey(obj.GetType()))
            {
                return true;
            }
            throw new Exception($"Type {obj.GetType().Name} not found in packet registry");
        }

        /// <summary>
        /// Send a packet to the server
        /// </summary>
        /// <param name="obj"></param>
        public static void SendToServer(object obj)
        {
            Validate(obj);
            ServerPacket packet = new ServerPacket(obj.GetType(), TransitType.ToServer);
            packet.Wrap(obj);
            MyAPIGateway.Multiplayer.SendMessageToServer(CommsId, MyAPIGateway.Utilities.SerializeToBinary(packet));
        }

        /// <summary>
        /// Send to all players within your current sync range
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="flag"></param>
        public static void SendToSyncRange(object obj, TransitType flag)
        {
            Validate(obj);
            ServerPacket packet = new ServerPacket(obj.GetType(), flag);
            packet.Wrap(obj);
            packet.Range = MyAPIGateway.Session.SessionSettings.SyncDistance;
            packet.TransmitLocation = MyAPIGateway.Session.Player.GetPosition();
            MyAPIGateway.Multiplayer.SendMessageToServer(CommsId, MyAPIGateway.Utilities.SerializeToBinary(packet));
        }

        /// <summary>
        /// Transmit to all players, optionally including the sending player
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="excludeSender"></param>
        public static void SendToAllPlayers(object obj, bool excludeSender)
        {
            Validate(obj);
            ServerPacket packet = new ServerPacket(obj.GetType(), TransitType.ToAll | (excludeSender ? TransitType.ExcludeSender : 0));
            packet.Wrap(obj);
            MyAPIGateway.Multiplayer.SendMessageToServer(CommsId, MyAPIGateway.Utilities.SerializeToBinary(packet));
        }

        private static void RecivedPacket(ushort handler, byte[] raw, ulong id, bool isFromServer)
        {
            try
            {
                ServerPacket packet = MyAPIGateway.Utilities.SerializeFromBinary<ServerPacket>(raw);
                PacketIn packetIn = new PacketIn(packet.Data, id, isFromServer);

                ProcessPacket?.Invoke(packet.Type, packetIn);
                if (packetIn.IsCancelled)
                    return;

                if (isFromServer)
                {
                    if (MyAPIGateway.Session.IsServer && !packet.Flag.HasFlag(TransitType.Final))
                    {
                        if (!packet.Flag.HasFlag(TransitType.ExcludeSender))
                        {
                            registry[packet.Type]?.Invoke(packetIn);
                        }
                    }
                    else
                    {
                        registry[packet.Type]?.Invoke(packetIn);
                    }
                }

                if (MyAPIGateway.Session.IsServer && packet.Flag.HasFlag(TransitType.ToAll))
                {
                    TransmitPacket(id, packet);
                }

            }
            catch (Exception e)
            {
                MyLog.Default.WriteLineAndConsole($"Malformed packet from {id}!");
                MyLog.Default.WriteLineAndConsole($"{e.Message}\n{e.StackTrace}\n\n{e.InnerException}\n\n{e.Source}");

                if (MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[Mod critical error! | Send SpaceEngineers.Log]", 10000, MyFontEnum.Red);
            }
        }

        private static void UpdatePlayers()
        {
            if (tempPlayers == null)
                tempPlayers = new List<IMyPlayer>(MyAPIGateway.Session.SessionSettings.MaxPlayers);
            else
                tempPlayers.Clear();

            MyAPIGateway.Players.GetPlayers(tempPlayers);
        }

        private static void TransmitPacket(ulong sender, ServerPacket packet)
        {
            UpdatePlayers();

            foreach (var p in tempPlayers)
            {
                if (p.IsBot || (packet.Flag.HasFlag(TransitType.ExcludeSender) && p.SteamUserId == sender) ||
                    (MyAPIGateway.Session.IsServer && MyAPIGateway.Session?.Player?.SteamUserId == sender))
                    continue;

                ServerPacket send = new ServerPacket(packet.Type, TransitType.Final);
                send.Data = packet.Data;

                if (packet.Range != -1)
                {
                    if (packet.Range >= Vector3D.Distance(p.GetPosition(), packet.TransmitLocation))
                    {
                        MyAPIGateway.Multiplayer.SendMessageTo(CommsId, MyAPIGateway.Utilities.SerializeToBinary(send), p.SteamUserId);
                    }
                }
                else
                {
                    MyAPIGateway.Multiplayer.SendMessageTo(CommsId, MyAPIGateway.Utilities.SerializeToBinary(send), p.SteamUserId);
                }
            }
        }

        [ProtoContract]
        private class ServerPacket
        {

            [ProtoMember(1)] public Type Type;
            [ProtoMember(2)] public int Range = -1;
            [ProtoMember(3)] public Vector3D TransmitLocation = Vector3D.Zero;
            [ProtoMember(4)] public TransitType Flag;
            [ProtoMember(5)] public byte[] Data;

            public ServerPacket() { }

            public ServerPacket(Type Type, TransitType Flag)
            {
                this.Type = Type;
                this.Flag = Flag;
            }

            public void Wrap(object data)
            {
                Data = MyAPIGateway.Utilities.SerializeToBinary(data);
            }
        }

        public class PacketIn
        {
            public bool IsCancelled { protected set; get; }
            public ulong SenderId { protected set; get; }
            public bool IsFromServer { protected set; get; }

            private readonly byte[] Data;

            public PacketIn(byte[] data, ulong senderId, bool isFromServer)
            {
                this.SenderId = senderId;
                this.IsFromServer = isFromServer;
                this.Data = data;
            }

            public T UnWrap<T>()
            {
                return MyAPIGateway.Utilities.SerializeFromBinary<T>(Data);
            }

            public void SetCancelled(bool value)
            {
                this.IsCancelled = value;
            }
        }

    }
}
