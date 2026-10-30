using RTNetwork.Components;
using RTNetwork.PacketManagers;
using RTNetwork.Packets;
using RTServer.Core;
using RTServer.Managers;
using RTShared.Files.Default;
using RTShared.Files.Player;
using RTShared.Misc;

namespace RTServer.PacketManagers
{
    public class PM_Leaderboard : PM_Base
    {
        private static float ScoreMultiplier = 0.001f;

        [HandlesPacket(PacketHeader.Leaderboard)]
        public override void Receive(ServerClient client, byte[] bytes, PacketHeader header) 
        {
            if (!FL_PlayerCooldown.CheckIfCanLeaderboard(client.GetData<FL_Player>(), Master.ActionConfigs.LeaderboardAction)) ResponseShortcutManager.SendUnavailablePacket(client);
            else
            {
                SendLeaderboard(client);
                client.GetData<FL_Player>().Cooldowns.SetLeaderboardTimer(client.GetData<FL_Player>());
            }
        }

        private static void SendLeaderboard(ServerClient client)
        {
            PKT_Leaderboard data = new PKT_Leaderboard();
            data.File = (FL_Leaderboard)FL_Leaderboard.Load<FL_Leaderboard>(FL_Leaderboard.SavePath);
            client.Listener.EnqueuePacket(PacketHeader.Leaderboard, data);
        }

        public static void UpdateLeaderboard(ServerClient client, int wealth)
        {
            FL_Leaderboard file = (FL_Leaderboard)FL_Leaderboard.Load<FL_Leaderboard>(FL_Leaderboard.SavePath);
            double scoreValue = Math.Round(wealth * ScoreMultiplier) + 1;
            
            if (!file.Scores.Keys.Contains(client.GetData<FL_Player>().Username)) file.Scores.Add(client.GetData<FL_Player>().Username, scoreValue);
            else
            {
                foreach (KeyValuePair<string, double> pair in file.Scores.ToArray())
                {
                    if (pair.Key == client.GetData<FL_Player>().Username)
                    {
                        double currentScore = pair.Value;
                        file.Scores.Remove(pair.Key);
                        file.Scores.Add(client.GetData<FL_Player>().Username, currentScore + scoreValue);
                    }
                }
            }

            file.Scores = file.Scores.OrderByDescending(fetch => fetch.Value).ToDictionary(x => x.Key, x => x.Value);
            FL_Leaderboard.Save(FL_Leaderboard.SavePath, file);
        }
    }
}
