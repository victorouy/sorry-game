using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace SorryGame
{
    //Holds information about Players, current actions, the deck of cards, the current cards
    public partial class MainWindow : Window
    {
        public List<Player> players;
        public List<Player> cpus;
        public List<Player> allplayers;
        public Player currentPlayer;
        public int playerNum = 0;
        public Pawn currentPawn;
        public int movedSoFar;
        public int currentCardValue;
        public List<Card> deck;
        public System.Timers.Timer timer;
        public TaskCompletionSource<bool> tcs = null;
        public Boolean movingFireToken = false;
        public Boolean movingIceToken = false;
        public Boolean swapping = false;
        public Boolean sorry = false;
        public Boolean split = false;
        public int splitValue1;
        public int splitValue2;
        public Pawn firstSelection = null;
        public Pawn specialNeedsPawn;
        int counttt = 0;

        // Basic constructor that starts our program and calls the start menu
        public MainWindow()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Window startWin = new StartMenu();
            startWin.Show();
            this.Close();
        }

        // Constructor that takes in as input all information from the previous game and set all values 
        public MainWindow(List<Player> players, List<Player> cpus, List<Player> allplayers, Player currentPlayer, int playerNum, Pawn currentPawn, int movedSoFar, int currentCardValue, List<Card> deck, Boolean movingFireToken, Boolean movingIceToken, Boolean swapping, Boolean sorry, Boolean split, int splitValue1, int splitValue2, Pawn firstSelection, Pawn specialNeedsPawn)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
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

            //draw all the pawns which were on the board
            foreach(Player player in allplayers)
            {
                foreach (Pawn pawn in player.pawns)
                {
                    drawPawn(pawn);
                }
            }
            timer = new System.Timers.Timer(500);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(tick);
            timer.Start();
        }

        //A constructor that takes an input of players and cpus from the setup window
        public MainWindow(List<Player> players, List<Player> cpus)
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.players = players;
            this.cpus = cpus;
            this.allplayers = new List<Player>();
            List<Player> templist = new List<Player>();
            foreach (Player realplayer in players)
            {
                templist.Add(realplayer);
            }
            foreach (Player cpuplayer in cpus)
            {
                templist.Add(cpuplayer);
            }

            // Set the order of the players turns using color to be clockwise
            int redindex = indexOfPlayer(templist, "red");
            int blueindex = indexOfPlayer(templist, "blue");
            int yellowindex = indexOfPlayer(templist, "yellow");
            int greenindex = indexOfPlayer(templist, "green");
            if (redindex != -1)
            {
                allplayers.Add(templist[redindex]);
            }
            if (blueindex != -1)
            {
                allplayers.Add(templist[blueindex]);
            }
            if (yellowindex != -1)
            {
                allplayers.Add(templist[yellowindex]);
            }
            if (greenindex != -1)
            {
                allplayers.Add(templist[greenindex]);
            }
            currentPlayer = this.allplayers[playerNum];
            setPawns(this.allplayers);
            deck = createDeck();
            shuffleDeck(deck);
            timer = new System.Timers.Timer(500);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(tick);
            timer.Start();
        }

        // Returns the index at which a player is given its color
        private int indexOfPlayer(List<Player> tempplayers, String color)
        {
            for (int i = 0; i < tempplayers.Count; i++)
            {
                if (tempplayers[i].color.Equals(color))
                {
                    return i;
                }
            }
            return -1;
        }

        private void tick(object sender, EventArgs e)
        {
            timer.Stop();
            Dispatcher.Invoke(new ThreadStart(() => playerTurn()));
        }

        private void RestartBtn_Click(object sender, RoutedEventArgs e)
        {
            Window win2 = new SetUpMenu();
            win2.Show();
            this.Close();
        }

        // Returns if it moving to a fire token will be valid or not
        private Boolean shouldMoveFire()
        {
            Boolean shouldMoveFire = true;
            foreach (Pawn pawn in currentPlayer.pawns)
            {
                if (pawn.hasFire && !pawn.hasIce && !pawn.isAtStart && !pawn.isSafe && !pawn.isAtHome)
                {
                    if ((pawn.currentGrid.Equals(LeftGrid.Name) && !(pawn.currentPosition == 0)) || (pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 0))
                    {
                        foreach (Pawn pawntemp in currentPlayer.pawns)
                        {
                            if (!(pawntemp.hasFire) && (pawntemp.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 0))
                            {
                                return false;
                            }
                        }
                    }
                    else if ((pawn.currentGrid.Equals(TopGrid.Name) && !(pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 0))
                    {
                        foreach (Pawn pawntemp in currentPlayer.pawns)
                        {
                            if (!(pawntemp.hasFire) && (pawntemp.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 14))
                            {
                                return false;
                            }
                        }
                    }
                    else if ((pawn.currentGrid.Equals(RightGrid.Name) && !(pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 14))
                    {
                        foreach (Pawn pawntemp in currentPlayer.pawns)
                        {
                            if (!(pawntemp.hasFire) && (pawntemp.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 14))
                            {
                                return false;
                            }
                        }
                    }
                    else if ((pawn.currentGrid.Equals(BottomGrid.Name) && !(pawn.currentPosition == 0)) || (pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 14))
                    {
                        foreach (Pawn pawntemp in currentPlayer.pawns)
                        {
                            if (!(pawntemp.hasFire) && (pawntemp.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 0))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return shouldMoveFire;
        }

        // Checks to see if a pawn movement will land on its own pawn
        public Boolean checkValid(int count, Pawn pawn)
        {
            Pawn pawnTemp = new Pawn(pawn.isAtStart, pawn.isAtHome, pawn.isSafe, pawn.hasFire, pawn.hasIce, pawn.imageref, pawn.color, pawn.currentGrid, pawn.canMove);
            pawnTemp.currentGrid = pawn.currentGrid;
            pawnTemp.currentPosition = pawn.currentPosition;
            pawnTemp.movingTo = pawn.movingTo;
            pawnTemp.movingToGrid = pawn.movingToGrid;

            currentPawn = pawn;
            MoveForward();

            if (!validateMove(pawn))
            {
                currentPlayer.pawns[count] = pawnTemp;
                movedSoFar = 0;
                currentPawn = null;
                return false;
            }
            else
            {
                return true;
            }
        }

        public Boolean checkValidBack(int count, Pawn pawn)
        {
            Pawn pawnTemp = new Pawn(pawn.isAtStart, pawn.isAtHome, pawn.isSafe, pawn.hasFire, pawn.hasIce, pawn.imageref, pawn.color, pawn.currentGrid, pawn.canMove);
            pawnTemp.currentGrid = pawn.currentGrid;
            pawnTemp.currentPosition = pawn.currentPosition;
            pawnTemp.movingTo = pawn.movingTo;
            pawnTemp.movingToGrid = pawn.movingToGrid;

            currentPawn = pawn;
            MoveBackward();

            if (!validateMove(pawn))
            {
                currentPlayer.pawns[count] = pawnTemp;
                movedSoFar = 0;
                currentPawn = null;
                return false;
            }
            else
            {
                return true;
            }
        }

        // This is the method that defines the movement and turn of CPU/AI
        private void cpuMove()
        {
            // Draws card
            CardImg_MouseLeftButtonDown(this, null);

            // Moves fire pawn to the corner fire token with checks
            if (shouldMoveFire())
            {
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (pawn.hasFire && !pawn.hasIce && !pawn.isAtStart && !pawn.isSafe && !pawn.isAtHome)
                    {
                        if (currentPlayer.color.Equals("red"))
                        {
                            if (!(pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 0) && !(pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 13) && !(pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 14))
                            {
                                moveFirePawn(pawn);
                                break;
                            }
                        }
                        else if (currentPlayer.color.Equals("blue"))
                        {
                            if (!(pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 0) && !(pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 1) && !(pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 0))
                            {
                                moveFirePawn(pawn);
                                break;
                            }
                        }
                        else if (currentPlayer.color.Equals("yellow"))
                        {
                            if (!(pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 14) && !(pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 1) && !(pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 0))
                            {
                                moveFirePawn(pawn);
                                break;
                            }
                        }
                        else if (currentPlayer.color.Equals("green"))
                        {
                            if (!(pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 14) && !(pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 13) && !(pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 14))
                            {
                                moveFirePawn(pawn);
                                break;
                            }
                        }
                    }
                }
            }

            // When draws card 1, moves ice token to opponent pawn
            if (currentCardValue == 1)
            {
                if (currentPlayer.color.Equals("red"))
                {
                    Boolean alreadyMoved = false;
                    foreach (Image img in BottomGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            Boolean checkIfOwnPawn = false;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    checkIfOwnPawn = true;
                                }
                            }
                            if (!checkIfOwnPawn)
                            {
                                moveIceToken();
                                ImageClickEvent(img, null);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in RightGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in TopGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in LeftGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (currentPlayer.color.Equals("blue"))
                {
                    Boolean alreadyMoved = false;
                    foreach (Image img in LeftGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            Boolean checkIfOwnPawn = false;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    checkIfOwnPawn = true;
                                }
                            }
                            if (!checkIfOwnPawn)
                            {
                                moveIceToken();
                                ImageClickEvent(img, null);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in BottomGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in RightGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in TopGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (currentPlayer.color.Equals("yellow"))
                {
                    Boolean alreadyMoved = false;
                    foreach (Image img in TopGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            Boolean checkIfOwnPawn = false;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    checkIfOwnPawn = true;
                                }
                            }
                            if (!checkIfOwnPawn)
                            {
                                moveIceToken();
                                ImageClickEvent(img, null);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in LeftGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in BottomGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in RightGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (currentPlayer.color.Equals("green"))
                {
                    Boolean alreadyMoved = false;
                    foreach (Image img in RightGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            Boolean checkIfOwnPawn = false;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    checkIfOwnPawn = true;
                                }
                            }
                            if (!checkIfOwnPawn)
                            {
                                moveIceToken();
                                ImageClickEvent(img, null);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in TopGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in LeftGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in BottomGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                Boolean checkIfOwnPawn = false;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        checkIfOwnPawn = true;
                                    }
                                }
                                if (!checkIfOwnPawn)
                                {
                                    moveIceToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                int count = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        if (currentPlayer.color.Equals("red"))
                        {
                            if ((pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 13) || pawn.currentGrid.Equals(RedHomeGrid.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(RedHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("blue"))
                        {
                            if ((pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 1) || pawn.currentGrid.Equals(BlueHomeGrid.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(BlueHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("yellow"))
                        {
                            if ((pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 1) || pawn.currentGrid.Equals(YellowHomeGrid.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(YellowHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("green"))
                        {
                            if ((pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 13) || pawn.currentGrid.Equals(GreenHomeGrid.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(GreenHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    count++;
                }
            }
            
            // If draw card 10, it will decide to move 10 if more profitable or 1 
            else if (currentCardValue == 10)
            {
                int count = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        if (currentPlayer.color.Equals("red"))
                        {
                            if ((pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 12))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4 || pawn.currentPosition == 5 || pawn.currentPosition == 7))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) && !(pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 1 || pawn.currentPosition == 0)) && !(pawn.currentGrid.Equals(RedHomeGrid.Name)) && !(pawn.currentGrid.Equals(RedHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("blue"))
                        {
                            if ((pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 2))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4 || pawn.currentPosition == 5 || pawn.currentPosition == 7))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)) && !(pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 1 || pawn.currentPosition == 0)) && !(pawn.currentGrid.Equals(BlueHomeGrid.Name)) && !(pawn.currentGrid.Equals(BlueHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("yellow"))
                        {
                            if ((pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 2))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 7 || pawn.currentPosition == 8 || pawn.currentPosition == 9 || pawn.currentPosition == 10 || pawn.currentPosition == 12))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)) && !(pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) && !(pawn.currentGrid.Equals(YellowHomeGrid.Name)) && !(pawn.currentGrid.Equals(YellowHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("green"))
                        {
                            if ((pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 12))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 7 || pawn.currentPosition == 8 || pawn.currentPosition == 9 || pawn.currentPosition == 10 || pawn.currentPosition == 12))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) && !(pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) && !(pawn.currentGrid.Equals(GreenHomeGrid.Name)) && !(pawn.currentGrid.Equals(GreenHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    count++;
                }
            }

            // When draw card 4, it will move backwards 4, unless it cannot then no if statement will be processed
            else if (currentCardValue == 4)
            {
                Boolean moved = false;
                int count = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        if (currentPlayer.color.Equals("red"))
                        {
                            if ((pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 9) || (pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 10) || (pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 11) || (pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 12))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    moved = true;
                                    break;
                                }
                            }

                            else if (!pawn.currentGrid.Equals(RedHome.Name) && !pawn.currentGrid.Equals(RedStartGrid.Name))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    moved = true;
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("blue"))
                        {
                            if ((pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 2) || (pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 3) || (pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 4) || (pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 5))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    moved = true;
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(BlueHome.Name) && !pawn.currentGrid.Equals(BlueStartGrid.Name))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    moved = true;
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("yellow"))
                        {
                            if ((pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 2) || (pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 3) || (pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 4) || (pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 5))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    moved = true;
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(YellowHome.Name) && !pawn.currentGrid.Equals(YellowStartGrid.Name))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    moved = true;
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("green"))
                        {
                            if ((pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 9) || (pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 10) || (pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 11) || (pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 12))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    moved = true;
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(GreenHome.Name) && !pawn.currentGrid.Equals(GreenStartGrid.Name))
                            {
                                if (checkValidBack(count, pawn))
                                {
                                    moved = true;
                                    break;
                                }
                            }
                        }
                    }
                    count++;
                }

                if (!moved)
                {
                    currentPawn = null;
                }
            }

            // If draws card 2, it will place fire token on own pawn and move
            else if (currentCardValue == 2)
            {
                if (currentPlayer.color.Equals("red"))
                {
                    Boolean alreadyMoved = false;
                    foreach (Image img in RedStartGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (pawn.hasFire && (pawn.currentGrid.Equals(RedHomeGrid.Name) || (pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 0) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14))))
                                {
                                    moveFireToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    foreach (Pawn pawnCpu in currentPlayer.pawns)
                    {
                        if ((pawnCpu.hasFire && pawnCpu.currentGrid.Equals(RightGrid.Name) && pawnCpu.currentPosition == 14) || (pawnCpu.hasFire && pawnCpu.currentGrid.Equals(LeftGrid.Name) && pawnCpu.currentPosition == 0) || (pawnCpu.hasFire && pawnCpu.currentGrid.Equals(TopGrid.Name) && pawnCpu.currentPosition == 14))
                        {
                            alreadyMoved = true;
                        }
                    }

                    if (alreadyMoved == false)
                    {
                        int count = 0;
                        foreach (Image img in LeftGrid.Children)
                        {
                            if (count <= 11)
                            {
                                if (img.Source != null)
                                {
                                    BitmapImage mp = new BitmapImage();
                                    mp = img.Source as BitmapImage;
                                    foreach (Pawn pawn in currentPlayer.pawns)
                                    {
                                        if (mp.UriSource.Equals(pawn.imageref))
                                        {
                                            moveFireToken();
                                            ImageClickEvent(img, null);
                                            alreadyMoved = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            count++;
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in TopGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in RightGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in BottomGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        int count = 0;
                        foreach (Image img in LeftGrid.Children)
                        {
                            if (count > 11)
                            {
                                if (img.Source != null)
                                {
                                    BitmapImage mp = new BitmapImage();
                                    mp = img.Source as BitmapImage;
                                    foreach (Pawn pawn in currentPlayer.pawns)
                                    {
                                        if (mp.UriSource.Equals(pawn.imageref))
                                        {
                                            moveFireToken();
                                            ImageClickEvent(img, null);
                                            alreadyMoved = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            count++;
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in RedStartGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                moveFireToken();
                                ImageClickEvent(img, null);
                                break;
                            }
                        }
                    }
                }
                else if (currentPlayer.color.Equals("blue"))
                {
                    Boolean alreadyMoved = false;
                    foreach (Image img in BlueStartGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (pawn.hasFire && (pawn.currentGrid.Equals(BlueHomeGrid.Name) || (pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 0) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1))))
                                {
                                    moveFireToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    foreach (Pawn pawnCpu in currentPlayer.pawns)
                    {
                        if ((pawnCpu.hasFire && pawnCpu.currentGrid.Equals(BottomGrid.Name) && pawnCpu.currentPosition == 0) || (pawnCpu.hasFire && pawnCpu.currentGrid.Equals(RightGrid.Name) && pawnCpu.currentPosition == 14) || (pawnCpu.hasFire && pawnCpu.currentGrid.Equals(TopGrid.Name) && pawnCpu.currentPosition == 14))
                        {
                            alreadyMoved = true;
                        }
                    }

                    if (alreadyMoved == false)
                    {
                        int count = 0;
                        foreach (Image img in TopGrid.Children)
                        {
                            if (count > 2)
                            {
                                if (img.Source != null)
                                {
                                    BitmapImage mp = new BitmapImage();
                                    mp = img.Source as BitmapImage;
                                    foreach (Pawn pawn in currentPlayer.pawns)
                                    {
                                        if (mp.UriSource.Equals(pawn.imageref))
                                        {
                                            moveFireToken();
                                            ImageClickEvent(img, null);
                                            alreadyMoved = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            count++;
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in RightGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in BottomGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in LeftGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        int count = 0;
                        foreach (Image img in TopGrid.Children)
                        {
                            if (count <= 2)
                            {
                                if (img.Source != null)
                                {
                                    BitmapImage mp = new BitmapImage();
                                    mp = img.Source as BitmapImage;
                                    foreach (Pawn pawn in currentPlayer.pawns)
                                    {
                                        if (mp.UriSource.Equals(pawn.imageref))
                                        {
                                            moveFireToken();
                                            ImageClickEvent(img, null);
                                            alreadyMoved = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            count++;
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in BlueStartGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                moveFireToken();
                                ImageClickEvent(img, null);
                                break;
                            }
                        }
                    }
                }
                else if (currentPlayer.color.Equals("yellow"))
                {
                    Boolean alreadyMoved = false;
                    foreach (Image img in YellowStartGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (pawn.hasFire && (pawn.currentGrid.Equals(YellowHomeGrid.Name) || (pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 14) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1))))
                                {
                                    moveFireToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    foreach (Pawn pawnCpu in currentPlayer.pawns)
                    {
                        if ((pawnCpu.hasFire && pawnCpu.currentGrid.Equals(BottomGrid.Name) && pawnCpu.currentPosition == 0) || (pawnCpu.hasFire && pawnCpu.currentGrid.Equals(LeftGrid.Name) && pawnCpu.currentPosition == 0) || (pawnCpu.hasFire && pawnCpu.currentGrid.Equals(RightGrid.Name) && pawnCpu.currentPosition == 14))
                        {
                            alreadyMoved = true;
                        }
                    }

                    if (alreadyMoved == false)
                    {
                        int count = 0;
                        foreach (Image img in RightGrid.Children)
                        {
                            if (count > 2)
                            {
                                if (img.Source != null)
                                {
                                    BitmapImage mp = new BitmapImage();
                                    mp = img.Source as BitmapImage;
                                    foreach (Pawn pawn in currentPlayer.pawns)
                                    {
                                        if (mp.UriSource.Equals(pawn.imageref))
                                        {
                                            moveFireToken();
                                            ImageClickEvent(img, null);
                                            alreadyMoved = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            count++;
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in BottomGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in LeftGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in TopGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        int count = 0;
                        foreach (Image img in RightGrid.Children)
                        {
                            if (count <= 2)
                            {
                                if (img.Source != null)
                                {
                                    BitmapImage mp = new BitmapImage();
                                    mp = img.Source as BitmapImage;
                                    foreach (Pawn pawn in currentPlayer.pawns)
                                    {
                                        if (mp.UriSource.Equals(pawn.imageref))
                                        {
                                            moveFireToken();
                                            ImageClickEvent(img, null);
                                            alreadyMoved = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            count++;
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in YellowStartGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                moveFireToken();
                                ImageClickEvent(img, null);
                                break;
                            }
                        }
                    }
                }
                else if (currentPlayer.color.Equals("green"))
                {
                    Boolean alreadyMoved = false;
                    foreach (Image img in GreenStartGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (pawn.hasFire && (pawn.currentGrid.Equals(GreenHomeGrid.Name) || (pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 14) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 14 || pawn.currentPosition == 13))))
                                {
                                    moveFireToken();
                                    ImageClickEvent(img, null);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                    }
                    foreach (Pawn pawnCpu in currentPlayer.pawns)
                    {
                        if ((pawnCpu.hasFire && pawnCpu.currentGrid.Equals(BottomGrid.Name) && pawnCpu.currentPosition == 0) || (pawnCpu.hasFire && pawnCpu.currentGrid.Equals(LeftGrid.Name) && pawnCpu.currentPosition == 0) || (pawnCpu.hasFire && pawnCpu.currentGrid.Equals(TopGrid.Name) && pawnCpu.currentPosition == 14))
                        {
                            alreadyMoved = true;
                        }
                    }

                    if (alreadyMoved == false)
                    {
                        int count = 0;
                        foreach (Image img in BottomGrid.Children)
                        {
                            if (count <= 11)
                            {
                                if (img.Source != null)
                                {
                                    BitmapImage mp = new BitmapImage();
                                    mp = img.Source as BitmapImage;
                                    foreach (Pawn pawn in currentPlayer.pawns)
                                    {
                                        if (mp.UriSource.Equals(pawn.imageref))
                                        {
                                            moveFireToken();
                                            ImageClickEvent(img, null);
                                            alreadyMoved = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            count++;
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in LeftGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in TopGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in RightGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        moveFireToken();
                                        ImageClickEvent(img, null);
                                        alreadyMoved = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        int count = 0;
                        foreach (Image img in BottomGrid.Children)
                        {
                            if (count > 11)
                            {
                                if (img.Source != null)
                                {
                                    BitmapImage mp = new BitmapImage();
                                    mp = img.Source as BitmapImage;
                                    foreach (Pawn pawn in currentPlayer.pawns)
                                    {
                                        if (mp.UriSource.Equals(pawn.imageref))
                                        {
                                            moveFireToken();
                                            ImageClickEvent(img, null);
                                            alreadyMoved = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            count++;
                        }
                    }
                    if (alreadyMoved == false)
                    {
                        foreach (Image img in GreenStartGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                moveFireToken();
                                ImageClickEvent(img, null);
                                break;
                            }
                        }
                    }
                }

                // After fire token, it moves pawn
                Boolean moved = false;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        if (currentPlayer.color.Equals("red"))
                        {
                            if ((pawn.currentGrid.Equals(RedStartGrid.Name)) && pawn.hasFire)
                            {
                                currentPawn = pawn;
                                MoveForward();
                                break;
                            }
                        }
                        else if (currentPlayer.color.Equals("blue"))
                        {
                            if ((pawn.currentGrid.Equals(BlueStartGrid.Name)) && pawn.hasFire)
                            {
                                currentPawn = pawn;
                                MoveForward();
                                moved = true;
                                break;
                            }
                        }
                        else if (currentPlayer.color.Equals("yellow"))
                        {
                            if ((pawn.currentGrid.Equals(YellowStartGrid.Name)) && pawn.hasFire)
                            {
                                currentPawn = pawn;
                                MoveForward();
                                moved = true;
                                break;
                            }
                        }
                        else if (currentPlayer.color.Equals("green"))
                        {
                            if ((pawn.currentGrid.Equals(GreenStartGrid.Name)) && pawn.hasFire)
                            {
                                currentPawn = pawn;
                                MoveForward();
                                moved = true;
                                break;
                            }
                        }
                    }
                }

                if (!moved)
                {
                    foreach (Pawn pawn in currentPlayer.pawns)
                    {
                        if (!pawn.hasIce)
                        {
                            if (currentPlayer.color.Equals("red"))
                            {
                                if ((pawn.currentGrid.Equals(RedStartGrid.Name) && pawn.currentPosition == 3) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                                else if ((pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 8) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 8 || pawn.currentPosition == 1)) || ((pawn.currentGrid.Equals(TopGrid.Name) || pawn.currentGrid.Equals(RightGrid.Name)) && (pawn.currentPosition == 13 || pawn.currentPosition == 6)))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                                else if (!pawn.currentGrid.Equals(RedHome.Name))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                            }
                            else if (currentPlayer.color.Equals("blue"))
                            {
                                if ((pawn.currentGrid.Equals(BlueStartGrid.Name) && pawn.currentPosition == 3) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 1 || pawn.currentPosition == 0)))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                                else if ((pawn.currentGrid.Equals(LeftGrid.Name) && pawn.currentPosition == 8) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 8 || pawn.currentPosition == 1)) || ((pawn.currentGrid.Equals(TopGrid.Name) || pawn.currentGrid.Equals(RightGrid.Name)) && (pawn.currentPosition == 13 || pawn.currentPosition == 6)))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                                else if (!pawn.currentGrid.Equals(BlueHome.Name))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                            }
                            else if (currentPlayer.color.Equals("yellow"))
                            {
                                if ((pawn.currentGrid.Equals(YellowStartGrid.Name) && pawn.currentPosition == 3) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 1 || pawn.currentPosition == 0)))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                                else if ((pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 6) || ((pawn.currentGrid.Equals(LeftGrid.Name) || pawn.currentGrid.Equals(BottomGrid.Name)) && (pawn.currentPosition == 8 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 6)))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                                else if (!pawn.currentGrid.Equals(YellowHome.Name))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                            }
                            else if (currentPlayer.color.Equals("green"))
                            {
                                if ((pawn.currentGrid.Equals(GreenStartGrid.Name) && pawn.currentPosition == 3) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                                else if ((pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 6) || ((pawn.currentGrid.Equals(LeftGrid.Name) || pawn.currentGrid.Equals(BottomGrid.Name)) && (pawn.currentPosition == 8 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 6 || pawn.currentPosition == 13)))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                                else if (!pawn.currentGrid.Equals(GreenHome.Name))
                                {
                                    currentPawn = pawn;
                                    MoveForward();
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            // Draws card 7, it will move out of 7 depending on which move is most profitable
            else if (currentCardValue == 7)
            {
                int count = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        if (currentPlayer.color.Equals("red"))
                        {
                            if ((pawn.currentGrid.Equals(RedHomeGrid.Name)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(TopGrid.Name)) && (pawn.currentPosition == 8 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 6)) || (pawn.currentGrid.Equals(BottomGrid) && pawn.currentPosition == 13))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(RedHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("blue"))
                        {
                            if ((pawn.currentGrid.Equals(BlueHomeGrid.Name)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(TopGrid.Name)) && (pawn.currentPosition == 8 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 6 || pawn.currentPosition == 13)) || (pawn.currentGrid.Equals(LeftGrid) && pawn.currentPosition == 13))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(BlueHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("yellow"))
                        {
                            if (pawn.currentGrid.Equals(YellowHomeGrid.Name) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 14 || pawn.currentPosition == 13 || pawn.currentPosition == 12 || pawn.currentPosition == 11 || pawn.currentPosition == 10)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if ((pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 1) || ((pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 6 || pawn.currentPosition == 13)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 1 || pawn.currentPosition == 8)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(YellowHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("green"))
                        {
                            if (pawn.currentGrid.Equals(GreenHomeGrid.Name) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 14 || pawn.currentPosition == 13 || pawn.currentPosition == 12 || pawn.currentPosition == 11 || pawn.currentPosition == 10)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if ((pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 1) || ((pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 6 || pawn.currentPosition == 13)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 1 || pawn.currentPosition == 8)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(GreenHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    count++;
                }
            }
            else if (currentCardValue == 11)
            {
                int count = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        if (currentPlayer.color.Equals("red"))
                        {
                            // To enter safe zone or home
                            if (pawn.currentGrid.Equals(RedHomeGrid.Name) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4 || pawn.currentPosition == 5 || pawn.currentPosition == 6 || pawn.currentPosition == 7 || pawn.currentPosition == 8)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            // To land on a slide
                            else if ((((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(TopGrid.Name)) && (pawn.currentPosition == 4 || pawn.currentPosition == 12)) || (pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 2 || pawn.currentPosition == 10)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            // To move a pawn on anything thats not in home
                            else if (!pawn.currentGrid.Equals(RedHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("blue"))
                        {
                            if (pawn.currentGrid.Equals(BlueHomeGrid.Name) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4 || pawn.currentPosition == 5 || pawn.currentPosition == 6 || pawn.currentPosition == 7 || pawn.currentPosition == 8)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if ((((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(TopGrid.Name)) && (pawn.currentPosition == 4 || pawn.currentPosition == 12)) || (pawn.currentGrid.Equals(BottomGrid.Name)) && (pawn.currentPosition == 2 || pawn.currentPosition == 10)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(BlueHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("yellow"))
                        {
                            if (pawn.currentGrid.Equals(YellowHomeGrid.Name) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 6 || pawn.currentPosition == 7 || pawn.currentPosition == 8 || pawn.currentPosition == 9 || pawn.currentPosition == 10 || pawn.currentPosition == 14 || pawn.currentPosition == 13 || pawn.currentPosition == 12 || pawn.currentPosition == 11)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if ((pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 2 || pawn.currentPosition == 10)) || (pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 2 || pawn.currentPosition == 10))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(YellowHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("green"))
                        {
                            if (pawn.currentGrid.Equals(GreenHomeGrid.Name) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 14 || pawn.currentPosition == 13 || pawn.currentPosition == 12 || pawn.currentPosition == 11 || pawn.currentPosition == 6 || pawn.currentPosition == 7 || pawn.currentPosition == 8 || pawn.currentPosition == 9 || pawn.currentPosition == 10)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if ((pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 2 || pawn.currentPosition == 10)) || (pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 2 || pawn.currentPosition == 10))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(GreenHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    count++;
                }
            }

            // Draw sorry card, will call sorry to opponent pawn, else will move 4
            else if (currentCardValue == 0)
            {
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (pawn.isAtStart && !pawn.hasIce)
                    {
                        sorry = true;
                        setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                    }
                }

                if (sorry)
                {
                    if (currentPlayer.color.Equals("red"))
                    {
                        foreach (Image img in RedStartGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                AiImageClickEvent(img);
                                break;
                            }
                        }
                    }
                    else if (currentPlayer.color.Equals("Blue"))
                    {
                        foreach (Image img in BlueStartGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                AiImageClickEvent(img);
                                break;
                            }
                        }
                    }
                    else if (currentPlayer.color.Equals("yellow"))
                    {
                        foreach (Image img in YellowStartGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                AiImageClickEvent(img);
                                break;
                            }
                        }
                    }
                    else if (currentPlayer.color.Equals("green"))
                    {
                        foreach (Image img in GreenStartGrid.Children)
                        {
                            if (img.Source != null)
                            {
                                AiImageClickEvent(img);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    currentCardValue = 4;
                    int count = 0;
                    foreach (Pawn pawn in currentPlayer.pawns)
                    {
                        if (!pawn.hasIce)
                        {
                            if (currentPlayer.color.Equals("red"))
                            {
                                // To enter safe zone or home
                                if ((pawn.currentGrid.Equals(RedHomeGrid.Name) && (pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                                // To land on a slide
                                else if ((pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 10) || ((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(TopGrid.Name)) && (pawn.currentPosition == 11 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 3)))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                                // To move a pawn on anything that's not in home
                                else if (!(pawn.currentGrid.Equals(RedHome.Name)))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (currentPlayer.color.Equals("blue"))
                            {
                                if ((pawn.currentGrid.Equals(BlueHomeGrid.Name) && (pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                                else if (((pawn.currentGrid.Equals(TopGrid.Name) || pawn.currentGrid.Equals(RightGrid.Name)) && (pawn.currentPosition == 11 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 10 || pawn.currentPosition == 3)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 10)))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                                else if (!(pawn.currentGrid.Equals(BlueHome.Name)))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (currentPlayer.color.Equals("yellow"))
                            {
                                if ((pawn.currentGrid.Equals(YellowHomeGrid.Name) && (pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 14 || pawn.currentPosition == 13)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                                else if ((pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 4) || ((pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 10 || pawn.currentPosition == 3)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 11 || pawn.currentPosition == 4)))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                                else if (!(pawn.currentGrid.Equals(YellowHome.Name)))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                            }
                            else if (currentPlayer.color.Equals("green"))
                            {
                                if ((pawn.currentGrid.Equals(GreenHomeGrid.Name) && (pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 14 || pawn.currentPosition == 13)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                                else if ((pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 4 || pawn.currentPosition == 11)) || ((pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 10 || pawn.currentPosition == 3)) || (pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 4))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                                else if (!(pawn.currentGrid.Equals(GreenHome.Name)))
                                {
                                    if (checkValid(count, pawn))
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        count++;
                    }
                }
            }

            // Move a pawn 3 spaces
            else if (currentCardValue == 3)
            {
                int count = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        if (currentPlayer.color.Equals("red"))
                        {
                            // To enter safe zone or home
                            if ((pawn.currentGrid.Equals(RedHomeGrid.Name) && (pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 0))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            // To land on a slide
                            else if ((pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 9) || ((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(TopGrid.Name)) && (pawn.currentPosition == 12 || pawn.currentPosition == 5)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 2)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            // To move a pawn on anything that's not on home
                            else if (!(pawn.currentGrid.Equals(RedHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("blue"))
                        {
                            if ((pawn.currentGrid.Equals(BlueHomeGrid.Name) && (pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 0)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (((pawn.currentGrid.Equals(TopGrid.Name) || pawn.currentGrid.Equals(RightGrid.Name)) && (pawn.currentPosition == 12 || pawn.currentPosition == 5)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 9 || pawn.currentPosition == 2)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 9)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(BlueHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("yellow"))
                        {
                            if ((pawn.currentGrid.Equals(YellowHomeGrid.Name) && (pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if ((pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 5) || ((pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 9 || pawn.currentPosition == 2)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 12 || pawn.currentPosition == 5)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(YellowHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("green"))
                        {
                            if ((pawn.currentGrid.Equals(GreenHomeGrid.Name) && (pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if ((pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 5 || pawn.currentPosition == 12)) || ((pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 9 || pawn.currentPosition == 2)) || (pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 5))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(GreenHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    count++;
                }
            }

            // Move pawn 5 spaces
            else if (currentCardValue == 5)
            {
                int count = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        if (currentPlayer.color.Equals("red"))
                        {
                            // To enter safe zone or home
                            if (pawn.currentGrid.Equals(RedHomeGrid.Name) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1 || pawn.currentPosition == 2)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            // To land on a slide
                            else if ((pawn.currentGrid.Equals(BottomGrid.Name) && pawn.currentPosition == 11) || ((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(TopGrid.Name)) && (pawn.currentPosition == 10 || pawn.currentPosition == 3)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 4)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            // To move a pawn on anything that's not in home
                            else if (!(pawn.currentGrid.Equals(RedHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("blue"))
                        {
                            if (pawn.currentGrid.Equals(BlueHomeGrid.Name) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1 || pawn.currentPosition == 2)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (((pawn.currentGrid.Equals(TopGrid.Name) || pawn.currentGrid.Equals(RightGrid.Name)) && (pawn.currentPosition == 10 || pawn.currentPosition == 3)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 11 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 11)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(BlueHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("yellow"))
                        {
                            if (pawn.currentGrid.Equals(YellowHomeGrid.Name) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 14 || pawn.currentPosition == 13 || pawn.currentPosition == 12)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if ((pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 3) || ((pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 11 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 10)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(YellowHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("green"))
                        {
                            if (pawn.currentGrid.Equals(GreenHomeGrid.Name) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 14 || pawn.currentPosition == 13 || pawn.currentPosition == 12)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if ((pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 3 || pawn.currentPosition == 10)) || ((pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 11 || pawn.currentPosition == 4)) || (pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 3))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(GreenHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    count++;
                }
            }

            // Move pawn 8 spaces
            else if (currentCardValue == 8)
            {
                int count = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        if (currentPlayer.color.Equals("red"))
                        {
                            // To enter safe zone or home
                            if ((pawn.currentGrid.Equals(RedHomeGrid.Name)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4 || pawn.currentPosition == 5)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            // To land on a slide
                            else if (((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(TopGrid.Name)) && (pawn.currentPosition == 7 || pawn.currentPosition == 0)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 7)) || (pawn.currentGrid.Equals(BottomGrid) && pawn.currentPosition == 14))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            // To move a pawn on anything that's not in home
                            else if (!(pawn.currentGrid.Equals(RedHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("blue"))
                        {
                            if ((pawn.currentGrid.Equals(BlueHomeGrid.Name)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4 || pawn.currentPosition == 5)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(TopGrid.Name)) && (pawn.currentPosition == 7 || pawn.currentPosition == 0)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 7 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(LeftGrid) && pawn.currentPosition == 14))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(BlueHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("yellow"))
                        {
                            if (pawn.currentGrid.Equals(YellowHomeGrid.Name) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 14 || pawn.currentPosition == 13 || pawn.currentPosition == 12 || pawn.currentPosition == 11 || pawn.currentPosition == 10 || pawn.currentPosition == 9)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if ((pawn.currentGrid.Equals(TopGrid.Name) && pawn.currentPosition == 0) || ((pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 7 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 7)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(YellowHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("green"))
                        {
                            if (pawn.currentGrid.Equals(GreenHomeGrid.Name) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 14 || pawn.currentPosition == 13 || pawn.currentPosition == 12 || pawn.currentPosition == 11 || pawn.currentPosition == 10 || pawn.currentPosition == 9)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if ((pawn.currentGrid.Equals(RightGrid.Name) && pawn.currentPosition == 0) || ((pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 7 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 7)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!(pawn.currentGrid.Equals(GreenHome.Name)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    count++;
                }
            }

            // Move pawn 12 spaces
            else if (currentCardValue == 12)
            {
                int count = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        if (currentPlayer.color.Equals("red"))
                        {
                            // To enter safe zone or home
                            if (pawn.currentGrid.Equals(RedHomeGrid.Name) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4 || pawn.currentPosition == 5 || pawn.currentPosition == 6 || pawn.currentPosition == 7 || pawn.currentPosition == 8 || pawn.currentPosition == 9)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            // To land on a slide
                            else if (((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(TopGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 3 || pawn.currentPosition == 11)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            // To move a pawn on anything that's not in home
                            else if (!pawn.currentGrid.Equals(RedHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("blue"))
                        {
                            if (pawn.currentGrid.Equals(BlueHomeGrid.Name) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(LeftGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1 || pawn.currentPosition == 2 || pawn.currentPosition == 3 || pawn.currentPosition == 4 || pawn.currentPosition == 5 || pawn.currentPosition == 6 || pawn.currentPosition == 7 || pawn.currentPosition == 8 || pawn.currentPosition == 9)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(TopGrid.Name) || pawn.currentGrid.Equals(BottomGrid.Name)) && (pawn.currentPosition == 11 || pawn.currentPosition == 3)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(BlueHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("yellow"))
                        {
                            if (pawn.currentGrid.Equals(YellowHomeGrid.Name) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 0 || pawn.currentPosition == 1)) || (pawn.currentGrid.Equals(TopGrid.Name) && (pawn.currentPosition == 5 || pawn.currentPosition == 6 || pawn.currentPosition == 7 || pawn.currentPosition == 8 || pawn.currentPosition == 9 || pawn.currentPosition == 10 || pawn.currentPosition == 14 || pawn.currentPosition == 13 || pawn.currentPosition == 12 || pawn.currentPosition == 11)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (((pawn.currentGrid.Equals(RightGrid.Name) || pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 3 || pawn.currentPosition == 11)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(YellowHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                        else if (currentPlayer.color.Equals("green"))
                        {
                            if (pawn.currentGrid.Equals(GreenHomeGrid.Name) || (pawn.currentGrid.Equals(BottomGrid.Name) && (pawn.currentPosition == 13 || pawn.currentPosition == 14)) || (pawn.currentGrid.Equals(RightGrid.Name) && (pawn.currentPosition == 14 || pawn.currentPosition == 13 || pawn.currentPosition == 12 || pawn.currentPosition == 11 || pawn.currentPosition == 5 || pawn.currentPosition == 6 || pawn.currentPosition == 7 || pawn.currentPosition == 8 || pawn.currentPosition == 9 || pawn.currentPosition == 10)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (((pawn.currentGrid.Equals(TopGrid.Name) || pawn.currentGrid.Equals(BottomGrid.Name) || pawn.currentGrid.Equals(LeftGrid.Name)) && (pawn.currentPosition == 3 || pawn.currentPosition == 11)))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                            else if (!pawn.currentGrid.Equals(GreenHome.Name))
                            {
                                if (checkValid(count, pawn))
                                {
                                    break;
                                }
                            }
                        }
                    }
                    count++;
                }
            }


            if (currentPawn != null)
            {
                if (validateMove(currentPawn))
                {
                    foreach (Pawn pawn in currentPlayer.pawns)
                    {
                        checkStart(pawn);
                        checkHomeGrid(pawn);
                        checkSafeGrid(pawn);
                        if (pawn != currentPawn)
                        {
                            pawn.currentGrid = pawn.movingToGrid;
                            pawn.currentPosition = pawn.movingTo;
                        }
                    }
                    drawPawn(currentPawn);
                    removePawn(currentPawn);
                    currentPawn.currentPosition = currentPawn.movingTo;
                    currentPawn.currentGrid = currentPawn.movingToGrid;
                    checkStart(currentPawn);
                    checkSafeGrid(currentPawn);
                    checkHomeGrid(currentPawn);
                    if (checkWin())
                    {
                        Window win = new YouWin(currentPlayer);
                        win.Show();
                        this.Close();
                    }
                    else
                    {
                        nextTurn();
                    }
                }
            }
            else
            { 
                nextTurn();
            }
        }

        // Ai version of ImageClickEvent by taking input an Image instead
        private void AiImageClickEvent(Image img)
        {
            Grid temp = (Grid)img.Parent;
            if (movingFireToken == false && movingIceToken == false && swapping == false && sorry == false && split == false)
            {
                if (temp.Name.Equals(TopGrid.Name) || temp.Name.Equals(BottomGrid.Name) || temp.Name.Equals(RedHomeGrid.Name) || temp.Name.Equals(YellowHomeGrid.Name) || temp.Name.Equals(BlueStartGrid.Name) || temp.Name.Equals(GreenStartGrid.Name))
                {
                    currentPawn = getPawnAtLocation(temp.Name, Grid.GetColumn(img));
                }
                else if (temp.Name.Equals(LeftGrid.Name) || temp.Name.Equals(RightGrid.Name) || temp.Name.Equals(BlueHomeGrid.Name) || temp.Name.Equals(GreenHomeGrid.Name) || temp.Name.Equals(RedStartGrid.Name) || temp.Name.Equals(YellowStartGrid.Name))
                {
                    currentPawn = getPawnAtLocation(temp.Name, Grid.GetRow(img));
                }
                cardCheck();
            }

            if (movingFireToken == true || movingIceToken == true || swapping == true || sorry == true || split == true)
            {
                if (temp.Name.Equals(TopGrid.Name) || temp.Name.Equals(BottomGrid.Name) || temp.Name.Equals(RedHomeGrid.Name) || temp.Name.Equals(YellowHomeGrid.Name) || temp.Name.Equals(BlueStartGrid.Name) || temp.Name.Equals(GreenStartGrid.Name))
                {
                    specialNeedsPawn = getPawnAtLocation(temp.Name, Grid.GetColumn(img));
                }
                else if (temp.Name.Equals(LeftGrid.Name) || temp.Name.Equals(RightGrid.Name) || temp.Name.Equals(BlueHomeGrid.Name) || temp.Name.Equals(GreenHomeGrid.Name) || temp.Name.Equals(RedStartGrid.Name) || temp.Name.Equals(YellowStartGrid.Name))
                {
                    specialNeedsPawn = getPawnAtLocation(temp.Name, Grid.GetRow(img));
                }
            }

            if (movingFireToken == true)
            {
                removeFirePawn();
                if (!specialNeedsPawn.hasIce)
                {
                    switch (specialNeedsPawn.color)
                    {
                        case "red":
                            specialNeedsPawn.imageref = new Uri("redfire.png", UriKind.Relative);
                            break;
                        case "blue":
                            specialNeedsPawn.imageref = new Uri("bluefire.png", UriKind.Relative);
                            break;
                        case "green":
                            specialNeedsPawn.imageref = new Uri("greenfire.png", UriKind.Relative);
                            break;
                        case "yellow":
                            specialNeedsPawn.imageref = new Uri("yellowfire.png", UriKind.Relative);
                            break;
                    }
                }
                img.Source = new BitmapImage(specialNeedsPawn.imageref);
                specialNeedsPawn.hasFire = true;
                movingFireToken = false;
                foreach (Player player in allplayers)
                {
                    foreach (Pawn pawn in player.pawns)
                    {
                        if (player != currentPlayer)
                        {
                            setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                        }
                        else if (pawn.hasFire)
                        {
                            setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                        }
                    }
                }
                tcs?.TrySetResult(true);
            }
            else if (movingIceToken == true)
            {
                removeIcePawn();
                switch (specialNeedsPawn.color)
                {
                    case "red":
                        specialNeedsPawn.imageref = new Uri("redice.png", UriKind.Relative);
                        break;
                    case "blue":
                        specialNeedsPawn.imageref = new Uri("blueice.png", UriKind.Relative);
                        break;
                    case "green":
                        specialNeedsPawn.imageref = new Uri("greenice.png", UriKind.Relative);
                        break;
                    case "yellow":
                        specialNeedsPawn.imageref = new Uri("yellowice.png", UriKind.Relative);
                        break;
                }
                img.Source = new BitmapImage(specialNeedsPawn.imageref);
                specialNeedsPawn.hasIce = true;
                movingIceToken = false;
                foreach (Player player in allplayers)
                {
                    foreach (Pawn pawn in player.pawns)
                    {
                        if (player != currentPlayer)
                        {
                            setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                        }
                        else if (pawn.hasIce)
                        {
                            setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                        }
                    }
                }
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                    }
                }
                tcs?.TrySetResult(true);
            }
            else if (sorry == true)
            {
                if (firstSelection == null)
                {
                    firstSelection = specialNeedsPawn;
                    AIsorryKnockOut();
                }
                else
                {
                    removePawn(firstSelection);
                    removePawn(specialNeedsPawn);
                    firstSelection.currentPosition = specialNeedsPawn.currentPosition;
                    firstSelection.currentGrid = specialNeedsPawn.currentGrid;
                    firstSelection.movingTo = specialNeedsPawn.movingTo;
                    firstSelection.movingToGrid = specialNeedsPawn.movingToGrid;
                    setBackToStart(specialNeedsPawn);
                    drawPawn(firstSelection);
                    checkStart(firstSelection);
                    firstSelection = null;
                    specialNeedsPawn = null;
                    sorry = false;
                    tcs?.TrySetResult(true);
                    nextTurn();
                }
            }
        }

        // When sorry, this method picks the opponent pawn to knock out
        private void AIsorryKnockOut()
        {
            foreach (Player player in allplayers)
            {
                foreach (Pawn pawn in player.pawns)
                {
                    if (player == currentPlayer)
                    {
                        setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                    }
                    else if (!(pawn.hasIce || pawn.isAtStart || pawn.isAtHome || pawn.isSafe))
                    {
                        setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                    }
                }
            }

            // Will compare the Uri to make sure the pawn it will knock out isnt the Cpu's own pawn
            Boolean alreadyMoved = false;
            if (currentPlayer.color.Equals("red"))
            {
                if (alreadyMoved == false)
                {
                    int count = 0;
                    foreach (Image img in LeftGrid.Children)
                    {
                        if (count >= 12)
                        {
                            if (img.Source != null)
                            {
                                Boolean check = true;
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        check = false;
                                    }
                                }
                                if (check)
                                {
                                    AiImageClickEvent(img);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                        count++;
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in BottomGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in RightGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in TopGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    int count = 0;
                    foreach (Image img in LeftGrid.Children)
                    {
                        if (count < 12)
                        {
                            if (img.Source != null)
                            {
                                Boolean check = true;
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        check = false;
                                    }
                                }
                                if (check)
                                {
                                    AiImageClickEvent(img);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                        count++;
                    }
                }
            }
            else if (currentPlayer.color.Equals("blue"))
            {
                if (alreadyMoved == false)
                {
                    int count = 0;
                    foreach (Image img in TopGrid.Children)
                    {
                        if (count <= 2)
                        {
                            if (img.Source != null)
                            {
                                Boolean check = true;
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        check = false;
                                    }
                                }
                                if (check)
                                {
                                    AiImageClickEvent(img);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                        count++;
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in LeftGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in BottomGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in RightGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    int count = 0;
                    foreach (Image img in TopGrid.Children)
                    {
                        if (count > 2)
                        {
                            if (img.Source != null)
                            {
                                Boolean check = true;
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        check = false;
                                    }
                                }
                                if (check)
                                {
                                    AiImageClickEvent(img);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                        count++;
                    }
                }
            }
            else if (currentPlayer.color.Equals("yellow"))
            {
                if (alreadyMoved == false)
                {
                    int count = 0;
                    foreach (Image img in RightGrid.Children)
                    {
                        if (count <= 2)
                        {
                            if (img.Source != null)
                            {
                                Boolean check = true;
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        check = false;
                                    }
                                }
                                if (check)
                                {
                                    AiImageClickEvent(img);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                        count++;
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in TopGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in LeftGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in BottomGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    int count = 0;
                    foreach (Image img in RightGrid.Children)
                    {
                        if (count > 2)
                        {
                            if (img.Source != null)
                            {
                                Boolean check = true;
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        check = false;
                                    }
                                }
                                if (check)
                                {
                                    AiImageClickEvent(img);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                        count++;
                    }
                }
            }
            else if (currentPlayer.color.Equals("green"))
            {
                if (alreadyMoved == false)
                {
                    int count = 0;
                    foreach (Image img in BottomGrid.Children)
                    {
                        if (count >= 12)
                        {
                            if (img.Source != null)
                            {
                                Boolean check = true;
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        check = false;
                                    }
                                }
                                if (check)
                                {
                                    AiImageClickEvent(img);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                        count++;
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in RightGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in TopGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    foreach (Image img in LeftGrid.Children)
                    {
                        if (img.Source != null)
                        {
                            Boolean check = true;
                            BitmapImage mp = new BitmapImage();
                            mp = img.Source as BitmapImage;
                            foreach (Pawn pawn in currentPlayer.pawns)
                            {
                                if (mp.UriSource.Equals(pawn.imageref))
                                {
                                    check = false;
                                }
                            }
                            if (check)
                            {
                                AiImageClickEvent(img);
                                alreadyMoved = true;
                                break;
                            }
                        }
                    }
                }
                if (alreadyMoved == false)
                {
                    int count = 0;
                    foreach (Image img in RightGrid.Children)
                    {
                        if (count < 12)
                        {
                            if (img.Source != null)
                            {
                                Boolean check = true;
                                BitmapImage mp = new BitmapImage();
                                mp = img.Source as BitmapImage;
                                foreach (Pawn pawn in currentPlayer.pawns)
                                {
                                    if (mp.UriSource.Equals(pawn.imageref))
                                    {
                                        check = false;
                                    }
                                }
                                if (check)
                                {
                                    AiImageClickEvent(img);
                                    alreadyMoved = true;
                                    break;
                                }
                            }
                        }
                        count++;
                    }
                }
            }
        }

        //Returns a Pawn if it is at a certain position in the grid
        private Pawn getPawnAtLocation(String grid, int RowCol)
        {
            List<Player> allplayers = new List<Player>();
            foreach (Player realplayer in players)
            {
                allplayers.Add(realplayer);
            }

            foreach (Player cpuplayer in cpus)
            {
                allplayers.Add(cpuplayer);
            }

            //For every pawn of every player check if the current grid and position match the grid and position given
            foreach (Player player in allplayers)
            {
                foreach (Pawn pawn in player.pawns)
                {
                    if (pawn.currentGrid.Equals(grid) && pawn.currentPosition == RowCol)
                    {
                        return pawn;
                    }
                }
            }
            return null;
        }

        //Sets the current players pawns unclickable and resets the values to change turns and calls the next players turn
        private void nextTurn()
        {
            foreach (Player player in players)
            {
                foreach (Pawn pawn in player.pawns)
                {
                    setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                    checkStart(pawn);
                    checkHomeGrid(pawn);
                    checkSafeGrid(pawn);
                }
            }
            if (playerNum == allplayers.Count - 1)
            {
                playerNum = 0;
                currentPlayer = allplayers[playerNum];
            }
            else
            {
                playerNum++;
                currentPlayer = allplayers[playerNum];
            }
            setStartTurnGameValues();
            playerTurn();
        }

        //Sets the game turn value like cards, movedsofar, special functions
        private void setStartTurnGameValues()
        {
            swapping = false;
            sorry = false;
            split = false;
            movingFireToken = false;
            movingIceToken = false;
            specialNeedsPawn = null;
            firstSelection = null;
            CardPopupImg.Source = null;
            movedSoFar = 0;
        }

        //Validates if the pawn selected can move
        private bool validateMove(Pawn pawn)
        {
            if (getPawnAtLocation(pawn.movingToGrid, pawn.movingTo) != null)
            {
                if (getPawnAtLocation(pawn.movingToGrid, pawn.movingTo).color.Equals(pawn.color) || getPawnAtLocation(pawn.movingToGrid, pawn.movingTo).hasIce)
                {
                    return false;
                }
            }
            return true;
        }

        //Executes a line of code depending on different events
        private void ImageClickEvent(object sender, MouseButtonEventArgs e)
        {
            //Sets the image clicked on and gets the grid of the image that was clicked on
            Image img = (Image)sender;
            Grid temp = (Grid)img.Parent;

            //If this statement is true, set the current pawn to the pawn of the image that was clicked and continue the move normally
            if (movingFireToken == false && movingIceToken == false && swapping == false && sorry == false && split == false)
            {
                if (temp.Name.Equals(TopGrid.Name) || temp.Name.Equals(BottomGrid.Name) || temp.Name.Equals(RedHomeGrid.Name) || temp.Name.Equals(YellowHomeGrid.Name) || temp.Name.Equals(BlueStartGrid.Name) || temp.Name.Equals(GreenStartGrid.Name))
                {
                    currentPawn = getPawnAtLocation(temp.Name, Grid.GetColumn(img));
                }
                else if (temp.Name.Equals(LeftGrid.Name) || temp.Name.Equals(RightGrid.Name) || temp.Name.Equals(BlueHomeGrid.Name) || temp.Name.Equals(GreenHomeGrid.Name) || temp.Name.Equals(RedStartGrid.Name) || temp.Name.Equals(YellowStartGrid.Name))
                {
                    currentPawn = getPawnAtLocation(temp.Name, Grid.GetRow(img));
                }
                cardCheck();
            }

            //If this statement is true, set the specialneedspawn to the pawn of the image that was clicked and execute the special cases
            if (movingFireToken == true || movingIceToken == true || swapping == true || sorry == true || split == true)
            {
                if (temp.Name.Equals(TopGrid.Name) || temp.Name.Equals(BottomGrid.Name) || temp.Name.Equals(RedHomeGrid.Name) || temp.Name.Equals(YellowHomeGrid.Name) || temp.Name.Equals(BlueStartGrid.Name) || temp.Name.Equals(GreenStartGrid.Name))
                {
                    specialNeedsPawn = getPawnAtLocation(temp.Name, Grid.GetColumn(img));
                }
                else if (temp.Name.Equals(LeftGrid.Name) || temp.Name.Equals(RightGrid.Name) || temp.Name.Equals(BlueHomeGrid.Name) || temp.Name.Equals(GreenHomeGrid.Name) || temp.Name.Equals(RedStartGrid.Name) || temp.Name.Equals(YellowStartGrid.Name))
                {
                    specialNeedsPawn = getPawnAtLocation(temp.Name, Grid.GetRow(img));
                }
            }

            //If moving fire token (drawing a 2), else if moving icetoken (drawing a 1),
            if (movingFireToken == true)
            {
                //Remove the current fireToken and replace it with the new selection
                removeFirePawn();
                if (!specialNeedsPawn.hasIce)
                {
                    switch (specialNeedsPawn.color)
                    {
                        case "red":
                            specialNeedsPawn.imageref = new Uri("redfire.png", UriKind.Relative);
                            break;
                        case "blue":
                            specialNeedsPawn.imageref = new Uri("bluefire.png", UriKind.Relative);
                            break;
                        case "green":
                            specialNeedsPawn.imageref = new Uri("greenfire.png", UriKind.Relative);
                            break;
                        case "yellow":
                            specialNeedsPawn.imageref = new Uri("yellowfire.png", UriKind.Relative);
                            break;
                    }
                }
                img.Source = new BitmapImage(specialNeedsPawn.imageref);
                specialNeedsPawn.hasFire = true;
                movingFireToken = false;
                //Set events for pawn selection
                foreach (Player player in allplayers)
                {
                    foreach (Pawn pawn in player.pawns)
                    {
                        if (player != currentPlayer)
                        {
                            setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                        }
                        else if (pawn.hasFire)
                        {
                            setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                        }
                    }
                }
                tcs?.TrySetResult(true);
            }
            else if (movingIceToken == true)
            {
                //Remove the current iceToken and replace it with the new selection
                removeIcePawn();
                switch (specialNeedsPawn.color)
                {
                    case "red":
                        specialNeedsPawn.imageref = new Uri("redice.png", UriKind.Relative);
                        break;
                    case "blue":
                        specialNeedsPawn.imageref = new Uri("blueice.png", UriKind.Relative);
                        break;
                    case "green":
                        specialNeedsPawn.imageref = new Uri("greenice.png", UriKind.Relative);
                        break;
                    case "yellow":
                        specialNeedsPawn.imageref = new Uri("yellowice.png", UriKind.Relative);
                        break;
                }
                img.Source = new BitmapImage(specialNeedsPawn.imageref);
                specialNeedsPawn.hasIce = true;
                movingIceToken = false;
                //Set events for pawn selection
                foreach (Player player in allplayers)
                {
                    foreach (Pawn pawn in player.pawns)
                    {
                        if (player != currentPlayer)
                        {
                            setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                        }
                        else if ( pawn.hasIce)
                        {
                            setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                        }
                    }
                }
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                    }
                }
                tcs?.TrySetResult(true);
            }
            else if (swapping == true)
            {
                //Executes when swapping is true, take the first pawn selected and store it in firstselection pawn
                if (firstSelection == null)
                {
                    firstSelection = specialNeedsPawn;
                    swapPawns();
                }
                else
                {
                    // Swap the pawn thats in firstselection with the pawn in specialneedspawn(2nd selected)
                    removePawn(firstSelection);
                    removePawn(specialNeedsPawn);
                    Pawn temp2 = new Pawn(firstSelection.isAtStart, firstSelection.isAtHome, firstSelection.isSafe, firstSelection.hasFire, firstSelection.hasIce, firstSelection.imageref, firstSelection.color, firstSelection.currentGrid, firstSelection.canMove);
                    temp2.movingTo = firstSelection.movingTo;
                    firstSelection.movingTo = specialNeedsPawn.movingTo;
                    firstSelection.movingToGrid = specialNeedsPawn.movingToGrid;
                    specialNeedsPawn.movingTo = temp2.movingTo;
                    specialNeedsPawn.movingToGrid = temp2.movingToGrid;
                    drawPawn(specialNeedsPawn);
                    drawPawn(firstSelection);
                    specialNeedsPawn.currentGrid = specialNeedsPawn.movingToGrid;
                    specialNeedsPawn.currentPosition = specialNeedsPawn.movingTo;
                    firstSelection.currentGrid = firstSelection.movingToGrid;
                    firstSelection.currentPosition = firstSelection.movingTo;
                    firstSelection = null;
                    specialNeedsPawn = null;
                    swapping = false;
                    tcs?.TrySetResult(true);
                    nextTurn();
                }
            }
            else if (sorry == true)
            {
                //Same as swapping but instead of swapping, second pawn gets bumped
                if (firstSelection == null)
                {
                    firstSelection = specialNeedsPawn;
                    sorryKnockOut();
                }
                else
                {
                    removePawn(firstSelection);
                    removePawn(specialNeedsPawn);
                    firstSelection.currentPosition = specialNeedsPawn.currentPosition;
                    firstSelection.currentGrid = specialNeedsPawn.currentGrid;
                    firstSelection.movingTo = specialNeedsPawn.movingTo;
                    firstSelection.movingToGrid = specialNeedsPawn.movingToGrid;
                    setBackToStart(specialNeedsPawn);
                    drawPawn(firstSelection);
                    checkStart(firstSelection);
                    firstSelection = null;
                    specialNeedsPawn = null;
                    sorry = false;
                    tcs?.TrySetResult(true);
                    nextTurn();
                }
            }
            else if (split == true)
            {
                //When split is true check if the firstselction can validly move. Else wise ask to pick another pawn.
                if (firstSelection == null)
                { 
                    currentCardValue = splitValue1;
                    currentPawn = specialNeedsPawn;
                    MoveForward();
                    movedSoFar = 0;
                    if (validateMove(currentPawn))
                    {
                        specialNeedsPawn.movingTo = specialNeedsPawn.currentPosition;
                        specialNeedsPawn.movingToGrid = specialNeedsPawn.currentGrid;
                        checkStart(specialNeedsPawn);
                        checkHomeGrid(specialNeedsPawn);
                        checkSafeGrid(specialNeedsPawn);
                        currentPawn = null;
                        firstSelection = specialNeedsPawn;
                    }
                    else
                    {
                        specialNeedsPawn.movingTo = specialNeedsPawn.currentPosition;
                        specialNeedsPawn.movingToGrid = specialNeedsPawn.currentGrid;
                        checkStart(specialNeedsPawn);
                        checkHomeGrid(specialNeedsPawn);
                        checkSafeGrid(specialNeedsPawn);
                        currentPawn = null;
                        specialNeedsPawn.canMove = false;
                        specialNeedsPawn = null;
                    }
                    moveSeven();
                }
                else
                {
                    //If the first pawn could validly move then ask for a second pawn 
                    currentCardValue = splitValue2;
                    currentPawn = specialNeedsPawn;
                    MoveForward();
                    movedSoFar = 0;
                    //If both pawns can validly move. Move both
                    if (validateMove(currentPawn) || firstSelection == getPawnAtLocation(currentPawn.movingToGrid, currentPawn.movingTo))
                    {
                        currentPawn = null;
                        currentCardValue = splitValue1;
                        currentPawn = firstSelection;
                        MoveForward();
                        checkStart(firstSelection);
                        checkHomeGrid(firstSelection);
                        checkSafeGrid(firstSelection);
                        checkStart(specialNeedsPawn);
                        checkHomeGrid(specialNeedsPawn);
                        checkSafeGrid(specialNeedsPawn);
                        currentPawn = null;
                        removePawn(firstSelection);
                        removePawn(specialNeedsPawn);
                        drawPawn(firstSelection);
                        drawPawn(specialNeedsPawn);
                        firstSelection.currentGrid = firstSelection.movingToGrid;
                        firstSelection.currentPosition = firstSelection.movingTo;
                        specialNeedsPawn.currentGrid = specialNeedsPawn.movingToGrid;
                        specialNeedsPawn.currentPosition = specialNeedsPawn.movingTo;
                        nextTurn();
                    }
                    else
                    {
                        specialNeedsPawn.movingTo = specialNeedsPawn.currentPosition;
                        specialNeedsPawn.movingToGrid = specialNeedsPawn.currentGrid;
                        checkStart(specialNeedsPawn);
                        checkHomeGrid(specialNeedsPawn);
                        checkSafeGrid(specialNeedsPawn);
                        currentPawn = null;
                        specialNeedsPawn.canMove = false;
                        moveSeven();
                    }
                }
            }
        }

        //Removes the pawn which currently has the ice token and sets it back to its previous state
        private void removeIcePawn()
        {
            foreach (Player player in allplayers)
            {
                foreach (Pawn pawn in player.pawns)
                {
                    if (pawn.hasIce)
                    {
                        removePawn(pawn);
                        pawn.hasIce = false;
                        // If the pawn previously had fire, set the fire token
                        if (pawn.hasFire)
                        {
                            switch (pawn.color)
                            {
                                case "red":
                                    pawn.imageref = new Uri("redfire.png", UriKind.Relative);
                                    break;
                                case "blue":
                                    pawn.imageref = new Uri("bluefire.png", UriKind.Relative);
                                    break;
                                case "green":
                                    pawn.imageref = new Uri("greenfire.png", UriKind.Relative);
                                    break;
                                case "yellow":
                                    pawn.imageref = new Uri("yellowfire.png", UriKind.Relative);
                                    break;
                            }
                        }
                        else
                        {
                            pawn.imageref = new Uri(pawn.color + ".png", UriKind.Relative);
                        }
                        drawPawn(pawn);
                        break;
                    }
                }
            }
        }

        //Removes the pawn which currently has the fire token and sets it back to it's previous state
        private void removeFirePawn()
        {
            foreach (Player player in allplayers)
            {
                foreach (Pawn pawn in player.pawns)
                {
                    if (pawn.hasFire)
                    {
                        removePawn(pawn);
                        pawn.hasFire = false;
                        // If pawn previously had ice, set ice token
                        if (pawn.hasIce)
                        {
                            switch (pawn.color)
                            {
                                case "red":
                                    pawn.imageref = new Uri("redice.png", UriKind.Relative);
                                    break;
                                case "blue":
                                    pawn.imageref = new Uri("blueice.png", UriKind.Relative);
                                    break;
                                case "green":
                                    pawn.imageref = new Uri("greenice.png", UriKind.Relative);
                                    break;
                                case "yellow":
                                    pawn.imageref = new Uri("yellowice.png", UriKind.Relative);
                                    break;
                            }
                        }
                        else
                        {
                            pawn.imageref = new Uri(pawn.color + ".png", UriKind.Relative);
                        }
                        drawPawn(pawn);
                        break;
                    }
                }
            }
        }

        //All of these next methods, allow for movements inside each grid and from grid to grid
        
        private void GreenStartGridMove()
        {
            if (this.currentPawn.isAtStart && this.currentPawn.color.Equals("green"))
            {
                foreach (Image img in GreenStartGrid.Children)
                {
                    if (Grid.GetColumn(img) == currentPawn.movingTo)
                    {
                        this.currentPawn.movingTo = 11;
                        this.currentPawn.movingToGrid = BottomGrid.Name;
                        this.movedSoFar++;
                        break;
                    }
                }
            }
        }


        private void RedStartGridMove()
        {
            if (this.currentPawn.isAtStart && this.currentPawn.color.Equals("red"))
            {
                foreach (Image img in RedStartGrid.Children)
                {
                    if (Grid.GetRow(img) == currentPawn.movingTo)
                    {
                        this.currentPawn.movingTo = 11;
                        this.currentPawn.movingToGrid = LeftGrid.Name;
                        this.movedSoFar++;
                        break;
                    }
                }
            }
        }

        private void BlueStartGridMove()
        {
            if (this.currentPawn.isAtStart && this.currentPawn.color.Equals("blue"))
            {
                foreach (Image img in BlueStartGrid.Children)
                {
                    if (Grid.GetColumn(img) == currentPawn.movingTo)
                    {
                        this.currentPawn.movingTo = 3;
                        this.currentPawn.movingToGrid = TopGrid.Name;
                        this.movedSoFar++;
                        break;
                    }
                }
            }
        }

        private void YellowStartGridMove()
        {
            if (this.currentPawn.isAtStart && this.currentPawn.color.Equals("yellow"))
            {
                foreach (Image img in YellowStartGrid.Children)
                {
                    if (Grid.GetRow(img) == currentPawn.movingTo)
                    {
                        this.currentPawn.movingTo = 3;
                        this.currentPawn.movingToGrid = RightGrid.Name;
                        this.movedSoFar++;
                        break;
                    }
                }
            }
        }

        private void LeftGridMoveForward()
        {
            if (currentPawn.movingTo == 13 && currentPawn.color.Equals("red"))
            {
                currentPawn.movingToGrid = RedHomeGrid.Name;
                currentPawn.movingTo = 0;
                currentPawn.isSafe = true;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 0 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("blue")))
            {
                currentPawn.movingToGrid = TopGrid.Name;
                currentPawn.movingTo = 3;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 7 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("red")))
            {
                currentPawn.movingTo = 2;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 0)
            {
                currentPawn.movingToGrid = TopGrid.Name;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo--;
                this.movedSoFar++;
            }
        }

        private void LeftGridMoveBackward()
        {

            if (currentPawn.movingTo == 14)
            {
                currentPawn.movingToGrid = BottomGrid.Name;
                currentPawn.movingTo = 0;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 13 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("red")))
            {
                currentPawn.movingTo = 11;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 5 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("red")))
            {
                currentPawn.movingTo = 2;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo++;
                this.movedSoFar++;
            }
        }

        private void BottomGridMoveForward()
        {
            if (currentPawn.movingTo == 13 && currentPawn.color.Equals("green"))
            {
                currentPawn.movingToGrid = GreenHomeGrid.Name;
                currentPawn.movingTo = 4;
                currentPawn.isSafe = true;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 0 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("red")))
            {
                currentPawn.movingToGrid = LeftGrid.Name;
                currentPawn.movingTo = 11;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 7 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("green")))
            {
                currentPawn.movingTo = 2;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 0)
            {
                currentPawn.movingToGrid = LeftGrid.Name;
                currentPawn.movingTo = 14;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo--;
                this.movedSoFar++;
            }
        }

        private void BottomGridMoveBackward()
        {
            if (currentPawn.movingTo == 14)
            {
                currentPawn.movingToGrid = RightGrid.Name;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 13 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("green")))
            {
                currentPawn.movingTo = 11;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 5 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("green")))
            {
                currentPawn.movingTo = 2;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo++;
                this.movedSoFar++;
            }
        }

        private void TopGridMoveForward()
        {
            if (currentPawn.movingTo == 1 && currentPawn.color.Equals("blue"))
            {
                currentPawn.movingToGrid = BlueHomeGrid.Name;
                currentPawn.movingTo = 0;
                currentPawn.isSafe = true;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 14 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("yellow")))
            {
                currentPawn.movingToGrid = RightGrid.Name;
                currentPawn.movingTo = 3;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 7 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("blue")))
            {
                currentPawn.movingTo = 12;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 14)
            {
                currentPawn.movingToGrid = RightGrid.Name;
                currentPawn.movingTo = 0;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo++;
                this.movedSoFar++;
            }
        }

        private void TopGridMoveBackward()
        {
            if (currentPawn.movingTo == 0)
            {
                currentPawn.movingToGrid = LeftGrid.Name;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 1 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("blue")))
            {
                currentPawn.movingTo = 3;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 9 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("blue")))
            {
                currentPawn.movingTo = 12;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo--;
                this.movedSoFar++;
            }
        }

        private void RightGridMoveForward()
        {
            if (currentPawn.movingTo == 1 && currentPawn.color.Equals("yellow"))
            {
                currentPawn.movingToGrid = YellowHomeGrid.Name;
                currentPawn.movingTo = 4;
                currentPawn.isSafe = true;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 14 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("green")))
            {
                currentPawn.movingToGrid = BottomGrid.Name;
                currentPawn.movingTo = 11;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 7 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("yellow")))
            {
                currentPawn.movingTo = 12;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 14)
            {
                currentPawn.movingToGrid = BottomGrid.Name;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo++;
                this.movedSoFar++;
            }
        }

        private void RightGridMoveBackward()
        {
            if (currentPawn.movingTo == 0)
            {
                currentPawn.movingToGrid = TopGrid.Name;
                currentPawn.movingTo = 14;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 1 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("yellow")))
            {
                currentPawn.movingTo = 3;
                this.movedSoFar++;
            }
            else if (currentPawn.movingTo == 9 && (this.movedSoFar == this.currentCardValue - 1) && !(currentPawn.color.Equals("yellow ")))
            {
                currentPawn.movingTo = 12;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo--;
                this.movedSoFar++;
            }
        }

        private void GreenHomeGridMoveForward()
        {
            if (currentPawn.movingTo == 0)
            {
                removePawn(currentPawn);
                movedSoFar = currentCardValue;
                currentPawn.isAtHome = true;
                currentPawn.movingToGrid = GreenHome.Name;
                int counter = 0;
                foreach (Image img in GreenHome.Children)
                {
                    if (img.Source == null)
                    {
                        currentPawn.movingTo = counter;
                        img.Source = new BitmapImage(currentPawn.imageref);
                        break;
                    }
                    counter++;
                }
            }
            else
            {
                currentPawn.movingTo--;
                this.movedSoFar++;
            }
        }

        private void GreenHomeGridMoveBackward()
        {
            if (currentPawn.movingTo == 4)
            {
                currentPawn.movingToGrid = BottomGrid.Name;
                currentPawn.movingTo = 13;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo++;
                this.movedSoFar++;
            }
        }

        private void RedHomeGridMoveForward()
        {
            if (currentPawn.movingTo == 4)
            {
                removePawn(currentPawn);
                movedSoFar = currentCardValue;
                currentPawn.isAtHome = true;
                currentPawn.movingToGrid = RedHome.Name;
                int counter = 0;
                foreach (Image img in RedHome.Children)
                {
                    if (img.Source == null)
                    {
                        currentPawn.movingTo = counter;
                        img.Source = new BitmapImage(currentPawn.imageref);
                        break;
                    }
                    counter++;
                }
            }
            else
            {
                currentPawn.movingTo++;
                this.movedSoFar++;
            }
        }

        private void RedHomeGridMoveBackward()
        {
            if (currentPawn.movingTo == 0)
            {
                currentPawn.movingToGrid = LeftGrid.Name;
                currentPawn.movingTo = 13;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo--;
                this.movedSoFar++;
            }
        }

        private void BlueHomeGridMoveForward()
        {
            if (currentPawn.movingTo == 4)
            {
                removePawn(currentPawn);
                movedSoFar = currentCardValue;
                currentPawn.isAtHome = true;
                currentPawn.movingToGrid = BlueHome.Name;
                int counter = 0;
                foreach (Image img in BlueHome.Children)
                {
                    if (img.Source == null)
                    {
                        currentPawn.movingTo = counter;
                        img.Source = new BitmapImage(currentPawn.imageref);
                        break;
                    }
                    counter++;
                }
            }
            else
            {
                currentPawn.movingTo++;
                this.movedSoFar++;
            }
        }

        private void BlueHomeGridMoveBackward()
        {
            if (currentPawn.movingTo == 0)
            {
                currentPawn.movingToGrid = TopGrid.Name;
                currentPawn.movingTo = 1;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo--;
                this.movedSoFar++;
            }
        }

        private void YellowHomeGridMoveForward()
        {
            if (currentPawn.movingTo == 0)
            {
                removePawn(currentPawn);
                movedSoFar = currentCardValue;
                currentPawn.isAtHome = true;
                currentPawn.movingToGrid = YellowHome.Name;
                int counter = 0;
                foreach (Image img in YellowHome.Children)
                {
                    if (img.Source == null)
                    {
                        currentPawn.movingTo = counter;
                        img.Source = new BitmapImage(currentPawn.imageref);
                        break;
                    }
                    counter++;
                }
            }
            else
            {
                currentPawn.movingTo--;
                this.movedSoFar++;
            }
        }

        private void YellowHomeGridMoveBackward()
        {
            if (currentPawn.movingTo == 4)
            {
                currentPawn.movingToGrid = RightGrid.Name;
                currentPawn.movingTo = 1;
                this.movedSoFar++;
            }
            else
            {
                currentPawn.movingTo++;
                this.movedSoFar++;
            }
        }

        // The movement methods end here


        // Calls differnt move forward method depending on the grid the movement is at until the number of moves run out
        private void MoveForward()
        {
            while (movedSoFar < currentCardValue)
            {
                if (this.currentPawn.movingToGrid.Equals(TopGrid.Name))
                {
                    TopGridMoveForward();
                }
                else if (this.currentPawn.movingToGrid.Equals(BottomGrid.Name))
                {
                    BottomGridMoveForward();
                }
                else if (this.currentPawn.movingToGrid.Equals(LeftGrid.Name))
                {
                    LeftGridMoveForward();
                }
                else if (this.currentPawn.movingToGrid.Equals(RightGrid.Name))
                {
                    RightGridMoveForward();
                }
                else if (this.currentPawn.movingToGrid.Equals(GreenStartGrid.Name))
                {
                    GreenStartGridMove();
                }
                else if (this.currentPawn.movingToGrid.Equals(BlueStartGrid.Name))
                {
                    BlueStartGridMove();
                }
                else if (this.currentPawn.movingToGrid.Equals(RedStartGrid.Name))
                {
                    RedStartGridMove();
                }
                else if (this.currentPawn.movingToGrid.Equals(YellowStartGrid.Name))
                {
                    YellowStartGridMove();
                }
                else if (this.currentPawn.movingToGrid.Equals(GreenHomeGrid.Name))
                {
                    GreenHomeGridMoveForward();
                }
                else if (this.currentPawn.movingToGrid.Equals(BlueHomeGrid.Name))
                {
                    BlueHomeGridMoveForward();
                }
                else if (this.currentPawn.movingToGrid.Equals(YellowHomeGrid.Name))
                {
                    YellowHomeGridMoveForward();
                }
                else if (this.currentPawn.movingToGrid.Equals(RedHomeGrid.Name))
                {
                    RedHomeGridMoveForward();
                }
            }
        }

        // Calls differnt move backwards method depending on the grid the movement is at until the number of moves run out
        private void MoveBackward()
        {
            while (movedSoFar < currentCardValue)
            {
                if (this.currentPawn.movingToGrid.Equals(TopGrid.Name))
                {
                    TopGridMoveBackward();
                }
                else if (this.currentPawn.movingToGrid.Equals(BottomGrid.Name))
                {
                    BottomGridMoveBackward();
                }
                else if (this.currentPawn.movingToGrid.Equals(LeftGrid.Name))
                {
                    LeftGridMoveBackward();
                }
                else if (this.currentPawn.movingToGrid.Equals(RightGrid.Name))
                {
                    RightGridMoveBackward();
                }
                else if (this.currentPawn.movingToGrid.Equals(GreenHomeGrid.Name))
                {
                    GreenHomeGridMoveBackward();
                }
                else if (this.currentPawn.movingToGrid.Equals(BlueHomeGrid.Name))
                {
                    BlueHomeGridMoveBackward();
                }
                else if (this.currentPawn.movingToGrid.Equals(YellowHomeGrid.Name))
                {
                    YellowHomeGridMoveBackward();
                }
                else if (this.currentPawn.movingToGrid.Equals(RedHomeGrid.Name))
                {
                    RedHomeGridMoveBackward();
                }
            }
        }

        //Draws the pawns into the start grids and positions them in the start grids
        private void setPawns(List<Player> allplayers)
        {
            foreach (Player player in allplayers)
            {
                switch (player.color)
                {
                    case "red":
                        foreach (Image img in RedStartGrid.Children)
                        {
                            img.Source = new BitmapImage(player.pawns[0].imageref);
                            img.IsEnabled = false;
                        }
                        for (int i = 0; i < player.pawns.Length; i++)
                        {
                            player.pawns[i].currentGrid = RedStartGrid.Name;
                            player.pawns[i].currentPosition = i;
                            player.pawns[i].movingTo = i;
                        }
                        break;
                    case "green":
                        foreach (Image img in GreenStartGrid.Children)
                        {
                            img.Source = new BitmapImage(player.pawns[0].imageref);
                            img.IsEnabled = false;
                        }
                        for (int i = 0; i < player.pawns.Length; i++)
                        {
                            player.pawns[i].currentGrid = GreenStartGrid.Name;
                            player.pawns[i].currentPosition = i;
                            player.pawns[i].movingTo = i;
                        }
                        break;
                    case "blue":
                        foreach (Image img in BlueStartGrid.Children)
                        {
                            img.Source = new BitmapImage(player.pawns[0].imageref);
                            img.IsEnabled = false;
                        }
                        for (int i = 0; i < player.pawns.Length; i++)
                        {
                            player.pawns[i].currentGrid = BlueStartGrid.Name;
                            player.pawns[i].currentPosition = i;
                            player.pawns[i].movingTo = i;
                        }
                        break;
                    case "yellow":
                        foreach (Image img in YellowStartGrid.Children)
                        {
                            img.Source = new BitmapImage(player.pawns[0].imageref);
                            img.IsEnabled = false;
                        }
                        for (int i = 0; i < player.pawns.Length; i++)
                        {
                            player.pawns[i].currentGrid = YellowStartGrid.Name;
                            player.pawns[i].currentPosition = i;
                            player.pawns[i].movingTo = i;
                        }
                        break;
                }
            }
        }

        // Creates a list of the all cards and fills the deck
        private List<Card> createDeck()
        {
            this.deck = new List<Card>(45);
            Card[] cards = new Card[11];
            cards[0] = new Card(1, new Uri("card1.png", UriKind.Relative));
            cards[1] = new Card(2, new Uri("card2.png", UriKind.Relative));
            cards[2] = new Card(3, new Uri("card3.png", UriKind.Relative));
            cards[3] = new Card(4, new Uri("card4.png", UriKind.Relative));
            cards[4] = new Card(5, new Uri("card5.png", UriKind.Relative));
            cards[5] = new Card(7, new Uri("card7.png", UriKind.Relative));
            cards[6] = new Card(8, new Uri("card8.png", UriKind.Relative));
            cards[7] = new Card(10, new Uri("card10.png", UriKind.Relative));
            cards[8] = new Card(11, new Uri("card11.png", UriKind.Relative));
            cards[9] = new Card(12, new Uri("card12.png", UriKind.Relative));
            cards[10] = new Card(0, new Uri("cardsorry.png", UriKind.Relative));
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < cards.Length; j++)
                {
                    deck.Add(cards[j]);
                }
            }
            deck.Add(new Card(1, new Uri("card1.png", UriKind.Relative)));
            return deck;
        }
        private void shuffleDeck(List<Card> deck)
        {
            Random rnd = new Random();
            for (int i = 0; i < deck.Count; i++)
            {
                int j = rnd.Next(i, deck.Count);
                Card temp = deck[j];
                deck[j] = deck[i];
                deck[i] = temp;
            }
        }


        // Movement to be executed before turn start when a pawn has the fire token placed upon it
        private void moveFirePawn(Pawn firepawn)
        {
            if (firepawn.currentGrid.Equals(TopGrid.Name) && firepawn.currentPosition != 14)
            {
                Pawn fireLocation = getPawnAtLocation(TopGrid.Name, 14);
                if(fireLocation != null)
                {
                    if (!fireLocation.hasIce && !(fireLocation.color.Equals(firepawn.color)))
                    {
                        removePawn(fireLocation);
                        setBackToStart(fireLocation);
                        removePawn(firepawn);
                        firepawn.movingTo = 14;
                        drawPawn(firepawn);
                        firepawn.currentPosition = 14;
                    }
                    else
                    {
                        MessageBox.Show("Cannot move pawn with firetoken");
                    }
                }
                else
                {
                    removePawn(firepawn);
                    firepawn.movingTo = 14;
                    drawPawn(firepawn);
                    firepawn.currentPosition = 14;
                }
            }
            else if (firepawn.currentGrid.Equals(TopGrid.Name) && firepawn.currentPosition == 14)
            {
                Pawn fireLocation = getPawnAtLocation(RightGrid.Name, 14);
                if (fireLocation != null)
                {

                    if (!fireLocation.hasIce && !(fireLocation.color.Equals(firepawn.color)))
                    {
                        removePawn(fireLocation);
                        setBackToStart(fireLocation);
                        removePawn(firepawn);
                        firepawn.movingToGrid = RightGrid.Name;
                        firepawn.movingTo = 14;
                        drawPawn(firepawn);
                        firepawn.currentGrid = RightGrid.Name;
                        firepawn.currentPosition = 14;
                    }
                    else
                    {
                        MessageBox.Show("Cannot move pawn with firetoken");
                    }
                }
                else
                {
                    removePawn(firepawn);
                    firepawn.movingToGrid = RightGrid.Name;
                    firepawn.movingTo = 14;
                    drawPawn(firepawn);
                    firepawn.currentGrid = RightGrid.Name;
                    firepawn.currentPosition = 14;
                }
            }
            else if (firepawn.currentGrid.Equals(RightGrid.Name) && firepawn.currentPosition != 14)
            {

                Pawn fireLocation = getPawnAtLocation(RightGrid.Name, 14);
                if (fireLocation != null)
                {

                    if (!fireLocation.hasIce && !(fireLocation.color.Equals(firepawn.color)))
                    {
                        removePawn(fireLocation);
                        setBackToStart(fireLocation);
                        removePawn(firepawn);
                        firepawn.movingTo = 14;
                        drawPawn(firepawn);
                        firepawn.currentPosition = 14;
                    }
                    else
                    {
                        MessageBox.Show("Cannot move pawn with firetoken");
                    }
                }
                else
                {
                    removePawn(firepawn);
                    firepawn.movingTo = 14;
                    drawPawn(firepawn);
                    firepawn.currentPosition = 14;
                }
            }
            else if (firepawn.currentGrid.Equals(RightGrid.Name) && firepawn.currentPosition == 14)
            {

                Pawn fireLocation = getPawnAtLocation(BottomGrid.Name, 0);
                if (fireLocation != null)
                {

                    if (!fireLocation.hasIce && !(fireLocation.color.Equals(firepawn.color)))
                    {
                        removePawn(fireLocation);
                        setBackToStart(fireLocation);
                        removePawn(firepawn);
                        firepawn.movingToGrid = BottomGrid.Name;
                        firepawn.movingTo = 0;
                        drawPawn(firepawn);
                        firepawn.currentGrid = BottomGrid.Name;
                        firepawn.currentPosition = 0;
                    }
                    else
                    {
                        MessageBox.Show("Cannot move pawn with firetoken");
                    }
                }
                else
                {
                    removePawn(firepawn);
                    firepawn.movingToGrid = BottomGrid.Name;
                    firepawn.movingTo = 0;
                    drawPawn(firepawn);
                    firepawn.currentGrid = BottomGrid.Name;
                    firepawn.currentPosition = 0;
                }
            }
            else if (firepawn.currentGrid.Equals(BottomGrid.Name) && firepawn.currentPosition != 0)
            {

                Pawn fireLocation = getPawnAtLocation(BottomGrid.Name, 0);
                if (fireLocation != null)
                {

                    if (!fireLocation.hasIce && !(fireLocation.color.Equals(firepawn.color)))
                    {
                        removePawn(fireLocation);
                        setBackToStart(fireLocation);
                        removePawn(firepawn);
                        firepawn.movingTo = 0;
                        drawPawn(firepawn);
                        firepawn.currentPosition = 0;
                    }
                    else
                    {
                        MessageBox.Show("Cannot move pawn with firetoken");
                    }
                }
                else
                {
                    removePawn(firepawn);
                    firepawn.movingTo = 0;
                    drawPawn(firepawn);
                    firepawn.currentPosition = 0;
                }
            }
            else if (firepawn.currentGrid.Equals(BottomGrid.Name) && firepawn.currentPosition == 0)
            {

                Pawn fireLocation = getPawnAtLocation(LeftGrid.Name, 0);
                if (fireLocation != null)
                {

                    if (!fireLocation.hasIce && !(fireLocation.color.Equals(firepawn.color)))
                    {
                        removePawn(fireLocation);
                        setBackToStart(fireLocation);
                        removePawn(firepawn);
                        firepawn.movingToGrid = LeftGrid.Name;
                        firepawn.movingTo = 0;
                        drawPawn(firepawn);
                        firepawn.currentGrid = LeftGrid.Name;
                        firepawn.currentPosition = 0;
                    }
                    else
                    {
                        MessageBox.Show("Cannot move pawn with firetoken");
                    }
                }
                else
                {
                    removePawn(firepawn);
                    firepawn.movingToGrid = LeftGrid.Name;
                    firepawn.movingTo = 0;
                    drawPawn(firepawn);
                    firepawn.currentGrid = LeftGrid.Name;
                    firepawn.currentPosition = 0;
                }
            }
            else if (firepawn.currentGrid.Equals(LeftGrid.Name) && firepawn.currentPosition != 0)
            {

                Pawn fireLocation = getPawnAtLocation(LeftGrid.Name, 0);
                if (fireLocation != null)
                {

                    if (!fireLocation.hasIce && !(fireLocation.color.Equals(firepawn.color)))
                    {
                        removePawn(fireLocation);
                        setBackToStart(fireLocation);
                        removePawn(firepawn);
                        firepawn.movingToGrid = LeftGrid.Name;
                        firepawn.movingTo = 0;
                        drawPawn(firepawn);
                        firepawn.currentGrid = LeftGrid.Name;
                        firepawn.currentPosition = 0;
                    }
                    else
                    {
                        MessageBox.Show("Cannot move pawn with firetoken");
                    }
                }
                else
                {
                    removePawn(firepawn);
                    firepawn.movingToGrid = LeftGrid.Name;
                    firepawn.movingTo = 0;
                    drawPawn(firepawn);
                    firepawn.currentGrid = LeftGrid.Name;
                    firepawn.currentPosition = 0;
                }
            }
            else if (firepawn.currentGrid.Equals(LeftGrid.Name) && firepawn.currentPosition == 0)
            {
                Pawn fireLocation = getPawnAtLocation(TopGrid.Name, 14);
                if (fireLocation != null)
                {
                    if (!fireLocation.hasIce && !(fireLocation.color.Equals(firepawn.color)))
                    {
                        removePawn(fireLocation);
                        setBackToStart(fireLocation);
                        removePawn(firepawn);
                        firepawn.movingToGrid = TopGrid.Name;
                        firepawn.movingTo = 14;
                        drawPawn(firepawn);
                        firepawn.currentGrid = TopGrid.Name;
                        firepawn.currentPosition = 14;
                    }
                    else
                    {
                        MessageBox.Show("Cannot move pawn with firetoken");
                    }
                }
                else
                {
                    removePawn(firepawn);
                    firepawn.movingToGrid = TopGrid.Name;
                    firepawn.movingTo = 14;
                    drawPawn(firepawn);
                    firepawn.currentGrid = TopGrid.Name;
                    firepawn.currentPosition = 14;
                }
            }

        }


        //Sets the canMove property of each pawn of the current player
        private void setPawnsCanMove()
        {
            foreach (Pawn pawn in currentPlayer.pawns)
            {
                if (!pawn.hasIce && !pawn.isAtHome)
                {
                    pawn.canMove = true;
                }
                else
                {
                    pawn.canMove = false;
                }
            }
        }

        //Exectues the players turn. Sets teh movement of the pawns and enables the correct pawns to be clicked. Moves fire token when needed
        private async void playerTurn()
        {
            playersName.Text = currentPlayer.username + "'s turn";
            setPawnsCanMove();
            if (currentPlayer.color.Equals("blue"))
            {
                playersName.Background = Brushes.Blue;
            }
            else if (currentPlayer.color.Equals("green"))
            {
                playersName.Background = Brushes.Green;
            }
            else if (currentPlayer.color.Equals("yellow"))
            {
                playersName.Background = Brushes.Yellow;
            }
            else if (currentPlayer.color.Equals("red"))
            {
                playersName.Background = Brushes.Red;
            }

                MessageBox.Show(currentPlayer.username + ", it's your turn");

            if (!currentPlayer.isCpu)
            {

                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (pawn.hasFire && !pawn.hasIce && !pawn.isAtStart && !pawn.isSafe && !pawn.isAtHome)
                    {
                        if (MessageBox.Show(currentPlayer.username + ", do you wish to move your pawn with the firetoken", "Firepawn Movement", MessageBoxButton.YesNo).ToString().Equals("Yes"))
                        {
                            moveFirePawn(pawn);
                        }
                    }
                }
            }

            if (currentPlayer.isCpu == true)
            {
                Thread.Sleep(2000);
                cpuMove();
            }

            foreach (Player player in allplayers)
            {
                foreach(Pawn pawn in player.pawns)
                {
                    setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                }
            }
            if (!currentPlayer.isCpu)
            {
                CardImg.IsEnabled = true;
            }
            tcs = new TaskCompletionSource<bool>();
            await tcs.Task;
        }

        //Checks the current card value and calls the approiate movement and validates the move, if player can not move, call next turn. If player enters all pawns in home, calls wining window.
        public async void cardCheck()
        {
            if (currentCardValue == 1)
            {
                MoveForward();
            }
            else if (currentCardValue == -1)
            {
                currentCardValue = 1;
                MoveBackward();
            }
            else if (currentCardValue == 4)
            {
                MoveBackward();

            }
            else if (currentCardValue == 2)
            {
                MoveForward();
            }
            else if (currentCardValue == 7)
            {
                MoveForward();
            }
            else if (currentCardValue == 0)
            {
                currentCardValue = 4;
                MoveForward();
            }
            else
            {
                MoveForward();
            }
            if (validateMove(currentPawn))
            {
                foreach(Pawn pawn in currentPlayer.pawns)
                {
                    checkStart(pawn);
                    checkHomeGrid(pawn);
                    checkSafeGrid(pawn);
                    if(pawn != currentPawn)
                    {
                        pawn.currentGrid = pawn.movingToGrid;
                        pawn.currentPosition = pawn.movingTo;
                    }
                }
                drawPawn(currentPawn);
                removePawn(currentPawn);
                currentPawn.currentPosition = currentPawn.movingTo;
                currentPawn.currentGrid = currentPawn.movingToGrid;
                checkStart(currentPawn);
                checkSafeGrid(currentPawn);
                checkHomeGrid(currentPawn);
                if (checkWin())
                {
                    Window win = new YouWin(currentPlayer);
                    win.Show();
                    this.Close();
                }
                else
                {
                    nextTurn();
                }
            }
            else
            {
                if (checkIfCantMove())
                {
                    MessageBox.Show(currentPlayer.username + ", you have no valid moves");
                    nextTurn();
                }
                else
                {
                    MessageBox.Show("Invalid move, pick another pawn");
                    currentPawn.movingTo = currentPawn.currentPosition;
                    currentPawn.movingToGrid = currentPawn.currentGrid;
                    currentPawn.canMove = false;
                    currentPawn = null;
                    movedSoFar = 0;
                    tcs = new TaskCompletionSource<bool>();
                    await tcs.Task;
                }
            }
        }

        //Method which checks the start property of pawn is properly set
        private void checkStart(Pawn pawn)
        {
            switch (pawn.movingToGrid)
            {
                case "RedStartGrid":
                    pawn.isAtStart = true;
                    break;

                case "BlueStartGrid":
                    pawn.isAtStart = true;
                    break;

                case "YellowStartGrid":
                    pawn.isAtStart = true;
                    break;

                case "GreenStartGrid":
                    pawn.isAtStart = true;
                    break;

                default:
                    pawn.isAtStart = false;
                    break;
            }
        }

        //Method which checks the isSafe property of pawn is properly set
        private void checkSafeGrid(Pawn pawn)
        {
            switch (pawn.movingToGrid)
            {
                case "RedHomeGrid":
                    pawn.isSafe = true;
                    break;

                case "BlueHomeGrid":
                    pawn.isSafe = true;
                    break;

                case "YellowHomeGrid":
                    pawn.isSafe = true;
                    break;

                case "GreenHomeGrid":
                    pawn.isSafe = true;
                    break;

                default:
                    pawn.isSafe = false;
                    break;
            }
        }

        //Method which checks the isHome property of pawn is properly set
        private void checkHomeGrid(Pawn pawn)
        {
            switch (pawn.movingToGrid)
            {
                case "RedHome":
                    pawn.isAtHome = true;
                    break;

                case "BlueHome":
                    pawn.isAtHome = true;
                    break;

                case "YellowHome":
                    pawn.isAtHome = true;
                    break;

                case "GreenHome":
                    pawn.isAtHome = true;
                    break;
            }
        }

        //Checks if the current player has won
        private bool checkWin()
        {
            foreach (Pawn pawn in currentPlayer.pawns)
            {
                if (!pawn.isAtHome)
                {
                    return false;
                }
            }
            return true;
        }

        //Checks if the current player has any remaining valid moves
        private bool checkIfCantMove()
        {
            foreach (Pawn pawn in currentPlayer.pawns)
            {
                if (pawn.canMove)
                {
                    return false;
                }
            }
            return true;
        }

        //Checks if all the currentplayers pawns are in his start grid
        private bool checkPawnsAtStartGrid()
        {
            foreach (Pawn pawn in currentPlayer.pawns)
            {
                if (!(pawn.currentGrid.Equals(RedStartGrid.Name) || pawn.currentGrid.Equals(BlueStartGrid.Name) || pawn.currentGrid.Equals(YellowStartGrid.Name) || pawn.currentGrid.Equals(GreenStartGrid.Name)))
                {
                    return false;
                }
            }
            return true;
        }

        private bool checkPawnAtStartGrid()
        {
            foreach (Pawn pawn in currentPlayer.pawns)
            {
                if (pawn.isAtStart)
                {
                    return false;
                }
            }
            return true;
        }

        //Checks if current player has at least one pawn that is swappable(IE Top,Left,Bottom or Right Grids)
        private bool checkIfOwnPawnsAreSwappable()
        {
            foreach (Pawn pawn in currentPlayer.pawns)
            {
                if (!(pawn.isSafe || pawn.isAtHome || pawn.isAtStart || pawn.hasIce))
                {
                    return false;
                }
            }
            return true;
        }

        // Checks if all opponents are on the main grids( IE Top,Left,Bottom or Right Grids)
        private bool checkAllPawns()
        {
            foreach (Player player in players)
            {
                if (player != currentPlayer)
                {
                    foreach (Pawn pawn in player.pawns)
                    {
                        if (!(pawn.isSafe || pawn.isAtHome || pawn.isAtStart || pawn.hasIce))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        //Method used to set the correct image events and prompt players to swap
        private async void swapPawns()
        {
            foreach (Player player in players)
            {
                foreach (Pawn pawn in player.pawns)
                {
                    if (player == currentPlayer)
                    {
                        setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                    }
                    else if (!(pawn.hasIce || pawn.isAtStart || pawn.isAtHome || pawn.isSafe))
                    {
                        setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                    }
                }
            }
            MessageBox.Show("Pick a pawn to swap with");
            tcs = new TaskCompletionSource<bool>();
            await tcs.Task;
        }

        //Method used to set the correct image events and prompt players to knock out a pawn
        private async void sorryKnockOut()
        {
            foreach (Player player in allplayers)
            {
                foreach (Pawn pawn in player.pawns)
                {
                    if (player == currentPlayer)
                    {
                        setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                    }
                    else if (!(pawn.hasIce || pawn.isAtStart || pawn.isAtHome || pawn.isSafe))
                    {
                        setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                    }
                }
            }
            MessageBox.Show("Pick a pawn to knock out");
            tcs = new TaskCompletionSource<bool>();
            await tcs.Task;
        }

        //Method used to set the correct image events and prompt players actions
        private async void moveSeven()
        {
            // Checks if the current selection can move, if no disable the selection and ask for a new one
            if (checkIfCantMove())
            {
                switch (splitValue1)
                {
                    case 1:
                        RBtn16.IsEnabled = false;
                        break;
                    case 2:
                        RBtn25.IsEnabled = false;
                        break;
                    case 3:
                        RBtn34.IsEnabled = false;
                        break;
                }
            }
            if (!(RBtn16.IsEnabled && RBtn16.IsEnabled && RBtn16.IsEnabled))
            {
                split = false;
                currentCardValue = 7;
                MessageBox.Show("No valid move by splitting. Move by 7");
                tcs = new TaskCompletionSource<bool>();
                await tcs.Task;
            }
            else
            {
                setImageEventFalse(firstSelection.currentGrid, firstSelection.currentPosition);
                if( firstSelection != null)
                {
                    MessageBox.Show("You're first pick was valid, Pick a different pawn to move by " + splitValue2);
                }
                else
                {
                    MessageBox.Show("You're first pick was invalid, Pick a different pawn to move by " + splitValue1);
                }
                tcs = new TaskCompletionSource<bool>();
                await tcs.Task;
            }
        }

        //Method that sets all the events to true to move the fire token
        private async void moveFireToken()
        {
            foreach (Player player in allplayers)
            {
                foreach (Pawn pawn in player.pawns)
                {
                    if (!pawn.hasFire || !pawn.isAtHome) 
                    {
                        setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                    }
                    else
                    {
                        setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                    }
                }
            }
            movingFireToken = true;
            tcs = new TaskCompletionSource<bool>();
            await tcs.Task;
        }

        ////Method that sets all the events to true except the current ice token to move the ice token
        private async void moveIceToken()
        {
            foreach (Player player in allplayers)
            {
                foreach (Pawn pawn in player.pawns)
                {
                    if (!pawn.hasIce)
                    {
                        setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                    }
                    else
                    {
                        setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                    }
                }
            }
            movingIceToken = true;
            tcs = new TaskCompletionSource<bool>();
            await tcs.Task;
        }

        // When a pawn is landed on setBackToStart sets their position and draws them in thier start zone
        private void setBackToStart(Pawn pawn)
        {
             switch (pawn.color)
            {
                case "red":
                    foreach (Image img in RedStartGrid.Children)
                    {
                        if (img.Source == null)
                        {
                            pawn.currentPosition = Grid.GetRow(img);
                            pawn.movingTo = Grid.GetRow(img);
                            pawn.currentGrid = RedStartGrid.Name;
                            pawn.movingToGrid = RedStartGrid.Name;
                            checkStart(pawn);
                            img.Source = new BitmapImage(pawn.imageref);
                            break;
                        }
                    }
                    break;
                case "green":
                    foreach (Image img in GreenStartGrid.Children)
                    {
                        if (img.Source == null)
                        {
                            pawn.currentPosition = Grid.GetColumn(img);
                            pawn.movingTo = Grid.GetColumn(img);
                            pawn.currentGrid = GreenStartGrid.Name;
                            pawn.movingToGrid = GreenStartGrid.Name;
                            checkStart(pawn);
                            img.Source = new BitmapImage(pawn.imageref);
                            break;
                        }
                    }
                    break;
                case "blue":
                    foreach (Image img in BlueStartGrid.Children)
                    {
                        if (img.Source == null)
                        {
                            pawn.currentPosition = Grid.GetColumn(img);
                            pawn.movingTo = Grid.GetColumn(img);
                            pawn.currentGrid = BlueStartGrid.Name;
                            pawn.movingToGrid = BlueStartGrid.Name;
                            checkStart(pawn);
                            img.Source = new BitmapImage(pawn.imageref);
                            break;
                        }
                    }
                    break;
                case "yellow":
                    foreach (Image img in YellowStartGrid.Children)
                    {
                        if (img.Source == null)
                        {
                            pawn.currentPosition = Grid.GetRow(img);
                            pawn.movingTo = Grid.GetRow(img);
                            pawn.currentGrid = YellowStartGrid.Name;
                            pawn.movingToGrid = YellowStartGrid.Name;
                            checkStart(pawn);
                            img.Source = new BitmapImage(pawn.imageref);
                            break;
                        }
                    }
                    break;
            }
        }


        //  Draw the given pawn, if there is a pawn at that location. Set the pawn at that location back to start and draw the given pawn
        private void drawPawn(Pawn pawn)
        {
            if (pawn.movingToGrid.Equals(TopGrid.Name))
            {
                foreach (Image img in TopGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.movingTo && img.Source == null)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                    else if (Grid.GetColumn(img) == pawn.movingTo && img.Source != null)
                    {
                        setBackToStart(getPawnAtLocation(pawn.movingToGrid, pawn.movingTo));
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(BottomGrid.Name))
            {
                foreach (Image img in BottomGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.movingTo && img.Source == null)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                    else if (Grid.GetColumn(img) == pawn.movingTo && img.Source != null)
                    {
                        setBackToStart(getPawnAtLocation(pawn.movingToGrid, pawn.movingTo));
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(LeftGrid.Name))
            {
                foreach (Image img in LeftGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.movingTo && img.Source == null)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                    else if (Grid.GetRow(img) == pawn.movingTo && img.Source != null)
                    {
                        setBackToStart(getPawnAtLocation(pawn.movingToGrid, pawn.movingTo));
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(RightGrid.Name))
            {
                foreach (Image img in RightGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.movingTo && img.Source == null)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                    else if (Grid.GetRow(img) == pawn.movingTo && img.Source != null)
                    {
                        setBackToStart(getPawnAtLocation(pawn.movingToGrid, pawn.movingTo));
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(RedHomeGrid.Name))
            {
                foreach (Image img in RedHomeGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.movingTo)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(YellowHomeGrid.Name))
            {
                foreach (Image img in YellowHomeGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.movingTo)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(BlueHomeGrid.Name))
            {
                foreach (Image img in BlueHomeGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.movingTo)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(GreenHomeGrid.Name))
            {
                foreach (Image img in GreenHomeGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.movingTo)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(RedStartGrid.Name))
            {
                foreach (Image img in RedStartGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.movingTo)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(GreenStartGrid.Name))
            {
                foreach (Image img in GreenStartGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.movingTo)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(BlueStartGrid.Name))
            {
                foreach (Image img in BlueStartGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.movingTo)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(YellowStartGrid.Name))
            {
                foreach (Image img in YellowStartGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.movingTo)
                    {
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(YellowHome.Name))
            {
                foreach (Image img in YellowHome.Children)
                {
                    if (Grid.GetRow(img) == pawn.movingTo)
                    {
                        pawn.isAtHome = true;
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(RedHome.Name))
            {
                foreach (Image img in RedHome.Children)
                {
                    if (Grid.GetRow(img) == pawn.movingTo)
                    {
                        pawn.isAtHome = true;
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(BlueHome.Name))
            {
                foreach (Image img in BlueHome.Children)
                {
                    if (Grid.GetColumn(img) == pawn.movingTo)
                    {
                        pawn.isAtHome = true;
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
            else if (pawn.movingToGrid.Equals(GreenHome.Name))
            {
                foreach (Image img in GreenHome.Children)
                {
                    if (Grid.GetColumn(img) == pawn.movingTo)
                    {
                        pawn.isAtHome = true;
                        img.Source = new BitmapImage(pawn.imageref);
                    }
                }
            }
        }

        //Undraw the given pawn at thier old location(currentPosition before moving)
        private void removePawn(Pawn pawn)
        {
            if (pawn.currentGrid.Equals(TopGrid.Name))
            {
                foreach (Image img in TopGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
            else if (pawn.currentGrid.Equals(BottomGrid.Name))
            {
                foreach (Image img in BottomGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
            else if (pawn.currentGrid.Equals(LeftGrid.Name))
            {
                foreach (Image img in LeftGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
            else if (pawn.currentGrid.Equals(RightGrid.Name))
            {
                foreach (Image img in RightGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
            else if (pawn.currentGrid.Equals(RedStartGrid.Name))
            {
                foreach (Image img in RedStartGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
            else if (pawn.currentGrid.Equals(YellowStartGrid.Name))
            {
                foreach (Image img in YellowStartGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
            else if (pawn.currentGrid.Equals(GreenStartGrid.Name))
            {
                foreach (Image img in GreenStartGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
            else if (pawn.currentGrid.Equals(BlueStartGrid.Name))
            {
                foreach (Image img in BlueStartGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
            else if (pawn.currentGrid.Equals(RedHomeGrid.Name))
            {
                foreach (Image img in RedHomeGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
            else if (pawn.currentGrid.Equals(YellowHomeGrid.Name))
            {
                foreach (Image img in YellowHomeGrid.Children)
                {
                    if (Grid.GetColumn(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
            else if (pawn.currentGrid.Equals(BlueHomeGrid.Name))
            {
                foreach (Image img in BlueHomeGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
            else if (pawn.currentGrid.Equals(GreenHomeGrid.Name))
            {
                foreach (Image img in GreenHomeGrid.Children)
                {
                    if (Grid.GetRow(img) == pawn.currentPosition)
                    {
                        img.Source = null;
                    }
                }
            }
        }

        // Sets the image event of the given grid and column or row to be true
        private void setImageEventTrue(String gridName, int rowCol)
        {
            foreach (UIElement ui in MainGrid.Children)
            {
                if (ui is Grid)
                {
                    Grid grid = (Grid)ui;
                    if (grid.Name.Equals(gridName))
                    {
                        foreach (Image image in grid.Children)
                        {
                            if (Grid.GetColumn(image) == rowCol && (grid.Name.Equals(TopGrid.Name) || grid.Name.Equals(BottomGrid.Name) || grid.Name.Equals(YellowHomeGrid.Name) || grid.Name.Equals(RedHomeGrid.Name) || grid.Name.Equals(BlueStartGrid.Name) || grid.Name.Equals(GreenStartGrid.Name)))
                            {
                                image.IsEnabled = true;
                            }
                            else if (Grid.GetRow(image) == rowCol && (grid.Name.Equals(LeftGrid.Name) || grid.Name.Equals(RightGrid.Name) || grid.Name.Equals(BlueHomeGrid.Name) || grid.Name.Equals(GreenHomeGrid.Name) || grid.Name.Equals(YellowStartGrid.Name) || grid.Name.Equals(RedStartGrid.Name)))
                            {
                                image.IsEnabled = true;
                            }
                        }
                    }
                }
            }
        }

        // Sets the image event of the given grid and column or row to be false
        private void setImageEventFalse(String gridName, int rowCol)
        {
            foreach (UIElement ui in MainGrid.Children)
            {
                if (ui is Grid)
                {
                    Grid grid = (Grid)ui;
                    if (grid.Name.Equals(gridName))
                    {
                        foreach (Image image in grid.Children)
                        {
                            if (Grid.GetColumn(image) == rowCol && (grid.Name.Equals(TopGrid.Name) || grid.Name.Equals(BottomGrid.Name) || grid.Name.Equals(YellowHomeGrid.Name) || grid.Name.Equals(RedHomeGrid.Name) || grid.Name.Equals(BlueStartGrid.Name) || grid.Name.Equals(GreenStartGrid.Name)))
                            {
                                image.IsEnabled = false;
                            }
                            else if (Grid.GetRow(image) == rowCol && (grid.Name.Equals(LeftGrid.Name) || grid.Name.Equals(RightGrid.Name) || grid.Name.Equals(BlueHomeGrid.Name) || grid.Name.Equals(GreenHomeGrid.Name) || grid.Name.Equals(YellowStartGrid.Name) || grid.Name.Equals(RedStartGrid.Name)))
                            {
                                image.IsEnabled = false;
                            }
                        }
                    }
                }
            }
        }

        // A method that checks if the current player can use the swap action
        private bool checkCanSwap()
        {
            if (checkPawnsAtStartGrid())
            {
                return false;
            }
            else if (checkIfOwnPawnsAreSwappable())
            {
                return false;
            }
            else if (checkAllPawns())
            {
                return false;
            }
            else
            {
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.isAtStart && !pawn.hasIce)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        // A method that checks if the current player can use the sorry action
        private bool checkSorry()
        {
            if (checkPawnAtStartGrid())
            {
                return false;
            }
            else if (checkAllPawns())
            {
                return false;
            }
            else
            {
                foreach(Pawn pawn in currentPlayer.pawns)
                {
                    if(pawn.isAtStart && !pawn.hasIce)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        // A method that changes/shows the menu for splitting depending on the players choice
        private void pullUpMenu()
        {
            if (MessageBox.Show(currentPlayer.username + ", do you wish to split your movement", "7 card", MessageBoxButton.YesNo).ToString().Equals("Yes"))
            {
                SevenMenu.Visibility = Visibility.Visible;
                split = true;
                foreach(Player player in allplayers)
                {
                    foreach(Pawn pawn in player.pawns)
                    {
                        setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                    }
                }
            }
            else
            {
                split = false;
            }
        }

        //The method/event handler called when the cards a clicked
        private async void CardImg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (deck.Count == 0)
            {
                    deck = createDeck();
                    shuffleDeck(deck);
            }
            Card drawnCard = this.deck[deck.Count - 1];
            CardPopupImg.Source = new BitmapImage(drawnCard.cardpic);
            currentCardValue = drawnCard.movement;
            tcs?.TrySetResult(true);
            if (!currentPlayer.isCpu) 
            {
                //If the card value is 1 we must first move the ice token
                if (currentCardValue == 1)
                {
                    MessageBox.Show(currentPlayer.username + ", pick a pawn to move the ice token");
                    moveIceToken();
                }
                else if (currentCardValue == 2)
                {
                    //If the card value is 2 we must ask if they want to move the fire token
                    if (MessageBox.Show(currentPlayer.username + ", do you wish to move the fire token", "2 card", MessageBoxButton.YesNo).ToString().Equals("Yes"))
                    {
                        moveFireToken();
                    }
                }
                else if (currentCardValue == 11)
                {
                    //If the card value is 11 we must check if the player can swap and if they can ask if they want to swap
                    if (checkCanSwap())
                    {
                        if (MessageBox.Show(currentPlayer.username + ", do you wish to swap pawns", "11 card", MessageBoxButton.YesNo).ToString().Equals("Yes"))
                        {
                            MessageBox.Show(currentPlayer.username + ", pick one of your pawns");
                            swapping = true;
                        }
                        else
                        {
                            swapping = false;
                        }
                    }
                }
                else if (currentCardValue == 0)
                {
                    //If the card value is 0 we must check if the player can use sorry and if they can ask if they want to knock a pawn. If they 
                    // would not knock a pawn or can't then move by 4.
                    if (checkSorry())
                    {
                        if (MessageBox.Show(currentPlayer.username + ", do you wish to knock out a player", "Sorry card", MessageBoxButton.YesNo).ToString().Equals("Yes"))
                        {
                            MessageBox.Show(currentPlayer.username + ", pick one of your pawns");
                            sorry = true;
                        }
                        else
                        {
                            sorry = false;
                        }
                    }
                }
                else if (currentCardValue == 7)
                {
                    //If the card value is 7 reset the menu and ask if they want to split. If no move by 7 else wise move two pawns by split
                    RBtn16.IsEnabled = true;
                    RBtn25.IsEnabled = true;
                    RBtn34.IsEnabled = true;
                    pullUpMenu();
                    if (split)
                    {
                        tcs = new TaskCompletionSource<bool>();
                        await tcs.Task;
                        MessageBox.Show("Select a pawn to move by " + splitValue1);
                    }
                }
            }
            deck.RemoveAt(deck.Count - 1);
            CardImg.IsEnabled = false;
            // The remaining statments setDifferent events to true depending on the outcome of questions and cards.
            if (swapping == true)
            {
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (pawn.isAtStart || pawn.isAtHome || pawn.isSafe || pawn.hasIce)
                    {
                        setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                    }
                    else
                    {
                        setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                    }
                }
            }
            else if (sorry == true)
            {
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (pawn.isAtStart && !pawn.hasIce)
                    {
                        setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                    }
                    else
                    {
                        setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                    }
                }
            }
            else if (currentCardValue == 4 && !currentPlayer.isCpu)
            {
                bool[] pawnsCanMove = new bool[3];
                int counter = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce && !pawn.isAtStart && !pawn.isAtHome)
                    {
                        pawnsCanMove[counter] = true;
                        counter++;
                    }
                    else
                    {
                        pawnsCanMove[counter] = false;
                        counter++;
                    }
                }
                if (pawnsCanMove[0] || pawnsCanMove[1] || pawnsCanMove[2])
                {
                    foreach (Pawn pawn in currentPlayer.pawns)
                    {
                        if (!pawn.hasIce && !pawn.isAtStart )
                        {
                            pawn.canMove = true;
                            setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                        }
                        else
                        {
                            pawn.canMove = false;
                            setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(currentPlayer.username + " you have no valid moves.");
                    nextTurn();
                }
            }
            else if (currentCardValue == 10 && !currentPlayer.isCpu)
            {
                bool[] pawnsCanMove = new bool[3];
                int counter = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce && !pawn.isAtStart)
                    {
                        pawnsCanMove[counter] = true;
                        counter++;
                    }
                    else
                    {
                        pawnsCanMove[counter] = false;
                        counter++;
                    }
                }
                if (pawnsCanMove[0] || pawnsCanMove[1] || pawnsCanMove[2])
                {
                    if (MessageBox.Show(currentPlayer.username + ", do you wish to move backwards one space", "10 card", MessageBoxButton.YesNo).ToString().Equals("Yes"))
                    {
                        foreach (Pawn pawn in currentPlayer.pawns)
                        {
                            if (!pawn.hasIce && !pawn.isAtStart)
                            {
                                pawn.canMove = true;
                                setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                            }
                            else
                            {
                                pawn.canMove = false;
                                setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                            }
                        }
                        currentCardValue = -1;
                    }
                    else
                    {
                        foreach (Pawn pawn in currentPlayer.pawns)
                        {
                            if (!pawn.hasIce)
                            {
                                pawn.canMove = true;
                                setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                            }
                            else
                            {
                                pawn.canMove = false;
                                setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show(currentPlayer.username + " you have to move forward.");
                    foreach (Pawn pawn in currentPlayer.pawns)
                    {
                        if (!pawn.hasIce)
                        {
                            pawn.canMove = true;
                            setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                        }
                        else
                        {
                            pawn.canMove = false;
                            setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                        }
                    }
                }
            }
            else
            {
                bool[] pawnsCanMove = new bool[3];
                int counter = 0;
                foreach (Pawn pawn in currentPlayer.pawns)
                {
                    if (!pawn.hasIce && !pawn.isAtHome)
                    {
                        pawnsCanMove[counter] = true;
                        counter++;
                    }
                    else
                    {
                        pawnsCanMove[counter] = false;
                        counter++;
                    }
                }
                if (pawnsCanMove[0] || pawnsCanMove[1] || pawnsCanMove[2])
                {
                    foreach (Pawn pawn in currentPlayer.pawns)
                    {
                        if (!pawn.hasIce && !pawn.isAtHome)
                        {
                            pawn.canMove = true;
                            setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                        }
                        else
                        {
                            pawn.canMove = false;
                            setImageEventFalse(pawn.currentGrid, pawn.currentPosition);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No valid moves");
                    nextTurn();
                }
            }
        }

        //Method that sets the split value 1 and split value 2 once a player has confrimed which kind of split he desires
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement rb in SevenMenu.Items)
            {
                RadioButton temp;
                if ((rb is RadioButton))
                {
                    temp = (RadioButton)rb;
                    if (temp.IsChecked == true)
                    {
                        switch (temp.Name)
                        {
                            case "RBtn16":
                                splitValue1 = 1;
                                splitValue2 = 6;
                                break;

                            case "RBtn25":
                                splitValue1 = 2;
                                splitValue2 = 5;
                                break;

                            case "RBtn34":
                                splitValue1 = 3;
                                splitValue2 = 4;
                                break;
                        }
                        SevenMenu.Visibility = Visibility.Hidden;
                        foreach (Pawn pawn in currentPlayer.pawns)
                        {
                            if (!pawn.hasIce)
                            {
                                pawn.canMove = true;
                                setImageEventTrue(pawn.currentGrid, pawn.currentPosition);
                            }
                        }
                        temp.IsChecked = false;
                        tcs?.TrySetResult(true); 
                        break;
                    }
                    else if (RBtn16.IsChecked == false && RBtn25.IsChecked == false && RBtn34.IsChecked == false)
                    {
                        MessageBox.Show("Select one of the splits");
                        break;
                    }
                }
            }    
        }

        //Save button event
        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Game has been saved");
            GameState serial = new GameState(players, cpus, allplayers, currentPlayer, playerNum, currentPawn, movedSoFar, currentCardValue, deck, movingFireToken, movingIceToken, swapping, sorry, split, splitValue1, splitValue2, firstSelection, specialNeedsPawn);
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            Stream stream = new FileStream("save.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, serial);
            stream.Close();
        }
    }
}