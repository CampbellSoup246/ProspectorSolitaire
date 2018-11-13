using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum ScoreEvent //An enum to handle all possible scoring events. pg712
{ 
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}
public class Prospector : MonoBehaviour {

    static public Prospector S; //Singletons again.
    static public int SCORE_FROM_PREV_ROUND = 0;    //This one and below from pg 712
    static public int HIGH_SCORE = 0;

    public float reloadDelay = 10f; //The delay between rounds. Page 724    //Changed from 1f to 10f b/c too quick!!

    //These 4 Vector3s from page 721 for scoreboard incorporation
    public Vector3 fsPosMid = new Vector3(0.5f,0.9f,0);
    public Vector3 fsPosRun = new Vector3(0.5f, 0.75f, 0);
    public Vector3 fsPosMid2 = new Vector3(0.5f, 0.5f, 0);
    public Vector3 fsPosEnd = new Vector3(1.0f, 0.65f, 0);

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

    //These vars from pg712 for scoring
    public int chain = 0; //of cards in this run
    public int scoreRun = 0;
    public int  score = 0;

    public FloatingScore fsRun; //Pg 721.

    public Text GTGameOver;     //These two from pg 726
    public Text GTRoundResult;

    void Awake()
    {
        S = this; //Set up a Singleton for Prospector
        //For scoring from page 712
        if (PlayerPrefs.HasKey("ProspectorHighScore")) //Check for a high score in PlayerPrefs
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }
        score += SCORE_FROM_PREV_ROUND; //Add the score from the last round, which will be >0 if it was a win
        SCORE_FROM_PREV_ROUND = 0; //And reset the thing.

        //Below from page 726
        //Set up the GUITexts that show at the end of the round								
        //Get the GUIText Components								
        GameObject go = GameObject.Find("GameOver");
        if (go != null)
        { GTGameOver = go.GetComponent<Text>(); }
        go = GameObject.Find("RoundResult");
        if (go != null) { GTRoundResult = go.GetComponent<Text>(); }
        //Make them invisible					
        ShowResultGTs(false);
        go = GameObject.Find("HighScore");
        string hScore = "High Score: " + Utils.AddCommasToNumber(HIGH_SCORE);
        go.GetComponent<Text>().text = hScore;
    }

    void ShowResultGTs(bool show)   //From pg 726
    {
        GTGameOver.gameObject.SetActive(show);
        GTRoundResult.gameObject.SetActive(show);
    }  
    


    // Use this for initialization
    void Start () {
        Scoreboard.S.score = score; //pg 721

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
    CardProspector FindCardByLayoutID(int layoutID)  //Convert from the layoutID int to the CardProspector with that ID.  //Pg 709. 
    {
        foreach (CardProspector tCP in tableau) //Search thru all cards in the tableau List<>
        {
            if (tCP.layoutID == layoutID)  //If card has same ID, return it
            {
                return (tCP);
            }
        }
        return (null); //If it's not found,r eturn null
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

        //This stuff from pg 709
        foreach(CardProspector tCP in tableau) //Set which cards are hiding others)
        {
            foreach(int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }   //End stuff from pg 709

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
                ScoreManager(ScoreEvent.draw);  //from pg 713 for score
                break;
            case CardState.tableau:
                //Clicking a card in the tableau will chec if it's a valid play
                bool validMatch = true;                                         //This and below pg 708.
                if(!cd.faceUp)  //If it's not an adjacent rank, it's not valid
                {  validMatch = false;   }
                if(!AdjacentRank(cd, target))   //If it's not an adjacent rank, it's not valid.
                { validMatch = false; } 
                if (!validMatch) return; //Return if not valid
                //Next two lines means it has to be valid.
                tableau.Remove(cd); //If it's a valid card, Remove it from the tableau List.
                MoveToTarget(cd);   //Make it the target card.                  //End pg 708 stuff.
                SetTableauFaces(); //Update tableau card face-ups.      //From pg 709
                if (cd.tag.Equals("goldCard"))                                      //MY STUFF to make gold card count as double chain value.
                {
                    ScoreManager(ScoreEvent.mineGold);
                }
                else
                {
                    ScoreManager(ScoreEvent.mine); //From pg 713 for score
                }
                break;
        }
        CheckForGameOver(); //Check to see whether game is over or not.  Pg 710
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

    //My stuff to fix things not appearing in target card for pips and decos.
    //public SpriteRenderer[] spriteRenderers;
    //End my stuff
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
        /*
        //My stuff fix above "my stuff to fix..."
        spriteRenderers = cd.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer tSR in spriteRenderers) //Iterate thru all the spriteRenderers as tSR
        {
            print("in the foreach loop");
            if (tSR.gameObject == cd.gameObject) //If the gameObject is this.gameObject, it's the background.
            {
                print("Did this work");
                tSR.sortingOrder = 0; //Set its order to sOrd
                //continue; //And continue tot he next iteration of the loop.
            }
        }*/
        //end my stuff
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
	
    void SetTableauFaces()  //This turns cards in the Mine faceup or face down.     //Pg 710.
    {
        foreach (CardProspector cd in tableau)
        {
            bool fup = true; //Assume the card will be faceup
            foreach (CardProspector cover in cd.hiddenBy) //If either of the covering cards are in the tableau
            {
                if (cover.state == CardState.tableau)
                {
                    fup = false;    //Then this card is facedown
                }
            }
            cd.faceUp = fup; //Set the value on the card
        }
    }

    void CheckForGameOver() //Testwheter game is over. Pg 710
    {
        if (tableau.Count == 0) //If tableau is empty, the game is over.
        {
            GameOver(true); //Call GameOver with a win
            return;
        }
        if (drawPile.Count > 0) //If are still cards in draw pile, game's not over
        {
            return;
        }
        foreach (CardProspector cd in tableau) //Check for remaining valid plays.
        {
            if (AdjacentRank(cd, target))    //If there is a valid play, game's not over
            {
                return;
            }
        }
        //Since there are no valid plays, game is over
        GameOver(false); //Call GameOver with a loss. 
    }

    void GameOver(bool won) //Called when game is over. Simple for now, but expandable. Pg 711
    {
        if (won)
        {
            //print("Game Over. You won!"); 
            ScoreManager(ScoreEvent.gameWin);
        }
        else
        {
            //print("Game over. You lost."); 
            ScoreManager(ScoreEvent.gameLoss);
        }

        //Reload the scene in reloadDelay seconds. This will give score a moment to travel. //From pg 724, No longer need .LoadScene line.
        Invoke("ReloadLevel", reloadDelay);

        //SceneManager.LoadScene("__Prospector_Scene_0"); //Reload the scene, resetting the game

    }

    void ReloadLevel() //Reload the scene, resetting the game. From pg 724
    {
        SceneManager.LoadScene("__Prospector_Scene_0");
    }

    void ScoreManager(ScoreEvent sEvt) //ScoreManager handles all the scoring.   //Pg 713
    {
        List<Vector3> fsPts; //Pg 721
        FloatingScore fs; //My stuff replacing these vars from ScoreEvent.mine to make room for mineGold one as well. COMMENT OUT IF NEEDED!!!
        switch(sEvt)    //same things need to happen whether its a draw, a win, or a loss. Hence why they are the same
        {
            case ScoreEvent.draw: //Drawing a card
            case ScoreEvent.gameWin: //Won the round
            case ScoreEvent.gameLoss: //Lost the round
                chain = 0; //reset score chain
                score += scoreRun; //Add scoreRun to total score
                scoreRun = 0; //reset scoreRun
                //Below from pg 721. //Add fsRun to the _Scoreboard score
                if(fsRun != null)
                {
                    //Create points for the Bezier curve
                    fsPts = new List<Vector3>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    //Also adjust the fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null; //Clear fsRun so its created again
                }
                break;
            case ScoreEvent.mine: //Remove a mine card
                chain++; //increase score chain
                scoreRun += chain; //add score for this card to run
                //Below from pg 721. 	//	Create	a	FloatingScore	for	this	score
                //FloatingScore	fs;     //Commented out from my stuff up above in top of ScoreManager. COMMENT OUT IF NEED
                //	Move	it	from	the	mousePosition	to	fsPosRun			
                Vector3 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector3>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(chain, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                { fs.reportFinishTo = fsRun.gameObject; }                
                break;
            case ScoreEvent.mineGold:
                chain++; //increase score chain
                //chain *= 2;
                //scoreRun += chain; //add score for this card to run
                scoreRun *= 2;      //Replaces above code to instead double value of the run.
                //Below from pg 721. 	//	Create	a	FloatingScore	for	this	score
                //FloatingScore fs2;
                //	Move	it	from	the	mousePosition	to	fsPosRun			
                Vector3 p02 = Input.mousePosition;
                p02.x /= Screen.width;
                p02.y /= Screen.height;
                fsPts = new List<Vector3>();
                fsPts.Add(p02);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(chain, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                { fs.reportFinishTo = fsRun.gameObject; }
                break;
        }

        switch (sEvt) //Second switch statement handles round wins and loss
        {
            case ScoreEvent.gameWin:
                GTGameOver.text = "Round Over"; //From pg 727
                //If a win, add score to next round
                //sstatic fields are NOT reset by Application.LoadLevel().     //But will they be for .LoadScene()?????**************
                Prospector.SCORE_FROM_PREV_ROUND = score;
                print("You won the round! ROund score: " + score);
                GTRoundResult.text = "You won this round! \nRound Score: " + score; //From pg 727
                ShowResultGTs(true);                    //||
                break;
            case ScoreEvent.gameLoss:
                GTGameOver.text = "Game Over"; //From pg 727
                if (Prospector.HIGH_SCORE <= score) //If its a loss, check against the high score
                {
                    print("You got the high score! High score: " + score);
                    string sRR = "You got the high score!\nHigh score: " + score; //From pg 727
                    GTRoundResult.text = sRR;               //||
                    Prospector.HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else {
                    print("Your final score for the game was: " + score);
                    GTRoundResult.text = "Your final score was: " + score; //From pg 727
                }
                ShowResultGTs(true); //From pg 727
                break;
            default:
                print("Score: " + score + "  scoreRun: " + scoreRun + "  Chain: " + chain);
                break;
        }
    }


}
