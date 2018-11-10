using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Prospector : MonoBehaviour {

    static public Prospector S; //Singletons again.

    public Deck deck;
    public TextAsset deckXML;

    public Layout layout;      //These 2 vars from pg 694, Layout class stuff.
    public TextAsset layoutXML;

    //All below from pg 698
    public Vector3 layoutCenter;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;

    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;

    public List<CardProspector> drawPile; //From pg 696


    void Awake()
    {
        S = this; //Set up a Singleton for Prospector

    }


    // Use this for initialization
    void Start () {
        deck = GetComponent<Deck>(); //Get the Deck
        deck.InitDeck(deckXML.text); //Pass DeckXML to it.
        //Page 686
        Deck.Shuffle(ref deck.cards); //This shuffles the deck. Pg 686
        //The ref keyword passes a refernece to deck.cards, which allows deck.cards to be modified by Deck.Shuffle()

        layout = GetComponent<Layout>(); //Get the Layout       //These two from pg 694, Layout class stuff.
        layout.ReadLayout(layoutXML.text); //Pass LayoutXML to it.

        //Below from pg 696
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame(); //Pg 698
	}

    CardProspector Draw()   //The Draw function will pull a single card from the drawPile and return it.  //This method from pg 698
    {
        CardProspector cd = drawPile[0]; //Pull the 0th CardProspector
        drawPile.RemoveAt(0); //Then remove it from List<> drawPile.
        return (cd);        //And return it
    }
    //PICK UP HERE!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    CardProspector FindCardByLayoutID(int layoutID)  //Convert from the layoutID int to the CardProspector with that ID.  //Pg 709. 
    {

    }
    
    void LayoutGame() //LayoutGame() positions the initial tableau of cards, aka the "mine". //This method also from pg 698
    {
        if(layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");   //Create an empty GameObject named that in the Hierarchy
            layoutAnchor = tGO.transform; //Grab its Transform
            layoutAnchor.transform.position = layoutCenter; //Position it.
        }

        CardProspector cp;
        //Follow the layout
        foreach(SlotDef tSD in layout.slotDefs) //Iterate thru all the SlotDefs in the layout.slotDefs as tSD
        {
            cp = Draw(); //Pull a card from top (beginning) of the drawPile
            cp.faceUp = tSD.faceUp; //Set its faceUp to the value in SlotDef
            cp.transform.parent = layoutAnchor; //Make its parent layoutAnchor
            //This replaces the previous parent: deck.deckAnchor, which appears as _Deck in the Hierarchy when scene plays.
            cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID); //Set localPosition of card based on slotDef
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = CardState.tableau; //CardProspectors in the tableau have the state CardState.tableau

            cp.SetSortingLayerName(tSD.layerName); //Set the sorting layers. This part from pg 703 in Cards.

            tableau.Add(cp); //Add this CardProspector to the List<> tableau.

        }

        MoveToTarget(Draw()); //Set up the initial target card. //From pg 705
        UpdateDrawPile(); //Set up the Draw pile.
    }

    /*  //Copy pasted one from txtbook to fix "Object reference not set to an instance of an object" issue in unity. Didn't work.
    void LayoutGame()
    {                               //	Create	an	empty	GameObject	to	serve	as	an	anchor	for	the tableau	//1								
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");                                                //	^	Create	an	empty	GameObject	named	_LayoutAnchor	in the	Hierarchy												
            layoutAnchor = tGO.transform;                                                                  //	Grab	its Transform												
            layoutAnchor.transform.position = layoutCenter;                       // Position	it								}								
            CardProspector cp;                             //	Follow	the	layout								
            foreach (SlotDef tSD in layout.slotDefs)
            {                                               //	^	Iterate	through	all	the	SlotDefs	in	the layout.slotDefs	as	tSD												
                cp = Draw(); //	Pull	a	card	from	the	top	(beginning)	of the	drawPile												
                cp.faceUp = tSD.faceUp;             //	Set	its	faceUp	to	the	value in	SlotDef												
                cp.transform.parent = layoutAnchor;       //	Make	its	parent layoutAnchor												//	This	replaces	the	previous	parent:	deck.deckAnchor, which	appears												//			as	_Deck	in	the	Hierarchy	when	the	scene	is	playing.												
                cp.transform.localPosition = new Vector3(layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID);                                              //	^	Set	the	localPosition	of	the	card	based	on	slotDef												
                cp.layoutID = tSD.id; cp.slotDef = tSD;
                cp.state = CardState.tableau;                                              //	CardProspectors	in	the	tableau	have	the	state CardState.tableau												
                tableau.Add(cp);    //	Add	this	CardProspector	to	the List<>	tableau								
            }
        }
    } */

        //Below method from pg 696.
    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List < CardProspector > lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector;    //When we treat the Card tCD as a CardProspector, the as returns null instead of a converted Card.
            lCP.Add(tCP);
        }
        return (lCP);
    }

    public void CardClicked(CardProspector cd)  //From pg 704
    {
        switch(cd.state) //The reaction is determined by the state of the clicked card
        {
            case CardState.target: //clcking the target card does nothing
                break;
            case CardState.drawpile: //Clicking any card in the drawPile will draw the next card
                MoveToDiscard(target); //Moves target to discardPile
                MoveToTarget(Draw()); //Moves next drawn card to th target
                UpdateDrawPile(); //Restacks the drawPile
                break;
            case CardState.tableau:
                //Clicking a card in the tableau will chec if it's a valid play
                bool validMatch = true;                                         //This and below pg 708.
                if(!cd.faceUp)  //If it's not an adjacent rank, it's not valid
                {  validMatch = false;   }
                if (!validMatch) return; //Return if not valid
                tableau.Remove(cd); //If it's a valid card, Remove it from the tableau List.
                MoveToTarget(cd);   //Make it the target card.                  //End pg 708 stuff.
                break;
        }
    }

    void MoveToDiscard(CardProspector cd)  //Moves the current target to the discardPile. //From pg 706
    {
        cd.state = CardState.discard; //Set the state of the card to discard
        discardPile.Add(cd); //Add it to the discardPile List<>
        cd.transform.parent = layoutAnchor; //Update its transform parent
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f); //Position it on discardPile
        cd.faceUp = true;
        //Place it on top of pile for depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);

    }
    void MoveToTarget(CardProspector cd)  //Make cd the new target card. //From pg 706
    {
        if (target != null) MoveToDiscard(target); //If there is currently a target card, move it to discardPile
        target = cd; //cd is new target
        cd.state = CardState.target;
        cd.transform.parent = layoutAnchor;
        //Move to target position
        cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);
        cd.faceUp = true; //Make it face-up.
        //Set the depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }
	
    void UpdateDrawPile() //Arranges all the cards of the drawPile to show how many are left. //From pg 706
    {
        CardProspector cd;
        //Go thru all the cards of the drawPile
        for(int i = 0; i< drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;
            //Position it correctly with the layout.drawPile.stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + i*dpStagger.x), layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y), -layout.drawPile.layerID + 0.1f * i);
            cd.faceUp = false; //make them all face down
            cd.state = CardState.drawpile;
            //Set depth sorting
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    public bool AdjacentRank(CardProspector c0, CardProspector c1) //Return true if the two cards are adjcent in rank (A & K wrap around).  //Pg 707!
    {
        if (!c0.faceUp || !c1.faceUp) return (false); //If either card is face down, it's not adjacent.
        if(Mathf.Abs(c0.rank - c1.rank) == 1) { return (true); }  //If they are 1 apart, they are adjacent
        //If one is A and the other King, they're adjacent.
        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank == 13 && c1.rank == 1) return (true);

        //Otherwise, return false
        return (false);
    }
	// Update is called once per frame
	void Update () {
	
	}
}
