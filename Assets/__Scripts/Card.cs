using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Pg 668
//Class for each individual cardin the deck. Also contain CardDefinition class, holds info as where sprites positioned on each rank of card.

public class Card : MonoBehaviour {

    //These vars from pg 679
    public string suit; //Suit of card
    public int rank; //From 1-14
    public Color color = Color.black; //Color to tint pips
    public string colS = "Black"; //Or "Red". Name of color.
     //This List holds all Decorator GameObjs
    public List<GameObject> decoGOs = new List<GameObject>();
     //List holds all of the Pip GameObjects
    public List<GameObject> pipGOs = new List<GameObject>();
    
    public GameObject back;  //The GOj of the back of card
    public CardDefinition def; //Parse from DeckXML.xml

    public bool faceUp          //from page 684
    {
        get
        {   return(!back.activeSelf);  }
        set
        {   back.SetActive(!value);    }
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

//Pg 668:
[System.Serializable]       //These let them appear in Unity Inspector under the Deck script component apparently.
public class Decorator
{
    //This class stores info about each decorator (the small symbol saying what card type it is) or pip (the things on the card representing its #) from DeckXML
    public string type; //For card pips, type = "pip"  (??)
    public Vector3 loc; // Location of sprite on card
    public bool flip = false;  //whther to flip spirte vertically.
    public float scale = 1f; //the scale of the Sprie.


}

[System.Serializable]
public class CardDefinition
{
    //Class stores info for each rank of card
    public string face; //Sprite to use for each sprite card.
    public int rank; //Rank (1-13) of the card
    public List<Decorator> pips = new List<Decorator>(); //pips used.
    //Since decorators from the XML are used the same way on every card in a deck, pips only stores info about pips on #ed cards.
}
