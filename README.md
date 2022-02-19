# SRFusionCore: Slime Rancher Fusion Core Mod

Sketch of how the project might work:  
Modders provide a `FusionStrategy` which comprises of 
 - factory :: ([SlimeDefinition], [FusionParameter]) -> SlimeDefinition
 - category :: (String, String)
 - blame :: String
      
And the factory can make use of members in a static helper class
 - list of all pure slimes by very late startup
 - list of all pure slimes' names
 - function that breaks down an ID into component slimes
 - function that constructs the left part of an ID from component slimes
 - function that constructs the right part of an ID from a strategy and parameters
 - function that constructs the full ID from a strategy, component slimes, and parameters
 - manual invocation of a fusion strategy
 - enlisting a strategy in the first place
         
And under the hood this mod
 - enables IDs to contain unique suffixes
 - registers slimes created by manual invocation with the parameters and blame cached in the save registry if run while in a save
 - generates unique slime IDs: component slimes + category + hash of the parameters
 - invokes the strategies on missing IDs that match the blame

And where possible
 - Adds a console command to invoke a blamed strategy, marshalling arguments into it
