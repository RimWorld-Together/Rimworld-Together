using RTNetwork.Packets;
using RTShared.Misc;
using static RTNetwork.Packets.PKT_ResponseShortcut;
using RTNetwork.Components;
using RTShared.Files.Player;

namespace RTServer.Managers
{
    public static class ResponseShortcutManager
    {
        public static void SendIllegalPacket(ServerClient client, string message = "", bool shouldBroadcast = true)
        {
            PKT_ResponseShortcut data = new PKT_ResponseShortcut();
            data.StepMode = ResponseStepMode.IllegalAction;

            client.Listener.EnqueuePacket(PacketHeader.ResponseShortcut, data);
            client.Listener.MarkForDisconnect();

            if (shouldBroadcast)
            {
                Printer.Warning($"[Illegal action] > {client.GetData<FL_Player>().Username} > {client.IP}");
                Printer.Warning($"[Illegal reason] > {message}");
            }
        }

        public static void SendUserUnavailablePacket(ServerClient client)
        {
            PKT_ResponseShortcut data = new PKT_ResponseShortcut();
            data.StepMode = ResponseStepMode.UserUnavailable;

            client.Listener.EnqueuePacket(PacketHeader.ResponseShortcut, data);
        }

        public static void SendUnavailablePacket(ServerClient client)
        {
            PKT_ResponseShortcut data = new PKT_ResponseShortcut() { StepMode = ResponseStepMode.Unavailable };
            client.Listener.EnqueuePacket(PacketHeader.ResponseShortcut, data);
        }

        public static void SendBreakPacket(ServerClient client)
        {
            PKT_ResponseShortcut data = new PKT_ResponseShortcut();
            data.StepMode = ResponseStepMode.Pop;

            client.Listener.EnqueuePacket(PacketHeader.ResponseShortcut, data);
        }

        public static void SendNoPowerPacket(ServerClient client)
        {
            PKT_ResponseShortcut data = new PKT_ResponseShortcut();
            data.StepMode = ResponseStepMode.NoPower;

            client.Listener.EnqueuePacket(PacketHeader.ResponseShortcut, data);
        }
    }
}
