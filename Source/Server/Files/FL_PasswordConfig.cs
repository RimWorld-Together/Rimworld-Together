using RTShared.Files;
using RTShared.Files.Default;

namespace RTServer.Files
{
    public class FL_PasswordConfig : FL_Base
    {
        public static string SavePath { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}