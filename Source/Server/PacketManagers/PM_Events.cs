using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTServer.Misc;
using RTShared.Files.Default;
using RTShared.Files.Player;
using RTShared.Misc;
using static RTNetwork.Packets.PKT_Event;

namespace RTServer.PacketManagers
{
    public class PM_Events : PM_Base
    {
        public static List<FL_Event> LoadedEvents { get; private set; } = null;

        [HandlesPacket(PacketHeader.Event)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            PKT_Event packet = Serializer.ConvertBytesToObject<PKT_Event>(bytes);

            if (!FL_PlayerCooldown.CheckIfCanEvent(client.GetData<FL_Player>(), Master.ActionConfigs.EventAction))
            {
                packet.CurrentStepMode = StepMode.Recover;
                client.Listener.EnqueuePacket(PacketHeader.Event, packet);
            }

            else
            {
                switch (packet.CurrentStepMode)
                {
                    case StepMode.Send:
                        SendEvent(client, packet);
                        break;

                    case StepMode.Bulk:
                        SetEvents(client, packet);
                        break;
                }

                client.GetData<FL_Player>().Cooldowns.SetEventTimer(client.GetData<FL_Player>());
            }
        }

        private static void SendEvent(ServerClient client, PKT_Event packet)
        {
            if (!PM_Settlements.CheckIfTileIsInUse(packet.ToTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.GetData<FL_Player>().Username} attempted to send an event to settlement at tile {packet.ToTile}, but it has no settlement");
            else
            {
                FL_Settlement settlement = PM_Settlements.GetSettlementFileFromTile(packet.ToTile);
                if (!UserManagerH.CheckIfUserIsConnected(settlement.Username))
                {
                    packet.CurrentStepMode = StepMode.Recover;
                    client.Listener.EnqueuePacket(PacketHeader.Event, packet);
                }

                else
                {
                    ServerClient target = ServerNetwork.GetConnectedClientFromUsername(settlement.Username);

                    //Back to player

                    client.Listener.EnqueuePacket(PacketHeader.Event, packet);

                    //To the person that should receive it

                    packet.CurrentStepMode = StepMode.Receive;

                    target.Listener.EnqueuePacket(PacketHeader.Event, packet);
                }
            }
        }

        private static void SetEvents(ServerClient client, PKT_Event data)
        {
            if (!client.GetData<FL_Player>().IsAdmin && PM_World.CheckIfWorldExists()) client.Listener.MarkForDisconnect();
            else
            {
                foreach (FL_Event file in data.Files)
                {
                    Serializer.SerializeToFile(Path.Combine(Master.EventsPath, file.DefName + CommonValues.DefaultSaveFormat), file);
                }

                LoadAllEvents();
                InformationDisplayer.DisplaySetEvents(client);
                ServerNetwork.SendPacketToAllClients(PacketHeader.Event, data);
            }
        }

        public static void LoadAllEvents()
        {
            List<FL_Event> toLoad = new List<FL_Event>();
            foreach (string str in Directory.GetFiles(Master.EventsPath))
            {
                FL_Event file = Serializer.SerializeFromFile<FL_Event>(str);
                toLoad.Add(file);

                Printer.Warning($"Loaded event '{file.Name}'", Printer.Verbosity.Extreme);
            }

            LoadedEvents = toLoad.OrderBy(fetch => fetch.Name).ToList();
        }
    }
}
