using System;
using System.Collections.Generic;
using System.IO;
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
    public partial class StartMenu : Window
    {
        public StartMenu()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            Stream stream = new FileStream("save.bin", FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length != 0)
            {
                GameState serial = (GameState)formatter.Deserialize(stream);
                Window board = new MainWindow(serial.players, serial.cpus, serial.allplayers, serial.currentPlayer, serial.playerNum, serial.currentPawn, serial.movedSoFar, serial.currentCardValue, serial.deck, serial.movingFireToken, serial.movingIceToken, serial.swapping, serial.sorry, serial.split, serial.splitValue1, serial.splitValue2, serial.firstSelection, serial.specialNeedsPawn);
                stream.Close();
                board.Show();
                this.Close();
            }
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            Window SetUpwin = new SetUpMenu();
            SetUpwin.Show();
            this.Close();
        }
    }
}
