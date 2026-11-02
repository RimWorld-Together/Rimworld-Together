using RTServer.Hooks.TCPNetwork;
using RTServer.Managers;
using RTShared.Commands;
using RTShared.Misc;
using RTNetwork.Components;
using RTServer.PacketManagers;
using RTShared.Files.Player;

namespace RTServer.Commands
{
    public class CMD_ResetPlayer : CMD_Base
    {
        public CMD_ResetPlayer()
        {
            Prefix = "resetplayer";
            Description = "Resets the selected player";
            ParameterCount = 1;
        }

        public override void Action()
        {
            FL_Player userFile = UserManager.GetUserFileFromName(CMD_Base.CommandParameters[0]);
            if (userFile == null) Printer.Warning($"User '{CMD_Base.CommandParameters[0]}' was not found");
            else
            {
                ServerClient toFind = ServerNetwork.GetConnectedClientFromUsername(userFile.Username);
                PM_Saves.ResetPlayerData(toFind, userFile.Username);
            }
        }
    }
}
