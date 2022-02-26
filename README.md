# SRFusionCore: Slime Rancher Fusion Core Mod

Modders provide a `FusionCore.Strategy` which contains a factory delegate and some metadata
      
The strategy's factory can make use of members in a static helper class
 - list of all pure slimes by very late startup
 - list of all pure slimes' names
 - function that breaks down an ID into pure component slimes
 - function that constructs the left part of an ID from pure component slimes
 - function that constructs the right part of an ID from a strategy and parameters
 - function that constructs the full ID from a strategy, component slimes, and parameters
 - function that constructs a sensible display name from a strategy and component slimes
 - function to easily set the display name of a slime
 - function that creates a new named ID at runtime
 - manual invocation of a fusion strategy    
         
And under the hood this mod
 - enables IDs to contain unique suffixes
 - registers slimes created by manual invocation with the parameters and blame cached in the save registry if run while in a save
 - generates unique slime IDs: component slimes + category + hash of the strategy, components, parameters
 - invokes the strategies on missing IDs that match the blame allowing saves to be loaded 
         
And adds a console command `fuse` to invoke a blamed strategy, marshalling arguments into it         
`fuse <mode> <components> <parameters...>`
