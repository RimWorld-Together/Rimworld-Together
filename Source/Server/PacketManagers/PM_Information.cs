using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTShared.Files.Default;
using RTShared.Misc;

namespace RTServer.PacketManagers
{
    public class PM_Information : PM_Base
    {
        [HandlesPacket(PacketHeader.Information)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            PKT_Information data = Serializer.ConvertBytesToObject<PKT_Information>(bytes);

            switch (data.StepMode)
            {
                case PKT_Information.InfoStepMode.Connection:
                    SendInformation(client, data);
                    break;

                case PKT_Information.InfoStepMode.Wealth:
                    SendWealth(client, data);
                    break;
            }
        }

        private static void SendInformation(ServerClient client, PKT_Information data)
        {
            FL_Settlement settlementToFind = PM_Settlements.GetSettlementFileFromTile(data.SettlementTile);
            ServerClient clientToFind = ServerNetwork.GetConnectedClientFromUsername(settlementToFind.Username);

            data.IsPlayerOnline = clientToFind != null ? true : false;

            client.Listener.EnqueuePacket(PacketHeader.Information, data);
        }

        private static void SendWealth(ServerClient client, PKT_Information data)
        {
            if (!PM_Map.CheckIfMapExists(data.SettlementTile)) ResponseShortcutManager.SendUnavailablePacket(client);
            else 
            {
                data.SettlementMapBytes = PM_Map.GetMapFromTile(data.SettlementTile);
                client.Listener.EnqueuePacket(PacketHeader.Information, data);
            }
        }
    }
}
