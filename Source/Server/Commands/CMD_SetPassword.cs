using RTServer.PacketManagers;
using RTShared.Commands;
using RTShared.Misc;

namespace RTServer.Commands
{
    public class CMD_SetPassword : CMD_Base
    {
        public CMD_SetPassword()
        {
            Prefix = "setpassword";
            Description = "Sets the password to join this server. Leave empty for reset";
            ParameterCount = 1;
        }

        public override void Action()
        {
            if (string.IsNullOrWhiteSpace(CommandParameters[0])) PM_ServerPassword.ClearPassword();
            else PM_ServerPassword.SetPassword(CommandParameters[0]);
            Printer.Title("Password has been changed");
        }
    }
}
