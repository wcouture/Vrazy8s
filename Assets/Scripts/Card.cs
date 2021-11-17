using UnityEngine;
public enum color{
    RED,
    GREEN,
    BLUE,
    YELLOW
}

public enum type { 
    NUMBER,
    REVERSE,
    DRAW
}


public class Card
{
    color cardColor;
    type cardType;
    int cardNum;

    public Card()
    {
        int rand = Random.Range(1,5);
        switch (rand) {
            case 1: cardColor = color.RED;
                break;
            case 2: cardColor = color.GREEN;
                break;
            case 3: cardColor = color.BLUE;
                break;
            case 4: cardColor = color.YELLOW;
                break;
        }

        rand = Random.Range(1,101);
        if (rand < 80)
        {
            cardType = type.NUMBER;
            cardNum = Random.Range(0, 10);
        }
        else if (rand < 90)
        {
            cardType = type.DRAW;
            cardNum = -1;
        }
        else
        {
            cardType = type.REVERSE;
            cardNum = -1;
        }
    }

    public Card(color cardColor, type cardType, int num)
    {
        this.cardColor = cardColor;
        this.cardType = cardType;
        cardNum = num;
    }

    public color getColor()
    {
        return cardColor;
    }

    public type getType()
    {
        return cardType;
    }

    public int getNum()
    {
        return cardNum;
    }

    public override bool Equals(object obj)
    {
        Card otherCard = (Card)obj;
        if (otherCard.cardType.Equals(type.DRAW))
        {
            return cardType.Equals(otherCard.cardType);
        }
        return cardColor.Equals(otherCard.cardColor) || cardNum == otherCard.cardNum || (!cardType.Equals(type.NUMBER) && cardType.Equals(otherCard.cardType));
    }

    public override string ToString()
    {
        return "Color : " + cardColor + ", Type : " + cardType + ", Number : " + cardNum;
    }
}
