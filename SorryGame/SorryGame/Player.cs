using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SorryGame
{
    [Serializable]
    
    // A player with a color,name, pawn[] and check if cpu.
    public class Player
    {
        public String color { get; }
        public Pawn[] pawns { get; set; }
        public String username { get; }
        public Boolean isCpu { get; }

        //Constructor to create players and set their properties
        public Player(String color, Pawn[] pawns, String username, Boolean isCpu)
        {
            this.color = color;
            this.pawns = pawns;
            this.username = username;
            this.isCpu = isCpu;
        }
    }
}
