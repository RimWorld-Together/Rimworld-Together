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

namespace RTServer.PacketManagers
{
    public class PM_Settlements : PM_Base
    {
        [HandlesPacket(PacketHeader.Settlement)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            PKT_PlayerSettlement data = Serializer.ConvertBytesToObject<PKT_PlayerSettlement>(bytes);

            switch (data.StepMode)
            {
                case PKT_PlayerSettlement.SettlementStepMode.Add:
                    AddSettlement(client, data);
                    break;

                case PKT_PlayerSettlement.SettlementStepMode.Remove:
                    RemoveSettlement(client, data);
                    break;
            }
        }

        private static void AddSettlement(ServerClient client, PKT_PlayerSettlement packet)
        {
            if (CheckIfTileIsInUse(packet.File.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.GetData<FL_Player>().Username} attempted to add a settlement at tile {packet.File.Tile}, but that tile already has a settlement");
            else
            {
                FL_Settlement settlementFile = new FL_Settlement();
                settlementFile.Tile = packet.File.Tile;
                settlementFile.Username = client.GetData<FL_Player>().Username;
                Serializer.SerializeToFile(Path.Combine(Master.SettlementsPath, settlementFile.Tile + CommonValues.DefaultSaveFormat), settlementFile);

                packet.StepMode = PKT_PlayerSettlement.SettlementStepMode.Add;
                packet.File = settlementFile;
                packet.File.IconID = client.GetData<FL_Player>().Customizations.SettlementIconID;
                packet.File.IconColor = client.GetData<FL_Player>().Customizations.SettlementIconColor;
                
                foreach (ServerClient cClient in ServerNetwork.GetConnectedClients())
                {
                    if (cClient == client) continue;
                    else cClient.Listener.EnqueuePacket(PacketHeader.Settlement, packet);
                }

                InformationDisplayer.DisplayAddSettlement(settlementFile.Tile.ToString());
            }
        }

        public static void RemoveSettlement(ServerClient client, PKT_PlayerSettlement packet)
        {
            FL_Settlement settlementFile = GetSettlementFileFromTile(packet.File.Tile);
            
            //Keep this here in case any mod tries to do some funky settlement removal patch
            if (!CheckIfTileIsInUse(packet.File.Tile)) return;
            else
            {
                //In case we're removing the settlement from a command, and we don't have a client
                if (client == null) DeleteAndSend();
                else
                {
                    if (settlementFile.Username == client.GetData<FL_Player>().Username) DeleteAndSend();
                    else ResponseShortcutManager.SendIllegalPacket(client, $"'{client.GetData<FL_Player>().Username}' attempted to remove non-owned settlement");
                }
            }

            void DeleteAndSend()
            {
                File.Delete(Path.Combine(Master.SettlementsPath, settlementFile.Tile + CommonValues.DefaultSaveFormat));
                InformationDisplayer.DisplayRemoveSettlement(settlementFile.Tile.ToString());
                
                packet.StepMode = PKT_PlayerSettlement.SettlementStepMode.Remove;
                ServerNetwork.SendPacketToAllClients(PacketHeader.Settlement, packet, client);
            }
        }

        public static bool CheckIfTileIsInUse(int tile)
        {
            string[] settlements = Directory.GetFiles(Master.SettlementsPath);
            return settlements.Select(Serializer.SerializeFromFile<FL_Settlement>).Any(settlement => settlement.Tile == tile);
        }

        public static FL_Settlement GetSettlementFileFromTile(int tile)
        {
            string[] settlements = Directory.GetFiles(Master.SettlementsPath);
            return settlements.Select(Serializer.SerializeFromFile<FL_Settlement>).FirstOrDefault(settlement => settlement.Tile == tile);
        }

        public static FL_Settlement GetSettlementFileFromUsername(string username)
        {
            string[] settlements = Directory.GetFiles(Master.SettlementsPath);
            return settlements.Select(Serializer.SerializeFromFile<FL_Settlement>).FirstOrDefault(settlement => settlement.Username == username);
        }

        public static List<FL_Settlement> GetAllSettlements()
        {
            List<FL_Settlement> settlementList = [];
            string[] settlementPaths = Directory.GetFiles(Master.SettlementsPath);
            
            foreach (string path in settlementPaths)
            {
                try
                {
                    FL_Settlement file = Serializer.SerializeFromFile<FL_Settlement>(path);
                    FL_Player userFile = UserManager.GetUserFileFromName(file.Username);
                
                    file.IconID = userFile.Customizations.SettlementIconID;
                    file.IconColor = userFile.Customizations.SettlementIconColor;
                    settlementList.Add(file);
                }
                catch (Exception ex) { Printer.Error(ex); }
            }

            return settlementList;
        }

        public static FL_Settlement[] GetAllSettlementsFromUsername(string username)
        {
            List<FL_Settlement> settlementList = [];

            foreach (FL_Settlement settlement in PM_Settlements.GetAllSettlements())
            {
                if (settlement.Username == username) settlementList.Add(settlement);
            }

            return settlementList.ToArray();
        }
    }
}
