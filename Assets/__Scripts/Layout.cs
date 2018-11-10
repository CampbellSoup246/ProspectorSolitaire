using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//ALL OF THIS so far from pg 692.

//The SlotDef class is not a subclas of MonoBehavior, so it doesn't need a separate C# file.
[System.Serializable] //This makes SlotDefs visible in Unity Inspector pane.
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;
}

public class Layout : MonoBehaviour {

    public PT_XMLReader xmlr; //Just like Deck, this has one ofthese.
    public PT_XMLHashtable xml; //This var is for easier xml access
    public Vector2 multiplier; //Sets the spacing of the tableau.
    //SlotDef references
    public List<SlotDef> slotDefs; //All the SlotDefs for Row0-ROw3
    public SlotDef drawPile;
    public SlotDef discardPile;
    //This holds all possible names for the layers set by layerID
    public string[] sortingLayerNames = new string[] { "Row0", "Row1", "Row2", "Row3", "Discard", "Draw" };
    //This func is called to read in the LAyoutXML.xml file
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText); //the XML is parsed
        xml = xmlr.xml["xml"][0]; //And xml is set as a shortcut ot the XML
        //read in the multiplier, which sets card spacing
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));
        //Read in the slots
        SlotDef tSD;
        //slotsX is used as a shortcut to all the <slot>s 
        PT_XMLHashList slotsX = xml["slot"];

        for(int i =0; i<slotsX.Count; i++)
        {
            tSD = new SlotDef(); //Create a new SlotDef instance.
            if(slotsX[i].HasAtt("type"))
            {
                tSD.type = slotsX[i].att("type");  //If this <slot> has a type attribute parse it.
            }
            else
            { tSD.type = "slot"; } //If not, set its type to "slot"; it's a tableau card.

            //Vartious attributes are parsed into numerical values
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            //THis converts the number of the layerID into a text layerName
            tSD.layerName = sortingLayerNames[tSD.layerID];
            //The layers are used to make sure that the correct cards are on top of the others. 
            //In Unity 2D, all of our assets are at ~same Z depth, so the layer is to differentiate between them.

            switch(tSD.type)
            {
                //Pull additional attributes based on the type of this <slot>
                case "slot":
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    if (slotsX[i].HasAtt("hiddenby"))
                    {
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach (string s in hiding)
                        {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }

                    }
                    slotDefs.Add(tSD);
                    break;
                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;

            }
        }
    }


    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
