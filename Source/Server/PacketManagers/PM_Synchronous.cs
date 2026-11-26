using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTNetwork.Components;
using RTShared.Files.Default;
using RTShared.Misc;
using RTShared.Files.Player;

namespace RTServer.PacketManagers
{
    public class PM_Synchronous : PM_Base
    {
        [HandlesPacket(PacketHeader.Synchronous)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            PKT_Synchronous packet = Serializer.ConvertBytesToObject<PKT_Synchronous>(bytes);

            switch (packet.CurrentStepMode)
            {
                case PKT_Synchronous.StepMode.Ask:
                    TryStartSynchronousSession(client, packet);
                    break;

                case PKT_Synchronous.StepMode.Accept:
                    AcceptSynchronousSession(client, packet);
                    break;

                case PKT_Synchronous.StepMode.Reject:
                    RejectSynchronousSession(packet);
                    break;

                case PKT_Synchronous.StepMode.Start:
                    StartSynchronousSession(client);
                    break;

                case PKT_Synchronous.StepMode.Action:
                    RouteToManager(client, packet, header);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void RouteToManager(ServerClient client, PKT_Synchronous packet, PacketHeader header)
        {
            client.Listener.EnqueuePacket(header, packet);
            ServerNetwork.GetClientFromID(client.GetData<FL_Player>().SynchronousClientID).Listener.EnqueuePacket(header, packet);
        }

        private static void TryStartSynchronousSession(ServerClient client, PKT_Synchronous packet)
        {
            FL_Settlement settlement = PM_Settlements.GetSettlementFileFromTile(packet.ToTile);
            ServerClient toFind = ServerNetwork.GetConnectedClientFromUsername(settlement.Username);

            if (toFind == null) ResponseShortcutManager.SendUserUnavailablePacket(client);
            else
            {
                packet.CurrentStepMode = PKT_Synchronous.StepMode.Ask;
                packet.Username = client.GetData<FL_Player>().Username;
                packet.FromTile = PM_Settlements.GetSettlementFileFromUsername(client.GetData<FL_Player>().Username).Tile;
                toFind.Listener.EnqueuePacket(PacketHeader.Synchronous, packet);
            }
        }

        private static void AcceptSynchronousSession(ServerClient client, PKT_Synchronous packet)
        {
            FL_Settlement settlement = PM_Settlements.GetSettlementFileFromTile(packet.ToTile);
            ServerClient toFind = ServerNetwork.GetConnectedClientFromUsername(settlement.Username);

            client.GetData<FL_Player>().SynchronousClientID = toFind.ID;
            toFind.GetData<FL_Player>().SynchronousClientID = client.ID;

            packet.CurrentStepMode = PKT_Synchronous.StepMode.Accept;
            toFind.Listener.EnqueuePacket(PacketHeader.Synchronous, packet);
        }

        private static void RejectSynchronousSession(PKT_Synchronous packet)
        {
            FL_Settlement settlement = PM_Settlements.GetSettlementFileFromTile(packet.ToTile);
            ServerClient toFind = ServerNetwork.GetConnectedClientFromUsername(settlement.Username);

            PKT_Synchronous _ = new PKT_Synchronous();
            _.CurrentStepMode = PKT_Synchronous.StepMode.Reject;
            _.FromTile = packet.FromTile;
            _.ToTile = packet.ToTile;

            toFind.Listener.EnqueuePacket(PacketHeader.Synchronous, _);
        }

        private static void StartSynchronousSession(ServerClient client)
        {
            PKT_Synchronous packet = new PKT_Synchronous() { CurrentStepMode = PKT_Synchronous.StepMode.Start };
            ServerNetwork.GetClientFromID(client.GetData<FL_Player>().SynchronousClientID).Listener.EnqueuePacket(PacketHeader.Synchronous, packet);
        }
    }
}
