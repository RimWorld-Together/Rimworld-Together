using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTShared.Files.Default;
using RTShared.Files.Player;
using RTShared.Misc;
using static RTNetwork.Packets.PKT_Aid;

namespace RTServer.PacketManagers
{
    public class PM_Aid : PM_Base
    {
        [HandlesPacket(PacketHeader.Aid)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            PKT_Aid data = Serializer.ConvertBytesToObject<PKT_Aid>(bytes);

            if (!FL_PlayerCooldown.CheckIfCanAid(client.GetData<FL_Player>(), Master.ActionConfigs.AidAction))
            {
                data.StepMode = AidStepMode.Reject;
                client.Listener.EnqueuePacket(PacketHeader.Aid, data);
            }

            else
            {
                switch (data.StepMode)
                {
                    case AidStepMode.Send:
                        SendAidRequest(client, data);
                        break;

                    case AidStepMode.Accept:
                        SendAidAccept(client, data);
                        break;

                    case AidStepMode.Reject:
                        SendAidReject(client, data);
                        break;
                }

                client.GetData<FL_Player>().Cooldowns.SetAidTimer(client.GetData<FL_Player>());
            }
        }

        private static void SendAidRequest(ServerClient client, PKT_Aid data)
        {
            if (!PM_Settlements.CheckIfTileIsInUse(data.ToTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.GetData<FL_Player>().Username} attempted to send an aid packet to settlement at tile {data.ToTile}, but it has no settlement");
            else
            {
                FL_Settlement settlementFile = PM_Settlements.GetSettlementFileFromTile(data.ToTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.Username))
                {
                    data.StepMode = AidStepMode.Receive;
                    ServerClient target = ServerNetwork.GetConnectedClientFromUsername(settlementFile.Username);
                    target.Listener.EnqueuePacket(PacketHeader.Aid, data);
                }

                else
                {
                    data.StepMode = AidStepMode.Reject;
                    client.Listener.EnqueuePacket(PacketHeader.Aid, data);
                }
            }
        }

        private static void SendAidAccept(ServerClient client, PKT_Aid data)
        {
            if (!PM_Settlements.CheckIfTileIsInUse(data.FromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.GetData<FL_Player>().Username} attempted to send an aid packet to settlement at tile {data.FromTile}, but it has no settlement");
            else
            {
                FL_Settlement settlementFile = PM_Settlements.GetSettlementFileFromTile(data.FromTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.Username))
                {
                    ServerClient target = ServerNetwork.GetConnectedClientFromUsername(settlementFile.Username);
                    target.Listener.EnqueuePacket(PacketHeader.Aid, data);
                }

                //Back to client sending the request

                else
                {
                    data.StepMode = AidStepMode.Reject;
                    client.Listener.EnqueuePacket(PacketHeader.Aid, data);
                }
            }
        }

        private static void SendAidReject(ServerClient client, PKT_Aid data)
        {
            if (!PM_Settlements.CheckIfTileIsInUse(data.FromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.GetData<FL_Player>().Username} attempted to send an aid packet to settlement at tile {data.FromTile}, but it has no settlement");
            else
            {
                FL_Settlement settlementFile = PM_Settlements.GetSettlementFileFromTile(data.FromTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.Username))
                {
                    ServerClient target = ServerNetwork.GetConnectedClientFromUsername(settlementFile.Username);
                    target.Listener.EnqueuePacket(PacketHeader.Aid, data);
                }

                //Back to client sending the request

                else client.Listener.EnqueuePacket(PacketHeader.Aid, data);
            }
        }
    }
}
