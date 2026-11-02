using RTServer.Managers;
using RTShared.Commands;
using RTShared.Misc;
using RTShared.Files.Player;

namespace RTServer.Commands
{
    public class CMD_BanList : CMD_Base
    {
        public CMD_BanList()
        {
            Prefix = "banlist";
            Description = "Shows a list of all banned server players";
            ParameterCount = 0;
        }

        public override void Action() 
        {
            FL_Player[] userFiles = UserManager.GetAllUserFiles().Where(x => x.IsBanned).ToArray();

            Printer.Title($"Banned players: [{userFiles.Count()}]");
            Printer.Title("----------------------------------------");
            foreach (FL_Player user in userFiles) Printer.Warning($"{user.Username} - {user.LatestIP}");
            Printer.Title("----------------------------------------");
        }
    }
}
