using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Files;
using RTShared.Misc;

namespace RTServer.PacketManagers
{
    public class PM_ServerPassword : PM_Base
    {
        [HandlesPacket(PacketHeader.ServerPassword)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header)
        {
            PKT_ServerPassword packet = Serializer.ConvertBytesToObject<PKT_ServerPassword>(bytes);
            OnPasswordSend(client, packet);
        }

        private void OnPasswordSend(ServerClient client, PKT_ServerPassword packet)
        {
            if (CheckForPassword(packet.ServerPassword)) PM_Login.TryLogin(client, packet.LoginPacket);
            else PM_Login.DenyConnectionWithReason(client, PKT_Login.LoginResponse.Password);
        }

        public static bool CheckIfPasswordIsSet() { return !string.IsNullOrEmpty(Master.PasswordConfig.Password); }
        
        private static bool CheckForPassword(string password) { return Master.PasswordConfig.Password == password; }
        
        public static void SetPassword(string password)
        {
            Master.PasswordConfig.Password = Hasher.GetHashFromString(password);
            FL_PasswordConfig.Save(FL_PasswordConfig.SavePath, Master.PasswordConfig);
        }

        public static void ClearPassword()
        {
            Master.PasswordConfig.Password = string.Empty;
            FL_PasswordConfig.Save(FL_PasswordConfig.SavePath, Master.PasswordConfig);
        }
        
        public static void AskForPassword(ServerClient client, PKT_Login login)
        {
            PKT_ServerPassword packet = new PKT_ServerPassword() { LoginPacket = login };
            client.Listener.EnqueuePacket(PacketHeader.ServerPassword, packet);
        }
    }
}