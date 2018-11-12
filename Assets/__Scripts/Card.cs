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

    public SpriteRenderer[] spriteRenderers; //List of the SpriteRenderer Components of this GameObject and its children. //From pg 701

    void Start()
    {
        SetSortOrder(0); //ensure that the card starts properly depth sorted. //From pg 701
    }

    public bool faceUp          //from page 684
    {
        get
        {   return(!back.activeSelf);  }
        set
        {   back.SetActive(!value);    }
    }

    public void PopulateSpriteRenderers() //If spriteRenderers is not yet defined, this function defines it. //Pg 702
    {
        if(spriteRenderers == null || spriteRenderers.Length == 0) //If spriteRenderers is null or empty
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();  //Get SpriteRenderer Components of this GO and its children
        }
    }

    public void SetSortingLayerName(string tSLN)    //Sets the sortingLayerName on all SPriteRenderer Components. //Pg 702
    {
        PopulateSpriteRenderers();
        foreach(SpriteRenderer tSR in spriteRenderers)
        {  tSR.sortingLayerName = tSLN;  }
    }

    public void SetSortOrder(int sOrd) //Sets the sortingOrder of all SpriteRenderer Componets
    {
        PopulateSpriteRenderers();

        //The white background of card is on bottom (sOrd).
        //On top of that are all the pips, decorators, face, etc. (sOrd+1)
        //The back is on top s that when visible, it covers the rest (sOrd+2)

        foreach(SpriteRenderer tSR in spriteRenderers) //Iterate thru all the spriteRenderers as tSR
        {
            if(tSR.gameObject == gameObject) //If the gameObject is this.gameObject, it's the background.
            {
                tSR.sortingOrder = sOrd; //Set its order to sOrd
                continue; //And continue tot he next iteration of the loop.
            }
            //Each of the children ofthis GameObject are named
            //switch based on the names
            switch(tSR.gameObject.name)
            {
                case "back": //if name is "back"
                    tSR.sortingOrder = sOrd + 2; //Set it to the highest layer to cover everything else
                    break;
                case "face": //if the name is "face"
                default: //or if its anything else
                    tSR.sortingOrder = sOrd + 1; // set it to the middle layer to be above the background
                    break;
            }
        }
    }
	// Update is called once per frame
	void Update () {
	
	}

    virtual public void OnMouseUpAsButton() //Virtual methods can be overridden by subclass methods with the same name. //Pg 704. This allows cards to be clicked!
    {
        print(name); //When clicked, this outputs card name.
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
