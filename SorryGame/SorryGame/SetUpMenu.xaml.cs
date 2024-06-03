 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SorryGame
{
    //2nd menu that pops up, initializes the players and cpus
    public partial class SetUpMenu : Window
    {
        private int playerCount;
        private int CPUCount;
        private List<Player> players { get; set; }
        private List<Player> cpus { get; set; }
        public SetUpMenu()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.cpus = new List<Player>();
            this.players = new List<Player>();
        }

        //decreases player count
        private void MinusPlayerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (playerCount != 0)
            {
                playerCount--;
                PlayerCountLbl.Content = playerCount;
            }
        }
        //increases player count
        private void PlusPlayerBtn_Click(object sender, RoutedEventArgs e)
        {
            if (playerCount != 4)
            {
                playerCount++;
                PlayerCountLbl.Content = playerCount;
            }
        }
        //confirms player count, pops up cpu initialization menu
        private void ConfirmPlayerBtn_Click(object sender, RoutedEventArgs e)
        {
            PlusPlayerBtn.Visibility = Visibility.Hidden;
            MinusPlayerBtn.Visibility = Visibility.Hidden;
            ConfirmPlayerBtn.Visibility = Visibility.Hidden;
            PlayerQuestionLbl.Visibility = Visibility.Hidden;
            PlayerCountLbl.Visibility = Visibility.Hidden;
            if (playerCount != 4)
            {
                PlusCPUBtn.Visibility = Visibility.Visible;
                MinusCPUBtn.Visibility = Visibility.Visible;
                ConfirmCPUBtn.Visibility = Visibility.Visible;
                CPUQuestionLbl.Visibility = Visibility.Visible;
                CPUCountLbl.Visibility = Visibility.Visible;
            }
            if (playerCount == 0)
            {
                CPUCount = 2;
                CPUCountLbl.Content = CPUCount;
            }
            else if (playerCount == 1)
            {
                CPUCount = 1;
                CPUCountLbl.Content = CPUCount;
            }
            else if (playerCount == 4)
            {
                ConfirmCPUBtn_Click(sender, new RoutedEventArgs());
            }
        }
        //increases cpu count
        private void PlusCPUBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CPUCount < 4 - playerCount)
            {
                CPUCount++;
                CPUCountLbl.Content = CPUCount;
            }
        }
        //decreases cpu count
        private void MinusCPUBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CPUCount > 0 && CPUCount + playerCount > 2)
            {
                CPUCount--;
                CPUCountLbl.Content = CPUCount;
            }
        }
        //confirms cpu count, pops up player creation menu if there are players
        private void ConfirmCPUBtn_Click(object sender, RoutedEventArgs e)
        {
            if (playerCount != 0)
            {
                EnterNameQuestionLbl.Visibility = Visibility.Visible;
                PlayerNameTxt.Visibility = Visibility.Visible;
                PickColorLbl.Visibility = Visibility.Visible;
                ConfirmPlayerCreationBtn.Visibility = Visibility.Visible;
                SelectColorCBox.Visibility = Visibility.Visible;
                PlusCPUBtn.Visibility = Visibility.Hidden;
                MinusCPUBtn.Visibility = Visibility.Hidden;
                ConfirmCPUBtn.Visibility = Visibility.Hidden;
                CPUQuestionLbl.Visibility = Visibility.Hidden;
                CPUCountLbl.Visibility = Visibility.Hidden;
                EnterNameQuestionLbl.Content = "Player " + playerCount + " enter your name:";
            }
            else
            {
                ConfirmPlayerCreationBtn_Click(sender,new RoutedEventArgs());
            }
        }

        //confirms the player's name and color choice
        private void ConfirmPlayerCreationBtn_Click(object sender, RoutedEventArgs e)
        {
            if (playerCount != 0)
            {
                 if (SelectColorCBox.SelectedItem != null)
                 {
                    Pawn[] pawns = new Pawn[3];
                    for (int i = 0; i < pawns.Length; i++)
                    {
                        pawns[i] = new Pawn(true, false, false, false, false, new Uri(SelectColorCBox.Text.ToLower() + ".png", UriKind.Relative), SelectColorCBox.Text.ToLower(), SelectColorCBox.Text + "StartGrid", false);
                    }
                    Player newplayer = new Player(SelectColorCBox.Text.ToLower(), pawns, PlayerNameTxt.Text, false);
                    players.Add(newplayer);
                    playerCount--;
                    PlayerNameTxt.Clear();
                    SelectColorCBox.Items.Remove(SelectColorCBox.SelectedItem);
                 }
                 ConfirmCPUBtn_Click(new object(), new RoutedEventArgs());
            }
            else
                                                                                  {
                for (int i = 0; i < CPUCount; i++)
                {
                    Pawn[] cpupawns = new Pawn[3];
                    for (int j = 0; j < cpupawns.Length; j++)
                    {
                        cpupawns[j] = new Pawn(true, false, false, false, false, new Uri(SelectColorCBox.Items[0].ToString().ToLower().Substring(38) + ".png", UriKind.Relative), SelectColorCBox.Items[0].ToString().ToLower().Substring(38), SelectColorCBox.Items[0].ToString().Substring(38) + "StartGrid", false); ;
                    }
                    Player cpu = new Player(SelectColorCBox.Items[0].ToString().ToLower().Substring(38), cpupawns, "CPU" + i, true);
                    cpus.Add(cpu);
                    SelectColorCBox.Items.Remove(SelectColorCBox.Items[0]);
                }
                Window win1 = new MainWindow(players, cpus);
                win1.Show();
                this.Close();
            }
        }
    }
}
