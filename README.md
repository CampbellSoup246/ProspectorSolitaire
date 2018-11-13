# ProspectorSolitaire

Project Readme

For me, mostly everything worked just fine except for a few major issues I came across in the textbook, which were solved thankfully relatively quickly, before the actual solution was found part way through.

One of the issues I had was one that someone else described on slack, a very "1" looking symbol actually being an "l" and causing all sorts of issues. Even the font on github maintains this annoying similarity between the two characters, with only a slightly heavier bolding on the 1 and a barely noticeable slant downwards distinguishing the letter from the number. I figured it out fairly quick thanks to that slack post that I remembered about, so it wasn't a major issue, just annoying.

Now onto the three main issues I came across. The first was after initially laying out the 52 cards, and beginning to make them by putting the pips and decos and faces on them. After I finished all the codeand hit run, nothing happened, before I spent the next hour or so trying to find out where in the code I went wrong. FInally when hitting play in Unity, I happened to check the Deck dropdown and open one of the card dropdowns, and sure enough all of the pips and decos were there, just set to INACTIVE. I had no idea why, and neither did classmates, but I figured it out relatively quickly by adding a sprite.SetActive(true); inside of the foreach loop that placed the pips and such, which worked... partially.
After this fix worked, I went on to to do the layout, and suddenly none of the cards in the Target position had any pips. Except a few did, notably the 4 Clubs is when I noticed a few cards kept their sprites. I found another solution for this, in getting rid of the default: case that would supposedly set anything to active for the target card, by instead specifically identifying case "face": case "pip": case "deco":  then the rest of the code, which fixed that.
Then... almost right towards the very end of the project, as I was working on the gold cards to get them to work, I noticed my spritePrefab looked different from the other prefabs... THen I finally noticed it, at some point way towards the beginning when we made the first prefabs, the spritePrefab had somehow been set INACTIVE. That tiny little checkbox at the top left of the inspector pane... When I checked it, and went back and commented out the code I mentioned up at the top to make pips and stuff active, it still worked now, just fine. Scouring through my code and looking everywhere for where in the book I typed it wrong was useless, because the problem was in Unity itself, and was as simple as ticking the checkbox to enable the prefab that all sprites were based off of, so they would actually appear. Go figure.

My other issue was getting the cards to only appear in the mine... I couldn't find a way to do it, or at least not in the deck class where they clearly wanted us to do something, since they had us type in cardGold and cardGoldBack variables at the start of the section in the book essentially. The gold cards work just fine as far as I can tell, they just also appear in the draw pile, though it doesn't seem to be an issue for the game itself.

The last issue I had was with the text for the values of both the GameOver text, the text saying how much you got on winning/losing, and the text that was supposed to float around as you got chains and serve as a little "auto tutorial" or something of the like. After I finished typing all the code in, the stuff was there, it just wasn't moving around at all. Never having dealt with that before, I had no idea how to go about taking care of those things and having the movement actually work, or even what was wrong in the first place.
