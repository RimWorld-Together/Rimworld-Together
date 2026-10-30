using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Managers;
using RTServer.Misc;
using RTShared.Files.Default;
using RTShared.Files.Player;
using RTShared.Misc;
using static RTNetwork.Packets.PKT_Save;

namespace RTServer.PacketManagers
{
    public class PM_Saves : PM_Base
    {
        [HandlesPacket(PacketHeader.Save)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            PKT_Save data = Serializer.ConvertBytesToObject<PKT_Save>(bytes);

            switch (data.StepMode)
            {
                case SaveStepMode.Receive:
                    ReceiveSaveFromClient(client, data);
                    break;

                case SaveStepMode.Reset:
                    ResetClientSave(client);
                    break;
            }
        }
       
        public static bool CheckIfUserHasSave(ServerClient client)
        {
            string[] saves = Directory.GetFiles(Master.SavesPath);
            foreach (string save in saves)
            {
                if (Path.GetFileNameWithoutExtension(save) == client.GetData<FL_Player>().Username) return true;
            }

            return false;
        }

        private static void ResetClientSave(ServerClient client)
        {
            if (!CheckIfUserHasSave(client))
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.GetData<FL_Player>().Username}'s save was attempted to be reset while the player doesn't have a save");
                return;
            }

            client.Listener.MarkForDisconnect();
            ResetPlayerData(client, client.GetData<FL_Player>().Username);
        }

        public static void ResetPlayerData(ServerClient client, string username)
        {
            BackupManager.BackupUser(username);

            if (client != null) client.Listener.MarkForDisconnect();

            // Delete save file
            string path = Path.Combine(Master.SavesPath, username + CommonValues.DefaultSaveFormat);
            if (File.Exists(path)) File.Delete(path);

            // Delete site files
            FL_Site[] playerSites = PM_Sites.GetAllSitesFromUsername(username);
            foreach (FL_Site site in playerSites) PM_Sites.DestroySiteFromFile(site);

            // Delete settlement files
            FL_Settlement[] playerSettlements = PM_Settlements.GetAllSettlementsFromUsername(username);
            foreach (FL_Settlement settlement in playerSettlements)
            {
                PKT_PlayerSettlement settlementData = new PKT_PlayerSettlement();
                settlementData.File.Tile = settlement.Tile;
                settlementData.File.Username = settlement.Username;

                PM_Settlements.RemoveSettlement(client, settlementData);
            }

            InformationDisplayer.DisplayResetPlayer(username);
        }

        public static void SendSaveToClient(ServerClient client)
        {
            string savePath = Path.Combine(Master.SavesPath, client.GetData<FL_Player>().Username + CommonValues.DefaultSaveFormat);

            PKT_Save data = new PKT_Save()
            {
                StepMode = SaveStepMode.Receive,
                FileBytes = File.ReadAllBytes(savePath),
                ForceUseSave = !Master.ServerConfig.UseClientSave
            };

            client.Listener.EnqueuePacket(PacketHeader.Save, data);
        }
        
        private static void ReceiveSaveFromClient(ServerClient client, PKT_Save data)
        {
            string savePath = Path.Combine(Master.SavesPath, client.GetData<FL_Player>().Username + CommonValues.DefaultSaveFormat);
            File.WriteAllBytes(savePath, data.FileBytes);

            InformationDisplayer.DisplaySaveGame(client);
            if (data.ForceDisconnect) client.Listener.MarkForDisconnect();
        }
    }
}
