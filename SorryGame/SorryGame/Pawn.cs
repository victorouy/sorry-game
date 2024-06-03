using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SorryGame
{
    [Serializable]

    // A pawn with a grid and position for both current and moving, as well as a color, a boolean for ice and fire token and if they can move, as well as booleans to know special statuses of grids
    public class Pawn 
    {
        public int currentPosition { get; set; }
        public int movingTo { get; set; }
        public String movingToGrid { get; set; }
        public String currentGrid { get; set; }
        public Boolean isAtStart { get; set; }
        public Boolean isAtHome { get; set; }
        public Boolean hasFire { get; set; }
        public Boolean hasIce { get; set; }
        public Uri imageref { get; set; }
        public String color { get; }
        public Boolean isSafe { get; set; }
        public Boolean canMove { get; set; }


        //Cosnstructor for pawns that allow you to set their properties
        public Pawn(Boolean isAtStart, Boolean isAtHome, Boolean isSafe, Boolean hasFire, Boolean hasIce, Uri imageref, String color, String currentGrid, Boolean canMove)
        {
            this.isAtStart = isAtStart;
            this.isAtHome = isAtHome;
            this.hasFire = hasFire;
            this.isSafe = isSafe;
            this.hasIce = hasIce;
            this.imageref = imageref;
            this.color = color;
            this.currentGrid = currentGrid;
            this.currentPosition = -1;
            this.movingTo = -1;
            this.movingToGrid = currentGrid;
            this.canMove = canMove;
        }
    }
}
