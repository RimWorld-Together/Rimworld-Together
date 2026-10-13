using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTServer.Misc;
using RTShared.Files;
using RTShared.Files.Default;
using RTShared.Files.Player;
using RTShared.Misc;
using static RTNetwork.Packets.PKT_Site;

namespace RTServer.PacketManagers
{
    public class PM_Sites : PM_Base
    {
        [HandlesPacket(PacketHeader.Site)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!FL_PlayerCooldown.CheckIfCanSite(client.GetData<FL_Player>(), Master.ActionConfigs.SiteAction)) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                PKT_Site data = Serializer.ConvertBytesToObject<PKT_Site>(bytes);

                switch (data.StepMode)
                {
                    case SiteStepMode.Build:
                        AddNewSite(client, data);
                        break;

                    case SiteStepMode.Destroy:
                        DestroySite(client, data);
                        break;

                    case SiteStepMode.Rewards:
                        SendRewardsToPlayer(client);
                        break;

                    case SiteStepMode.Worker:
                        ManageWorker(client, data);
                        break;

                    case SiteStepMode.RetrieveWorker:
                        RetrieveWorker(client, data);
                        break;
                }

                client.GetData<FL_Player>().Cooldowns.SetSiteTimer(client.GetData<FL_Player>());
            }
        }

        private static void ConfirmNewSite(ServerClient client, FL_Site siteFile)
        {
            siteFile.SaveSite();
            siteFile.IconColor = client.GetData<FL_Player>().Customizations.SettlementIconColor;
            
            PKT_Site packet = new PKT_Site()
            {
                StepMode = SiteStepMode.Build,
                File = siteFile
            };

            foreach (ServerClient sc in ServerNetwork.GetConnectedClients()) sc.Listener.EnqueuePacket(PacketHeader.Site, packet);

            packet.StepMode = SiteStepMode.Accept;
            client.Listener.EnqueuePacket(PacketHeader.Site, packet);

            InformationDisplayer.DisplayAddSite(siteFile.Tile.ToString());
        }

        private static void AddNewSite(ServerClient client, PKT_Site siteData)
        {
            if (PM_Settlements.CheckIfTileIsInUse(siteData.File.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData.File.Tile}, but that tile already has a settlement");
            else if (CheckIfTileIsInUse(siteData.File.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData.File.Tile}, but that tile already has a site");
            else
            {
                FL_Site siteFile = new FL_Site();

                siteFile.Tile = siteData.File.Tile;
                siteFile.Username = client.GetData<FL_Player>().Username;
                if (!string.IsNullOrEmpty(client.GetData<FL_Player>().GuildName)) siteFile.GuildName = client.GetData<FL_Player>().GuildName;
                ConfirmNewSite(client, siteFile);
            }
        }

        private static void DestroySite(ServerClient client, PKT_Site siteData)
        {
            FL_Site siteFile = GetSiteFileFromTile(siteData.File.Tile);
            if (siteFile.Username == client.GetData<FL_Player>().Username) DestroySiteFromFile(siteFile);
            else ResponseShortcutManager.SendNoPowerPacket(client);
        }

        public static void DestroySiteFromFile(FL_Site siteFile)
        {
            PKT_Site siteData = new PKT_Site();
            siteData.StepMode = SiteStepMode.Destroy;
            siteData.File = siteFile;

            ServerNetwork.SendPacketToAllClients(PacketHeader.Site, siteData);

            File.Delete(Path.Combine(Master.SitesPath, siteFile.Tile + CommonValues.DefaultSaveFormat));

            InformationDisplayer.DisplayRemoveSite(siteFile.Tile.ToString());
        }

        private static void ManageWorker(ServerClient client, PKT_Site packet)
        {
            if (string.IsNullOrWhiteSpace(packet.File.WorkerString))
            {
                FL_Site siteFile = GetSiteFileFromTile(packet.File.Tile);
                packet.StepMode = SiteStepMode.Worker;
                packet.File = siteFile;

                client.Listener.EnqueuePacket(PacketHeader.Site, packet);
            }

            else
            {
                FL_Site site = GetSiteFileFromTile(packet.File.Tile);
                site.WorkerString = packet.File.WorkerString;
                site.SaveSite();
            }
        }

        private static void RetrieveWorker(ServerClient client, PKT_Site packet)
        {
            FL_Site site = GetSiteFileFromTile(packet.File.Tile);
            site.WorkerString = string.Empty;
            site.SaveSite();
        }

        public static void SendRewardsToEveryPlayer()
        {
            foreach (ServerClient client in ServerNetwork.GetConnectedClients())
            {
                SendRewardsToPlayer(client);
            }
        }

        public static void SendRewardsToPlayer(ServerClient client)
        {
            List<FL_Site> availableSites = GetPlayerRewardableSites(client);

            if (availableSites.Count > 0)
            {
                PKT_Site siteData = new PKT_Site();
                siteData.StepMode = SiteStepMode.Rewards;
                siteData.Files = availableSites;

                client.Listener.EnqueuePacket(PacketHeader.Site, siteData);
            }
        }

        private static List<FL_Site> GetPlayerRewardableSites(ServerClient client)
        {
            return GetAllSites().Where(fetch => (fetch.Username == client.GetData<FL_Player>().Username ||
                (client.GetData<FL_Player>().GuildName != null && client.GetData<FL_Player>().GuildName == fetch.GuildName)) &&
                    !string.IsNullOrEmpty(fetch.WorkerString)).ToList();
        }

        public static FL_Site[] GetAllSitesFromUsername(string username)
        {
            List<FL_Site> sitesList = new List<FL_Site>();
            
            foreach (FL_Site site in GetAllSites())
            {
                if (site.Username == username) sitesList.Add(site);
            }

            return sitesList.ToArray();
        }

        private static FL_Site GetSiteFileFromTile(int tileToGet) { return GetAllSites().FirstOrDefault(fetch => fetch.Tile == tileToGet); }

        public static List<FL_Site> GetAllSites()
        {
            List<FL_Site> sitesList = new List<FL_Site>();
            string[] sites = Directory.GetFiles(Master.SitesPath);
            
            foreach (string site in sites)
            {
                FL_Site file = Serializer.SerializeFromFile<FL_Site>(site);
                FL_Player userFile = UserManagerH.GetUserFileFromName(file.Username);
                    
                file.IconColor = userFile.Customizations.SettlementIconColor;
                sitesList.Add(file);
            }

            return sitesList;
        }

        private static bool CheckIfTileIsInUse(int tileToCheck)
        {
            string[] sites = Directory.GetFiles(Master.SitesPath);
            foreach (string site in sites)
            {
                FL_Site siteFile = Serializer.SerializeFromFile<FL_Site>(site);
                if (siteFile.Tile == tileToCheck) return true;
            }

            return false;
        }
    }
}
