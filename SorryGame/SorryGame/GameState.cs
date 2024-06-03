using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace SorryGame
{
    //serializable class that saves the state of the game.
    [Serializable]
    class GameState
    {
        public List<Player> players { get; private set; }
        public List<Player> cpus { get; private set; }
        public List<Player> allplayers { get; private set; }
        public Player currentPlayer { get; private set; }
        public int playerNum { get; private set; }
        public Pawn currentPawn { get; private set; }
        public int movedSoFar { get; private set; }
        public int currentCardValue { get; private set; }
        public List<Card> deck { get; private set; }
        public Boolean movingFireToken { get; private set; }
        public Boolean movingIceToken { get; private set; }
        public Boolean swapping { get; private set; }
        public Boolean sorry { get; private set; }
        public Boolean split { get; private set; }
        public int splitValue1 { get; private set; }
        public int splitValue2 { get; private set; }
        public Pawn firstSelection { get; private set; }
        public Pawn specialNeedsPawn { get; private set; }

        //constructor that sets all of the properties, will be used to load from.
        public GameState(List<Player> players, List<Player> cpus, List<Player> allplayers, Player currentPlayer, int playerNum, Pawn currentPawn, int movedSoFar, int currentCardValue, List<Card> deck, Boolean movingFireToken, Boolean movingIceToken, Boolean swapping, Boolean sorry, Boolean split, int splitValue1, int splitValue2, Pawn firstSelection, Pawn specialNeedsPawn)
        {
            this.players = players;
            this.cpus = cpus;
            this.allplayers = allplayers;
            this.currentPlayer = currentPlayer;
            this.playerNum = playerNum;
            this.currentPawn = currentPawn;
            this.movedSoFar = movedSoFar;
            this.currentCardValue = currentCardValue;
            this.deck = deck;
            this.movingFireToken = movingFireToken;
            this.movingIceToken = movingIceToken;
            this.swapping = swapping;
            this.sorry = sorry;
            this.split = split;
            this.splitValue1 = splitValue1;
            this.splitValue2 = splitValue2;
            this.firstSelection = firstSelection;
            this.specialNeedsPawn = specialNeedsPawn;
        }
    }
}
