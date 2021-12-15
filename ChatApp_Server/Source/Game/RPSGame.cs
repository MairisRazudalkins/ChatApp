using System;
using Packets;
using User;

namespace Server
{
    public enum RPSSelection 
    {
        Rock, 
        Paper,
        Scissors,
        Pending
    }

    public class RPSGame
    {
        ConnectedClient player1, player2;

        RPSSelection p1Selection = RPSSelection.Pending, p2Selection = RPSSelection.Pending;
        int p1Score = 0, p2Score = 0, gameId;
        bool p2Accepted = false, shouldClose = false;

        public RPSGame(ConnectedClient player1, ConnectedClient player2, int gameId) 
        {
            this.player1 = player1;
            this.player2 = player2;
            this.gameId = gameId;

            this.player2.SendPacket(new MsgPacket(player1.user.info.uniqueId, 0, new Message("Server", 0, string.Format("/invite {0} has invited you to play rock, paper, scissors", player1.user.info.name))));
        }

        public int GetId() { return gameId; }

        private void OnRecieveSelection(int senderId, string selection) 
        {
            RPSSelection selectionType = SelectionFromString(selection);

            if (player1.user.info.uniqueId == senderId && p1Selection == RPSSelection.Pending)
                p1Selection = selectionType;
            else if (player2.user.info.uniqueId == senderId && p2Selection == RPSSelection.Pending)
                p2Selection = selectionType;

            if (p1Selection != RPSSelection.Pending && p2Selection != RPSSelection.Pending)
            {
                UserInfo p1Info = player1.user.info, p2Info = player2.user.info;

                string winnerName = GetWinner(), resultMsg = string.IsNullOrEmpty(winnerName) ? string.Format("It was a draw!\nScore: {1} - {2} : {3} - {4}", winnerName, player1.user.info.name, p1Score, player2.user.info.name, p2Score) : string.Format("Winner was '{0}'\nScore: {1} - {2} : {3} - {4}", winnerName, player1.user.info.name, p1Score, player2.user.info.name, p2Score);

                player1.SendPacket(new MsgPacket(p2Info.uniqueId, 0, new Message(p2Info.name, p2Info.uniqueId, "/" + SelectionToString(p2Selection))));
                player2.SendPacket(new MsgPacket(p1Info.uniqueId, 0, new Message(p1Info.name, p1Info.uniqueId, "/" + SelectionToString(p1Selection))));

                player1.SendPacket(new MsgPacket(p2Info.uniqueId, 0, new Message("Server", 0, resultMsg)));
                player2.SendPacket(new MsgPacket(p1Info.uniqueId, 0, new Message("Server", 0, resultMsg)));

                p1Selection = RPSSelection.Pending;
                p2Selection = RPSSelection.Pending;
            }
        }

        public bool ShouldClose() 
        {
            return shouldClose;
        }

        public void ProccessCommand(int senderId, string command) 
        {
            if (!p2Accepted)
            {
                if (command == "/accept")
                {
                    if (!IsPlayer1(senderId))
                    {
                        p2Accepted = true;
                        player1.SendPacket(new MsgPacket(player2.user.info.uniqueId, 0, new Message("Server", 0, string.Format("/accepted {0} has accepted to play", player2.user.info.name))));
                    }
                }
                else if (command == "/decline")
                {
                    if (!IsPlayer1(senderId))
                    {
                        shouldClose = true;
                        player1.SendPacket(new MsgPacket(player2.user.info.uniqueId, 0, new Message("Server", 0, string.Format("/decined {0} has declined to play", player2.user.info.name))));
                    }
                }

                return;
            }

            OnRecieveSelection(senderId, command.Substring(1));
        }

        private bool IsPlayer1(int id) 
        {
            return player1.user.info.uniqueId == id ? true : false;
        }

        private RPSSelection SelectionFromString(string selection) 
        {
            return (selection = selection.ToLower()) == "rock" ? RPSSelection.Rock : selection == "paper" ? RPSSelection.Paper : selection == "scissors" ? RPSSelection.Scissors : RPSSelection.Pending;
        }

        private string SelectionToString(RPSSelection selection)
        {
            return selection == RPSSelection.Rock ? "rock" : selection == RPSSelection.Paper ? "paper" : selection == RPSSelection.Scissors ? "scissors" : null;
        }

        private string GetWinner() 
        {
            UserInfo p1Info = player1.user.info, p2Info = player2.user.info;

            if (p1Selection == p2Selection)
            {
                return null;
            }

            if ((p1Selection == RPSSelection.Rock && p2Selection == RPSSelection.Paper) || (p1Selection == RPSSelection.Paper && p2Selection == RPSSelection.Scissors) || (p1Selection == RPSSelection.Scissors && p2Selection == RPSSelection.Rock))
            {
                p2Score++;
                return p2Info.name;
            }

            p1Score++;
            return p1Info.name;
        }

        public bool DoesContainPlayers(int p1Id, int p2Id) 
        {
            return (player1.user.info.uniqueId == p1Id && player2.user.info.uniqueId == p2Id) || (player1.user.info.uniqueId == p2Id && player2.user.info.uniqueId == p1Id);
        }
    }
}
