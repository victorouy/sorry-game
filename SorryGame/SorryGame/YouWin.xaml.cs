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
    //A window to let the players know who has won
    public partial class YouWin : Window
    {
        private Player winner;

        //A constructor for the window that takes as input a winner
        public YouWin(Player player)
        {
            InitializeComponent();
            this.winner = player;
            Winnerlbl.Content = "Congrats " + winner.username + " you win!";
        }

        //Calls the setup menu to start another game
        private void Restartbtn_Click(object sender, RoutedEventArgs e)
        {
            Window SetUpwin = new SetUpMenu();
            SetUpwin.Show();
            this.Close();
        }

        //Closes the window
        private void Closebtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
