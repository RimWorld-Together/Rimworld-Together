using RTServer.Core;
using RTNetwork.Packets;
using RTShared.Misc;
using RTServer.Hooks.TCPNetwork;
using static RTNetwork.Packets.PKT_Login;
using RTShared.Commands;
using RTNetwork.Components;
using RTServer.PacketManagers;
using RTShared.Files.Player;

namespace RTServer.Managers
{

    public static class UserManager
    {
        public static void SendPlayerRecount()
        {
            PKT_PlayerRecount playerRecountData = new PKT_PlayerRecount();
            playerRecountData.CurrentPlayerCount = ServerNetwork.GetConnectedClients().Count();
            ServerNetwork.SendPacketToAllClients(PacketHeader.PlayerRecount, playerRecountData);
        }

        public static void BanPlayerFromName(string username)
        {
            FL_Player userFile = UserManagerH.GetUserFileFromName(username);
            ServerClient client = ServerNetwork.GetConnectedClientFromUsername(username);
            if (userFile == null || client == null) Printer.Warning($"User '{CMD_Base.CommandParameters[0]}' was not found");
            else
            {
                if (userFile.IsBanned) Printer.Warning($"User '{userFile.Username}' is already banned from the server");
                else
                {
                    userFile.UpdateBan(true);
                    client.Listener.MarkForDisconnect();
                    Printer.Warning($"User '{userFile.Username}' has been banned from the server");
                }
            }
        }

        public static void PardonPlayerFromName(string username)
        {
            FL_Player userFile = UserManagerH.GetUserFileFromName(username);
            if (userFile == null) Printer.Warning($"User '{CMD_Base.CommandParameters[0]}' was not found");
            else
            {
                if (!userFile.IsBanned) Printer.Warning($"User '{userFile.Username}' is not banned from the server");
                else
                {
                    userFile.UpdateBan(false);
                    Printer.Warning($"User '{userFile.Username}' has been pardoned from the server");
                }
            }
        }
    }

    public static class UserManagerH
    {
        public static FL_Player GetUserFile(ServerClient client)
        {
            string[] userFiles = Directory.GetFiles(Master.UsersPath);

            foreach (string userFile in userFiles)
            {
                FL_Player file = Serializer.SerializeFromFile<FL_Player>(userFile);
                if (file.Username == client.GetData<FL_Player>().Username) return file;
            }

            return null;
        }

        public static FL_Player GetUserFileFromName(string username)
        {
            IEnumerable<FL_Player> files = Directory.GetFiles(Master.UsersPath).Select(Serializer.SerializeFromFile<FL_Player>);
            return files.FirstOrDefault(file => file.Username == username);
        }
        
        public static FL_Player GetUserFileFromHash(string hash)
        {
            IEnumerable<FL_Player> files = Directory.GetFiles(Master.UsersPath).Select(Serializer.SerializeFromFile<FL_Player>);
            return files.FirstOrDefault(file => file.Hash == hash);
        }

        public static FL_Player[] GetAllUserFiles()
        {
            string[] existingUsers = Directory.GetFiles(Master.UsersPath);
            return existingUsers.Select(Serializer.SerializeFromFile<FL_Player>).ToArray();
        }

        public static bool CheckIfUserIsConnected(string username)
        {
            ServerClient toGet = ServerNetwork.GetConnectedClientFromUsername(username);
            if (toGet != null) return true;
            else return false;
        }

        public static bool CheckIfUserExists(ServerClient client, PKT_Login data)
        {
            FL_Player toFind = GetAllUserFiles().FirstOrDefault(fetch => fetch.Username.ToLower() == data.Username.ToLower());
            if (toFind != null) return true;
            else return false;
        }

        public static bool CheckIfUserAuthCorrect(ServerClient client, PKT_Login data)
        {
            FL_Player toFind = GetAllUserFiles().FirstOrDefault(fetch => fetch.Username == data.Username && fetch.Password == data.Password);
            if (toFind != null) return true;
            else
            {
                PM_Login.DenyConnectionWithReason(client, LoginResponse.Invalid);
                return false;
            }
        }

        public static bool CheckIfUserBanned(ServerClient client)
        {
            if (client.GetData<FL_Player>().IsBanned || Master.IpBansConfig.CheckForIPBan(client.GetData<FL_Player>().LatestIP))
            {
                Printer.Message($"Banned user '{client.GetData<FL_Player>().Username}' tried to join the server");
                PM_Login.DenyConnectionWithReason(client, LoginResponse.Ban);
                return true;
            }

            else return false;
        }
    }
}
