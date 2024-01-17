
/*
Ink Cheat sheet

=== cat ===
marks a "knot", which is a dialogue, monologue, or cutscene accessible through in-game interaction. They are the entry point to any dialogues.

= cat_angry
marks a "stitch", or a passage within the knot that can be referenced. 

* [normal option] can be choosen only once
+ [sticky option] won't diappear (useful for navigation and interaction start/end)

-> divert aka links to stitch or knot

//single line comment

//choice conditioned to visited passage
* {cat_angry} [Pet the cat]


//conditions if/else
{
-passage_title:
    do this
-VARIABLE_NAME >= 2:
    do that
-else:
    otherwise
}

*/

//declaring a function to trigger on the unity side
EXTERNAL teleport(objectName)
EXTERNAL activate(objectName)
EXTERNAL deactivate(objectName)
EXTERNAL gameEvent(eventName)

//I like to use allcaps for global variable to distinguish them from knots ids
VAR KEY = false
VAR LEVEL = 0

//optional introduction paragraph
== intro ===
You wake up in a pink desert.
->END

== cat ===
Cat: Meow.
->choices

= choices
* [Pet the cat] -> pet
* [Talk to the cat] -> talk
+ [Ignore the cat] -> END

= pet
You attempt to pet the cat but it scratches you.
->choices

= talk
You: Hello kitty.
Cat: Leave me alone capsule.
->choices

=== cube ===
It's an obstacle shaped like a cube.
->END

=== pillar ===
You can call a function from Ink to activate a gameobject.
+ [Deactivate the cone] -> deactivateCone
+ [Activate the cone] -> activateCone

= deactivateCone
Bzzz
~ deactivate("cone") 
->END

= activateCone
Bzzzzz
~ activate("cone")
->END


=== portalA ===
~ teleport("orangePortal")
->END

=== portalB ===
Entering the portal!
~ teleport("bluePortal")
->END


=== door ===
{
- KEY == true: -> open
- else: -> locked
}

= open
The door is open!
~ teleport("bluePortal")
->END

= locked
You need a key to open the door
->END

=== key ===
Picking up the key
~ deactivate("key")
~ KEY = true
->END

=== exit ===
The End
~ gameEvent("restart")
->END

//example of "room" change
=== pinkToBlue ===
~ activate("blueDesert")
~ deactivate("pinkDesert")
~ teleport("pinkDoor")
->END

=== blueToPink ===
~ activate("pinkDesert")
~ deactivate("blueDesert")
~ teleport("blueDoor")
->END
