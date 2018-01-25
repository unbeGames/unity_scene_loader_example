# Unity async loader example

This is an example implementation of Scene Loader that uses Unity Scene Manager API. All scenes are loaded asynchronously and additively. 

## How to run

* Clone the repository or download the archive from the releases section.
* Open project and `managers` scene in Unity.
* Run and play around.

## Notes

* Scene structure is described by `scene_config` and located in Resources folder.
* Scene Loader uses Newtonsoft.Json to deserialise scene config. This dll requires Net 4.6. You can replace it with 3.5 dll version, if you can't use Net 4.6 in your project, or change the way the config data is serialized. 
* All necessary for Scene Loader scripts located in `Assets/scripts/unbeGames`
* Do not forget to add new scenes into Unity build settings and `scene_config`.
* API use examples can be founded in `ApplicationController`
