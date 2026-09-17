using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTServer.Misc;
using RTShared.Files;
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

        public static void AddSettlement(ServerClient client, PKT_PlayerSettlement packet)
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

        public static void RemoveSettlement(ServerClient client, PKT_PlayerSettlement settlementData)
        {
            if (!CheckIfTileIsInUse(settlementData.File.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData.File.Tile} was attempted to be removed, but the tile doesn't contain a settlement");

            FL_Settlement settlementFile = GetSettlementFileFromTile(settlementData.File.Tile);

            if (client != null)
            {
                if (settlementFile.Username != client.GetData<FL_Player>().Username)
                {
                    ResponseShortcutManager.SendIllegalPacket(client, $"Settlement at tile {settlementData.File.Tile} attempted to be removed by " +
                        $"{client.GetData<FL_Player>().Username}, but {settlementFile.Username} owns the settlement");
                }

                else
                {
                    Delete();
                    SendRemovalSignal();
                }
            }

            else
            {
                Delete();
                SendRemovalSignal();
            }

            void Delete()
            {
                File.Delete(Path.Combine(Master.SettlementsPath, settlementFile.Tile + CommonValues.DefaultSaveFormat));

                InformationDisplayer.DisplayRemoveSettlement(settlementFile.Tile.ToString());
            }

            void SendRemovalSignal()
            {
                settlementData.StepMode = PKT_PlayerSettlement.SettlementStepMode.Remove;

                ServerNetwork.SendPacketToAllClients(PacketHeader.Settlement, settlementData, client);
            }
        }

        public static bool CheckIfTileIsInUse(int tileToCheck)
        {
            string[] settlements = Directory.GetFiles(Master.SettlementsPath);
            foreach (string settlement in settlements)
            {
                FL_Settlement settlementJSON = Serializer.SerializeFromFile<FL_Settlement>(settlement);
                if (settlementJSON.Tile == tileToCheck) return true;
            }

            return false;
        }

        public static FL_Settlement GetSettlementFileFromTile(int tileToGet)
        {
            string[] settlements = Directory.GetFiles(Master.SettlementsPath);
            foreach (string settlement in settlements)
            {
                FL_Settlement settlementFile = Serializer.SerializeFromFile<FL_Settlement>(settlement);
                if (settlementFile.Tile == tileToGet) return settlementFile;
            }

            return null;
        }

        public static FL_Settlement GetSettlementFileFromUsername(string usernameToGet)
        {
            string[] settlements = Directory.GetFiles(Master.SettlementsPath);
            foreach (string settlement in settlements)
            {
                FL_Settlement settlementFile = Serializer.SerializeFromFile<FL_Settlement>(settlement);
                if (settlementFile.Username == usernameToGet) return settlementFile;
            }

            return null;
        }

        public static List<FL_Settlement> GetAllSettlements()
        {
            List<FL_Settlement> settlementList = new List<FL_Settlement>();
            string[] settlementPaths = Directory.GetFiles(Master.SettlementsPath);
            
            foreach (string path in settlementPaths)
            {
                try
                {
                    FL_Settlement file = Serializer.SerializeFromFile<FL_Settlement>(path);
                    FL_Player userFile = UserManagerH.GetUserFileFromName(file.Username);
                
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
            List<FL_Settlement> settlementList = new List<FL_Settlement>();

            foreach (FL_Settlement settlement in PM_Settlements.GetAllSettlements())
            {
                if (settlement.Username == username) settlementList.Add(settlement);
            }

            return settlementList.ToArray();
        }
    }
}
