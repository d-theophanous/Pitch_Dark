Requirements:
->	DefaultEngine 
		One of the following:
			put "Runtime/Prefabs/Global Managers" into your own scene
			or copy "Runtime/Scenes/Template Scene" and use as your scene(s)
->	DefaultNetworking

Set Up:
->	Create Folder "Network"
->	Copy DefaultNetworking/Runtime/Scripts/Template/DataObjectTemplate into "Network" 
	-> rename class and namespace
->	Copy DefaultNetworking/Runtime/Scripts/Template/GameMessageTemplate into "Network"
	-> rename class and namespace
->	Copy DefaultNetworking/Runtime/Scripts/Template/SyncObjectTemplate into "Network"
	-> rename class and namespace
> make sure they share the same namespace and resolve errors

->	Edit Global Managers in your scene(s) so that Game Messages Path has the corresponding namespace of your GameMessage class. (Example: <namespace_path>.<name_of_GameMessageTemplate>)
> Start scene and you should see: NetworkScene and NetworkManager should have your GameMessageTemplate object attached

Event Handlers
	static NetworkManager.AfterNetworkStart: Is Executed after any NetworkManager Start-Methode is fired. Can be used reliably to execute NetworkCode as ServerStart of filling Server Data.
	NetworkManager.Client.OnReceiveClientReady: Is executed after the client successfully connected and all data was transmitted (initial connect). After this the client will execute any global (ordered-)messages in order. 

abstract SyncObjects:
	Is used as a container to transfer various data objects.
	Can be used to transmit data on inital connect.
	Will only transfer data and will not handle the creation.
	> override ToLinkedMessage, defines what information of your class will be transmitted through a message
	> override ApplyLinkedMessage, defines how the object should be build from the transmitted data
	> (ushort) HashID must be network unique for every object and generated manually. 

Example:
	Create class and let it inherit from (your renamed) SyncObject class.
	Create NetworkType enum for your created class.
	In your DataObject class define how your SyncObject should be created after transmission. 

Add/Remove/Update SyncObjects
	There are three methods that exist in the ServerMessageHandler class.
	> ServerMessageHandler.Send_SyncObjectAdd, requires your GameMessageTemplate class (i.e. GameMessageTemplate.Server)
	> ServerMessageHandler.Send_SyncObjectRemove, requires your GameMessageTemplate class (i.e. GameMessageTemplate.Server)
	> ServerMessageHandler.Send_SyncObjectUpdate, requires your GameMessageTemplate class (i.e. GameMessageTemplate.Server)
	
Get SyncObject
	SyncObjects are stored locally in 
		> GameMessage.ServerObjects.Dictionary[your NetworkType enum]
		> GameMessage.ClientObjects.Dictionary[your NetworkType enum]
	And can be added / removed
		> GameMessage.ServerObjects.AddSyncObject(i.e. sync_object), vice-versa RemoveSyncObject
		> GameMessage.ClientObjects.AddSyncObject(i.e. sync_object), vice-versa RemoveSyncObject