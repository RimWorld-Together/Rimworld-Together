using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTServer.Misc;
using RTShared.Files;
using RTShared.Files.Default;
using RTShared.Files.Guilds;
using RTShared.Files.Player;
using RTShared.Misc;
using static RTNetwork.Packets.PKT_PlayerGuild;
using static RTShared.Files.Guilds.GuildMember;

namespace RTServer.PacketManagers
{

    public class PM_Guilds : PM_Base
    {
        [HandlesPacket(PacketHeader.Guild)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!FL_PlayerCooldown.CheckIfCanGuild(client.GetData<FL_Player>(), Master.ActionConfigs.GuildAction)) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                PKT_PlayerGuild data = Serializer.ConvertBytesToObject<PKT_PlayerGuild>(bytes);

                switch (data.StepMode)
                {
                    case GuildStepMode.Create:
                        Create(client, data);
                        break;

                    case GuildStepMode.Delete:
                        Delete(client, data);
                        break;

                    case GuildStepMode.Invite:
                        InviteMember(client, data);
                        break;

                    case GuildStepMode.AddMember:
                        AddMember(client, data);
                        break;

                    case GuildStepMode.RemoveMember:
                        RemoveMember(client, data);
                        break;

                    case GuildStepMode.Promote:
                        PromoteMember(client, data);
                        break;

                    case GuildStepMode.Demote:
                        DemoteMember(client, data);
                        break;

                    case GuildStepMode.Information:
                        SendGuildInformation(client, data);
                        break;
                }

                client.GetData<FL_Player>().Cooldowns.SetGuildTimer(client.GetData<FL_Player>());
            }
        }

        private static void Create(ServerClient client, PKT_PlayerGuild packet)
        {
            if (GuildManagerH.CheckIfGuildExistsByName(packet.Guild.Name))
            {
                packet.StepMode = GuildStepMode.NameInUse;
                client.Listener.EnqueuePacket(PacketHeader.Guild, packet);
            }
            
            else if (!GuildManagerH.CheckIfGuildNameLegal(packet.Guild.Name))
            {
                packet.StepMode = GuildStepMode.IllegalName;
                client.Listener.EnqueuePacket(PacketHeader.Guild, packet);
            }
            
            else
            {
                packet.Guild.AddMember(new GuildMember()
                {
                    Username = client.GetData<FL_Player>().Username,
                    Rank = GuildMember.GuildRanks.Admin
                });

                client.GetData<FL_Player>().UpdateGuild(packet.Guild);

                foreach (FL_Site site in PM_Sites.GetAllSitesFromUsername(client.GetData<FL_Player>().Username)) site.UpdateGuild(packet.Guild);

                client.Listener.EnqueuePacket(PacketHeader.Guild, packet);

                InformationDisplayer.DisplayAddGuild(packet.Guild.Name);
            }
        }

        private static void Delete(ServerClient client, PKT_PlayerGuild packet)
        {
            FL_Guild guild = GuildManagerH.GetGuildFromName(client.GetData<FL_Player>().GuildName);

            if (GuildManagerH.GetMemberRank(guild, client.GetData<FL_Player>().Username) != GuildRanks.Admin) ResponseShortcutManager.SendNoPowerPacket(client);
            else
            {
                foreach (FL_Player userFile in GuildManagerH.GetUsersFromGuildMembers(guild)) userFile.UpdateGuild(null);

                foreach (FL_Site site in GuildManagerH.GetGuildSites(guild)) site.UpdateGuild(null);

                packet.StepMode = GuildStepMode.Delete;
                foreach (ServerClient toUpdateConnected in GuildManagerH.GetConnectedGuildMembers(guild))
                {
                    toUpdateConnected.GetData<FL_Player>().UpdateGuild(null);
                    toUpdateConnected.Listener.EnqueuePacket(PacketHeader.Guild, packet);
                }

                guild.Delete();

                InformationDisplayer.DisplayRemoveGuild(guild.Name);
            }
        }

        private static void InviteMember(ServerClient client, PKT_PlayerGuild packet)
        {
            FL_Guild guild = GuildManagerH.GetGuildFromName(client.GetData<FL_Player>().GuildName);
            FL_Settlement settlement = PM_Settlements.GetSettlementFileFromTile(packet.DataInt);
            ServerClient toAdd = ServerNetwork.GetConnectedClientFromUsername(settlement.Username);

            if (toAdd == null) ResponseShortcutManager.SendUnavailablePacket(client);
            else if (toAdd.GetData<FL_Player>().GuildName != null) ResponseShortcutManager.SendUnavailablePacket(client);
            else if (GuildManagerH.GetMemberRank(guild, client.GetData<FL_Player>().Username) == GuildRanks.Member) ResponseShortcutManager.SendNoPowerPacket(client);
            else
            {
                packet.Guild = guild;
                toAdd.Listener.EnqueuePacket(PacketHeader.Guild, packet);
            }
        }

        private static void AddMember(ServerClient client, PKT_PlayerGuild packet)
        {
            FL_Guild guild = GuildManagerH.GetGuildFromName(packet.Guild.Name);

            if (!GuildManagerH.CheckIfUserIsInGuild(guild, client.GetData<FL_Player>().Username))
            {
                GuildMember member = new GuildMember();
                member.Username = client.GetData<FL_Player>().Username;
                member.Rank = GuildRanks.Member;
                guild.AddMember(member);

                client.GetData<FL_Player>().UpdateGuild(guild);

                foreach (FL_Site site in PM_Sites.GetAllSitesFromUsername(client.GetData<FL_Player>().Username)) site.UpdateGuild(guild);
            }
        }

        private static void RemoveMember(ServerClient client, PKT_PlayerGuild packet)
        {
            FL_Guild guild = GuildManagerH.GetGuildFromName(client.GetData<FL_Player>().GuildName);
            FL_Settlement settlement = PM_Settlements.GetSettlementFileFromTile(packet.DataInt);
            FL_Player toRemoveOffline = UserManagerH.GetUserFileFromName(settlement.Username);
            ServerClient toRemoveOnline = ServerNetwork.GetConnectedClientFromUsername(settlement.Username);

            GuildRanks userRank = GuildManagerH.GetMemberRank(guild, client.GetData<FL_Player>().Username);

            if (settlement.Username == client.GetData<FL_Player>().Username)
            {
                if (userRank != GuildRanks.Admin) Remove();
                else
                {
                    packet.StepMode = GuildStepMode.AdminProtection;
                    client.Listener.EnqueuePacket(PacketHeader.Guild, packet);
                }
            }

            else
            {
                if (userRank == GuildRanks.Member || userRank == GuildRanks.Moderator) ResponseShortcutManager.SendNoPowerPacket(client);
                else Remove();
            }

            void Remove()
            {
                if (toRemoveOnline != null)
                {
                    toRemoveOnline.GetData<FL_Player>().UpdateGuild(null);
                    toRemoveOnline.Listener.EnqueuePacket(PacketHeader.Guild, packet);
                }

                toRemoveOffline.UpdateGuild(null);

                guild.RemoveMember(guild.GuildMembers.First(fetch => fetch.Username == toRemoveOffline.Username));

                foreach (FL_Site site in PM_Sites.GetAllSitesFromUsername(toRemoveOffline.Username)) site.UpdateGuild(null);
            }
        }

        private static void PromoteMember(ServerClient client, PKT_PlayerGuild packet)
        {
            FL_Settlement settlement = PM_Settlements.GetSettlementFileFromTile(packet.DataInt);
            FL_Guild guild = GuildManagerH.GetGuildFromName(client.GetData<FL_Player>().GuildName);

            GuildRanks rank = GuildManagerH.GetMemberRank(guild, client.GetData<FL_Player>().Username);
            if (rank == GuildRanks.Member) ResponseShortcutManager.SendNoPowerPacket(client);
            else
            {
                FL_Player toPromoteOffline = UserManagerH.GetUserFileFromName(settlement.Username);
                if (GuildManagerH.GetMemberRank(guild, toPromoteOffline.Username) != GuildRanks.Member) ResponseShortcutManager.SendUnavailablePacket(client);
                else
                {
                    GuildMember member = GuildManagerH.GetAllGuildMembers(guild).First(fetch => fetch.Username == toPromoteOffline.Username);
                    guild.PromoteMember(member);

                    ServerClient toPromoteOnline = ServerNetwork.GetConnectedClientFromUsername(toPromoteOffline.Username);
                    if (toPromoteOnline != null) toPromoteOnline.Listener.EnqueuePacket(PacketHeader.Guild, packet);
                }
            }
        }

        private static void DemoteMember(ServerClient client, PKT_PlayerGuild packet)
        {
            FL_Settlement settlement = PM_Settlements.GetSettlementFileFromTile(packet.DataInt);
            FL_Guild guild = GuildManagerH.GetGuildFromName(client.GetData<FL_Player>().GuildName);

            GuildRanks rank = GuildManagerH.GetMemberRank(guild, client.GetData<FL_Player>().Username);
            if (rank != GuildRanks.Admin) ResponseShortcutManager.SendNoPowerPacket(client);
            else
            {
                FL_Player toDemoteOffline = UserManagerH.GetUserFileFromName(settlement.Username);
                if (GuildManagerH.GetMemberRank(guild, toDemoteOffline.Username) != GuildRanks.Moderator) ResponseShortcutManager.SendUnavailablePacket(client);
                else
                {
                    GuildMember member = GuildManagerH.GetAllGuildMembers(guild).First(fetch => fetch.Username == toDemoteOffline.Username);
                    guild.DemoteMember(member);

                    ServerClient toDemoteOnline = ServerNetwork.GetConnectedClientFromUsername(toDemoteOffline.Username);
                    if (toDemoteOnline != null) toDemoteOnline.Listener.EnqueuePacket(PacketHeader.Guild, packet);
                }
            }
        }

        private static void SendGuildInformation(ServerClient client, PKT_PlayerGuild packet)
        {
            packet.Guild = GuildManagerH.GetGuildFromName(client.GetData<FL_Player>().GuildName);
            client.Listener.EnqueuePacket(PacketHeader.Guild, packet);
        }
    }

    public class GuildManagerH
    {
        public static FL_Guild[] GetAllGuilds()
        {
            List<FL_Guild> files = new List<FL_Guild>();

            foreach (string file in Directory.GetFiles(Master.GuildsPath)) files.Add(Serializer.SerializeFromFile<FL_Guild>(file));

            return files.ToArray();
        }

        public static FL_Guild GetGuildFromName(string name) { return GetAllGuilds().FirstOrDefault(fetch => fetch.Name == name); }

        public static GuildMember[] GetAllGuildMembers(FL_Guild file) { return file.GuildMembers.ToArray(); }

        public static bool CheckIfUserIsInGuild(FL_Guild file, string username)
        {
            return GetAllGuildMembers(file).FirstOrDefault(fetch => fetch.Username == username) != null;
        }

        public static GuildRanks GetMemberRank(FL_Guild file, string username)
        {
            return GetAllGuildMembers(file).First(fetch => fetch.Username == username).Rank;
        }

        public static FL_Site[] GetGuildSites(FL_Guild file)
        {
            return PM_Sites.GetAllSites().Where(fetch => fetch.GuildName == file.Name).ToArray();
        }

        public static ServerClient[] GetConnectedGuildMembers(FL_Guild file)
        {
            return ServerNetwork.GetConnectedClients().Where(fetch => fetch.GetData<FL_Player>().GuildName == file.Name).ToArray();
        }

        public static FL_Player[] GetUsersFromGuildMembers(FL_Guild file)
        {
            return UserManagerH.GetAllUserFiles().Where(fetch => fetch.GuildName == file.Name).ToArray();
        }

        public static bool CheckIfGuildExistsByName(string nameToCheck)
        {
            return GetAllGuilds().FirstOrDefault(fetch => fetch.Name == nameToCheck) != null;
        }

        public static bool CheckIfGuildNameLegal(string guildName) { return StringChecker.CheckIfStringValid(guildName); }
    }
}
