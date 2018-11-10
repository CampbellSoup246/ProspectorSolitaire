using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//ALL THIS so far from pg. 695.

//This is an enum, which dfines a type of variable that only has a few possible named values.
//The CardState variable type has one of four values: drawpile, tableau, target, and discard
public enum CardState
{
    drawpile,
    tableau,
    target,
    discard
}
public class CardProspector : Card  //Making sure CardProspector extends Card!!!
{
    public CardState state = CardState.drawpile; //This is how to use the enum CardState
    public List<CardProspector> hiddenBy = new List<CardProspector>(); //The hiddenBy list stores which other cards will keep this one face down.
    public int layoutID; //LayoutID matches this card to a Layout XML id if it's a tableau card.
    public SlotDef slotDef; //The SlotDef class stores information pulled in from the LayoutXML <slot>


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
