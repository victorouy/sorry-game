using System;

[Serializable]

//A class card, with an image and a value
public class Card
{
    public int movement { get; }
    public Uri cardpic { get; }

    //A constructor allowing you to make a card and set it's constructors
	public Card(int movement, Uri cardpic)
	{
        this.movement = movement;
        this.cardpic = cardpic;
	}
}
