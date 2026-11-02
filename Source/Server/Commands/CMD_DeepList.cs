using RTServer.Managers;
using RTShared.Commands;
using RTShared.Misc;
using RTShared.Files.Player;

namespace RTServer.Commands
{
    public class CMD_DeepList : CMD_Base
    {
        public CMD_DeepList()
        {
            Prefix = "deeplist";
            Description = "Shows a list of all server players";
        }

        public override void Action() 
        {
            FL_Player[] userFiles = UserManager.GetAllUserFiles();

            Printer.Title($"Server players: [{userFiles.Count()}]");
            Printer.Title("----------------------------------------");
            foreach (FL_Player user in userFiles) Printer.Warning($"{user.Username}");
            Printer.Title("----------------------------------------");
        }
    }
}
