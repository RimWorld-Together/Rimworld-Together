using RTServer.Core;
using RTServer.Files;
using RTShared.Commands;
using RTShared.Files.Configs;
using RTShared.Misc;
using System.Text.RegularExpressions;

namespace RTServer.Managers
{
    public static class PasswordManager
    {
        public static void CheckFormat()
        {
			if ((Master.PasswordConfig.Password.Length == 64  && !Regex.IsMatch(Master.PasswordConfig.Password, "[^0-9A-F]")) || Master.PasswordConfig.Password.Length == 0) return;
			Printer.Title(Printer.SeparatorString);
			Printer.Error("The server password is set incorrectly. Please use 'clearpassword' to allow users to connect or 'setpassword' to set a password.");
		}
    }
}
