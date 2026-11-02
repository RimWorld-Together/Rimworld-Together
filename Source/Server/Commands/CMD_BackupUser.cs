using RTServer.Managers;
using RTShared.Commands;
using RTShared.Misc;
using RTShared.Files.Player;

namespace RTServer.Commands
{
    public class CMD_BackupUser : CMD_Base
    {
        public CMD_BackupUser()
        {
            Prefix = "backupuser";
            Description = "Backup the data of a specific user";
            ParameterCount = 1;
        }

        public override void Action() 
        {
            FL_Player userFile = UserManager.GetUserFileFromName(CMD_Base.CommandParameters[0]);
            if (userFile == null) Printer.Warning($"User '{CMD_Base.CommandParameters[0]}' was not found");
            else BackupManager.BackupUser(userFile.Username);
        }
    }
}
