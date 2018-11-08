using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//Class that interprets info in DeckXML.xml and uses that info to create entire deck of cards.

public class Deck : MonoBehaviour {


    public PT_XMLReader xmlr;

    //Page 671
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    //Page 675, vars to hold the sprites we made.
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;
    //Prefabs
    public GameObject prefabSprite;
    public GameObject prefabCard;


    //InitDeck called by Prospector when its ready
    public void InitDeck(string deckXMLText)
    {
        //This creates an anchor for all the Card GOs in the Hierarchy. Pg 680
        if(GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }
        //Initialize Dictionary of SuitSprites w/ necessary Sprites. Pg 680
        dictSuits = new Dictionary<string, Sprite>()
        {
            {"C", suitClub },
            {"D", suitDiamond },
            {"H", suitHeart },
            {"S", suitSpade }
        };

        ReadDeck(deckXMLText);  //From pg whenever first started Deck class
        MakeCards(); //From pg 680.
    }

    //ReadDeck parses the XML file passed to it into CardDefinitions
    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLText);  //Uses the PT_XMLReader to parse DeckXML

        //This wil rinta test line to show howxmlr can be used? Huh, interesting.

        string s = "xml[0] decorator[0] ";
        s += "type ="+xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        //print(s);  //Comment out line, dont w/ the test.

        //Read decorators for all Cards
        decorators = new List<Decorator>(); //Init the List of Decorators
        //Grab a PT_XMLHashList of all <decorator>s in the XML file.
        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;
        for(int i = 0; i < xDecos.Count; i++)
        {
            //For each <decorator> in the XML
            deco = new Decorator(); //Make a new Decorator
            //Copy the attributes of the <decorator> to the Decorator
            deco.type = xDecos[i].att("type");
            //Set the bool flip based on whether the text of the attribute is "1" or something else. This is an atypical but perfectly fine ( \/ )
            //   use of the == comparison operator. Will return ture or false, which will be assigned to deco.flip.
            deco.flip = (xDecos[i].att("flip") == "1");
            //floats need to be parsed from the attribute strings.
            deco.scale = float.Parse(xDecos[i].att("scale"));
            //Vector3 loc initializes to [0,0,0], so we just need to modify it.
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));
            //Add the temp deco to the List decorators.
            decorators.Add(deco);

        }

        //Read pip locations for each card num
        cardDefs = new List<CardDefinition>(); //Init the List of Cards.
        //Grab a PT_XMLHashList of all the <pip>s on this <card>
        PT_XMLHashList xCardsDefs = xmlr.xml["xml"][0]["card"];
        for(int i = 0; i<xCardsDefs.Count; i++)
        {
            //For each of the <card>s, Create a new CardDefinition.
            CardDefinition cDef = new CardDefinition();

            //Parse the attribute values and add them to cDef.
            cDef.rank = int.Parse(xCardsDefs[i].att("rank"));
            //Grab a PT_XMLHashList of all the <pip>s on this <card>
            PT_XMLHashList xPips = xCardsDefs[i]["pip"];
            if(xPips != null)
            {
                for(int j = 0; j<xPips.Count; j++)
                {
                    //Iterate through all the <pip>s
                    //deco = new global::Decorator();   //It auto filled to this
                    deco = new Decorator();             //When trying to do this
                    //<pip>s on the <card> are handled via the Decorator Class
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");
                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));
                    if(xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }
                    cDef.pips.Add(deco);

                }
            }

            //Face cards have a face attribute. cDef.face is base name of the face card Sprite.  (FaceCard_11C is Jack o clubs)
            if(xCardsDefs[i].HasAtt("face"))
            {
                cDef.face = xCardsDefs[i].att("face");
            }
            cardDefs.Add(cDef);
        }
    }

    //Get porper CardDefinition based on Rank (1 to 14 is Ace to King). Pg 680
    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        //Search thru all of the CardDefinitions
        foreach(CardDefinition cd in cardDefs)
        {
            //If the rank is correct, return this definition.
            if(cd.rank == rnk)
            {
                return (cd);
            }
            
        }
        return (null);
    }

    //Make the Card GameObjs. Page 680 still
    public void MakeCards()
    {
        //cardNames will be the names of cards to build. Each suit goes from 1 to 13 (C1 to C13 for clubs, for example)
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            for(int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }

        //Make a List to hold all the cards.
        cards = new List<Card>();
        //Several vars that will be reused several times.
        Sprite tS = null;
        GameObject tGO = null;
        SpriteRenderer tSR = null;

        //Iterate thru all the card names that were just made.
        for (int i = 0; i<cardNames.Count; i++)
        {
            //Create a new Card GameObject
            GameObject cgo = Instantiate(prefabCard) as GameObject;
            //Set the transform.parent of new card to the achor
            cgo.transform.parent = deckAnchor;
            Card card = cgo.GetComponent<Card>();
            //This just stacks te cards so they're all in nice rows
            cgo.transform.localPosition = new Vector3((i % 13) * 3, i / 13 * 4, 0);
            //Assigns basic values to the Card
            card.name = cardNames[i];
            card.suit = card.name[0].ToString();
            card.rank = int.Parse(card.name.Substring(1));
            if(card.suit == "D" || card.suit == "H")
            {
                card.colS = "Red";
                card.color = Color.red;
            }
            //Pull the CardDefinition for this card
            card.def = GetCardDefinitionByRank(card.rank);

            //Add Decorators
            int testing = 0;  ///This is my thing trying to get stuff to appear.
            foreach(Decorator deco in decorators)
            {
                if (deco.type == "suit")
                {
                    //Instantiate a Sprite GO
                    tGO = Instantiate(prefabSprite) as GameObject;
                    //Get the SpriteRenderer component.
                    tSR = tGO.GetComponent<SpriteRenderer>();
                    //Set sprite to proper suit
                    tSR.sprite = dictSuits[card.suit];
                }
                else
                {
                    tGO = Instantiate(prefabSprite) as GameObject;
                    tSR = tGO.GetComponent<SpriteRenderer>();
                    //Get the proper Sprite to show this rank
                    tS = rankSprites[card.rank];                    //This doesnt exist yet pg 681??
                    //assign this rank Sprite to the SpriteRenderer
                    tSR.sprite = tS;
                    //set color of rank to match suit
                    tSR.color = card.color;
                }

                //Make the deco Sprites render above the Card
                tSR.sortingOrder = 1;
                //Make decorator Sprite a child of the Card
                tGO.transform.parent = cgo.transform;
                //Set the localPosition based on loc from DeckXML
                tGO.transform.localPosition = deco.loc;
                //Flip the decorator if needed
                if(deco.flip)
                {
                    //An Euler rotation of 180 degrees around the Z-axis will flip it.
                    tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                }                
                //Set the scale to keep decos from being too big
                if(deco.scale != 1)
                {
                    tGO.transform.localScale = Vector3.one * deco.scale;
                }
                //Name this GameObject so its easy to find.
                tGO.name = deco.type;
                //Add this deco GO to the List card.decoGOs
                card.decoGOs.Add(tGO);
                card.decoGOs[testing].SetActive(true);        //My thing to make stuff appear maybe.
                testing++;                                    //Also still my thing.            I THINK IT ACTUALLY WORKS!!!
            }

            //Add Pips. THis stuff below from pg 683.
            //For each of the pips in the definition, Instantiate a Sprite GameObject
            int testingPip = 0;                            //My stuff to make pips active
            foreach(Decorator pip in card.def.pips)
            {
                tGO = Instantiate(prefabSprite) as GameObject;
                //Set the parent to be the card GameObject
                tGO.transform.parent = cgo.transform;
                //Set the position to that specified in the XML
                tGO.transform.localPosition = pip.loc;
                //Flip it if necessary
                if(pip.flip)        //Heheh, pip.flip.
                {
                    tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                }
                //Scale it if necessary (only for the Ace)
                if(pip.scale != 1)
                {
                    tGO.transform.localScale = Vector3.one * pip.scale;
                }
                //Give this GameObject a name
                tGO.name = "pip";
                //Get the SpriteRenderer Component
                tSR = tGO.GetComponent<SpriteRenderer>();
                //Set the Sprite to the proper suit
                tSR.sprite = dictSuits[card.suit];
                //Set sortingOrder so the pip is rendered above the Card_Front
                tSR.sortingOrder = 1;
                //Add this to the Card's list of pips
                card.pipGOs.Add(tGO);
                card.pipGOs[testingPip].SetActive(true);    //More my stuff to make pips active
                testingPip++;                               // Same ^
            }


            //Handle Face Cards
            if(card.def.face != "") //If this has a face in card.def
            {
                tGO = Instantiate(prefabSprite) as GameObject;
                tSR = tGO.GetComponent<SpriteRenderer>();
                //Generate the right name and pass it to GetFace()
                tS = GetFace(card.def.face + card.suit);
                tSR.sprite = tS; //Assings this Sprite to tSR.
                tSR.sortingOrder = 1;
                tGO.transform.parent = card.transform;
                tGO.transform.localPosition = Vector3.zero;
                tGO.name = "face";
                tGO.SetActive(true);                    //MY Thing to make these active.
            }

            //Add Card Back pg 685
            //The Card_Back will be able to cover everything else on the Card
            tGO = Instantiate(prefabSprite) as GameObject;
            tSR = tGO.GetComponent<SpriteRenderer>();
            tSR.sprite = cardBack;
            tGO.transform.parent = card.transform;
            tGO.transform.localPosition = Vector3.zero;
            //This is a higher sortingOrder than anything else
            tSR.sortingOrder = 2;
            tGO.name = "back";
            card.back = tGO;

            //Default to face-up
            card.faceUp = true; //Use the property faceUp of card.  //Change this back to false when finished fixing! ******************

            //Add the card to the deck. Was here before the pg 683 stuff
            cards.Add(card);

        }
    }

    //Find the proper face card Sprite. Pg 683 and 684
    public Sprite GetFace(string faceS)
    {
        foreach(Sprite tS in faceSprites)
        {
            //If this Sprite has the right name... then return the Sprite.
            if(tS.name == faceS)
            {
                return (tS);
            }
        }
        //If nothing found, return null
        return (null);
    }
        /////PICK UP FROM PAGE 684 AND SEE IF THE CARDS ALIGN AND STUFF!!!! ***********************************

    //Shuffle the Cards in Deck.cards  pg 686
    static public void Shuffle(ref List<Card> oCards)  //The ref basically makes so if cards of a Deck passed in via refernece, cards will be shuffled w/o require return var.
    {                                                   //More on page 686 ^
        //Create a temporary List to hold the new shuffle order
        List<Card> tCards = new List<Card>();
        int ndx; //this will hold the index of the card to be moved
        tCards = new List<Card>(); //Initialize the temporary List.
        while(oCards.Count > 0) //Repeat as long as there are cards in the original List
        {
            ndx = Random.Range(0, oCards.Count); //Pick the index of a random card.
            tCards.Add(oCards[ndx]); //Add that card to the temporary List
            oCards.RemoveAt(ndx); //And remove that card from the original List
        }
        oCards = tCards; //Replace the ogiinal List with the temporary List
        //Because oCards is a reference variable, the original that was passed in is changed as well.
    }
}
