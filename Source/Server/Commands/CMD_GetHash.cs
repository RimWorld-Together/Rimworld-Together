using RTServer.Managers;
using RTShared.Commands;
using RTShared.Files.Player;
using RTShared.Misc;

namespace RTServer.Commands
{
    public class CMD_GetHash : CMD_Base
    {
        public CMD_GetHash()
        {
            Prefix = "gethash";
            ParameterCount = 1;
            Description = "Returns the hash of a specified player";
        }

        public override void Action()
        {
            FL_Player file = UserManager.GetUserFileFromName(CMD_Base.CommandParameters[0]);
            if (file == null) Printer.Error($"Player '{CMD_Base.CommandParameters[0]}' doesn't exist");
            else Printer.Title($"Player '{CMD_Base.CommandParameters[0]}'s hash is '{file.Hash}'");
        }
    }
}