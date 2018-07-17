# DeliveryBoy
Unity remake of Delivery Boy Task

Hints about editing this repo:
The "MainMenu" scene is responsible for calling DeliveryExperiment.ConfigureExperiment, and then loading the MainGame scene.  In the MainGame scene, just look at DeliveryExperiment.Start which launches the coroutine that controls the entire flow of the experiment.

NOTE:
Town Constructor 3/Textures was liscensed from PurpleJump and is too big for github.  Please visit our private UPenn box to access it if you are a member of the team. (system_3_installers/delivery person)

Following are some examples of steps to accomplish various things in DBoy.  As you can see, some things are easier than others.

Steps to add a new store to DBoy:
First, create an object to represent the store:
 

    Open the "MainGame" scene.
    In the object hierarchy, look at the children of the "NamedStores" object
    Copy one (command c) and paste it (command v).  For example, copy the toy_store object.
    Rename your pasted object and make sure it is also a child of NamedStores.  For example, rename it to toy_toy_store.
    Move your object around in the scene view to place your new store.  For example, move it in the negative X direction to position it next to the toy_store.
    Change your object visually in some way.  For example, make it smaller by reducing the "scale" values under the "transform" component.

 

Add your store to the master list of stores:
 
 
 
 

    Under the "MainCoroutine" object's components, examine the DeliveryExperiment script.  Expand the Environments list, then expand Element 0.  Increase the length of the stores array by 1, and drag your new object into the new box.  (Your new object should be a child of "NamedStores").


You will also need to create an object to display during the familiarization phase:
 

    Under the "Faraway Parent" object, under "named stores," again copy one of the stores.  For example, the toy store.
    Again reparent the new object, rename it, and adjust its visual attributes if desired.
    Click the "Faraway Parent" object, and look in the inspector at its "Familiarizer" component.  Increase the size of the store array to 18, and add your new familiarization store.

 

Next, associate the store with audio items for the player to deliver:
 

    Click the "NamedStores" object.  Under the "store names to items" field, increase the size of the mapping to 18.  (Add another store to map to items).
    Expand the added entry in the mapping (most likely "toy store" at the bottom, a duplicate of the previous last entry).  Give it a new name, for example, "toy toy store."
    You will see a list of german and english audio files under your new entry.  These can be reassigned by clicking and dragging new audio files into the boxes.


 
Make sure your store name has english and german translations:

    In the LanguageSource script, edit language_string_dict.  Add an entry with the name of your store object as the key, and an array of english and german names as the values.  For example:  { "toy toy store", new string[] {"toy toy store", "den Spielespielwarenladen"} }

 


If you run the game (remember to deselect "use ramulator"), there will be an additional store during the familiarization phase, and it will be in the world ready to have items delivered.  It won't necessarily be named what you called it, since store names are randomized for each participant.


Steps to increase the number of packages delivered each day by 1:
 

    In the "DeliveryExperiment" script, edit the constant variable DELIVERIES_PER_TRIAL.  Increase its value by 1.
    (Note it's often useful to decrease this number to something low for testing purposes, if you want to get through delivery days quickly.)


 

Steps to reverse the order of cued and free recall:

    First, update your dboy to the latest version from GitHub if you haven't yet.
    In the "DeliveryExperiment" script, locate the "subcoroutine" called DoRecall().
    Reverse the order of the calls to DoFreeRecall and DoCuedRecall.
