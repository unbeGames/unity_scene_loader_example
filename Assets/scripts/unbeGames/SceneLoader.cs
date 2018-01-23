using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace UnbeGames {
  public class SceneLoader : MonoBehaviour {
    public const string none = "none";
    public const string startup = "startup";

    // Use this event if you want show the progress bar
    public static FloatHandler OnProgressTick = delegate {};

    // Use this event to determine when the scene was loaded
    public static StringHandler OnLoad = delegate {};

    Dictionary<SceneConfig.Type, string> loadedScenes = new Dictionary<SceneConfig.Type, string>(){
      { SceneConfig.Type.ui, none },
      { SceneConfig.Type.world, none },
    };

    HashSet<string> loadProgress = new HashSet<string>();
    HashSet<string> unloadProgress = new HashSet<string>();
    
    Dictionary<string, SceneConfig> scenes;

    System.Action finishLoad;
    System.Action finishUnload;
    System.Action checkLoad;
    System.Action checkUnload;

    void Awake(){
      SceneManager.sceneLoaded += OnSceneLoaded;
      SceneManager.sceneUnloaded += OnSceneUnloaded;

      scenes = new Dictionary<string, SceneConfig>();
      var config = Resources.Load<TextAsset>("scene_config");
      foreach(var scene in JsonConvert.DeserializeObject<SceneConfig[]>(config.text)){
        scenes.Add(scene.sceneName, scene);
      }
    }

    void OnDestroy(){
      SceneManager.sceneLoaded -= OnSceneLoaded;
      SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void Load(string name){
      Load(name, delegate {});
    }

    public void Load(string name, System.Action callback){
      var sceneConfig = GetSceneConfig(name);     
      if(sceneConfig.requires != null && sceneConfig.requires.Length != 0){
        foreach(var require in sceneConfig.requires){
          Load(require.sceneName, delegate {
            LoadRest(sceneConfig, callback);
          });
        }
      } else {
        LoadRest(sceneConfig, callback);
      }
    }
    
    public void UnloadAll(System.Action callback){
      UnloadUI(delegate {
        UnloadWorld(delegate {      
          callback.Invoke();
        });
      });
    }

    public void UnloadWorld(System.Action callback){
      if(loadedScenes[SceneConfig.Type.world] == none){
        callback.Invoke();
      } else 
        Unload(loadedScenes[SceneConfig.Type.world], delegate {
          callback.Invoke();
        });
    }

    public void UnloadUI(System.Action callback){
      if(loadedScenes[SceneConfig.Type.ui] == none){
        callback.Invoke();
      } else
        Unload(loadedScenes[SceneConfig.Type.ui], delegate {
          callback.Invoke();
        });
    }

    private void LoadRest(SceneConfig sceneConfig, System.Action callback){    
      PrepareLoad(callback);
      Load(sceneConfig);
      if(sceneConfig.dependencies != null){
        foreach(var dependency in sceneConfig.dependencies){
          Load(GetSceneConfig(dependency.sceneName));
        }
      } 
      PrepareFinishLoad();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode _){
      if(!scenes.ContainsKey(scene.name)){      
        return;
      }
      var config = scenes[scene.name];
      if(!config.isDependency){
        loadedScenes[config.type] = scene.name;
      }
      if(config.setActive){
        SceneManager.SetActiveScene(scene);
      }
      loadProgress.Remove(scene.name);
      OnLoad(scene.name);
      checkLoad.Invoke();  
    }

    private void OnSceneUnloaded(Scene scene){
      if(!scenes.ContainsKey(scene.name))
        return;
      var config = scenes[scene.name];
      if(!config.isDependency){
        loadedScenes[config.type] = none;
      }
      unloadProgress.Remove(scene.name);
      checkUnload.Invoke();
    }

    private void Load(SceneConfig config){
      if(config.isDependency){
        LoadAsync(config.sceneName);
      } else {
        var loaded = loadedScenes[config.type];  
        if(loaded != config.sceneName){
          if (loaded == none){
            LoadAsync(config.sceneName);        
          } else {
            Unload(loaded, delegate {
              LoadAsync(config.sceneName);
            });
          }
        } else {
          CheckAllLoaded();
        }
      }
    }

    private void Unload(string name, System.Action callback){
      PrepareUnload(callback);
      var sceneConfig = GetSceneConfig(name);
      // Load dependencies
      if(sceneConfig.dependencies != null){
        foreach(var dependency in sceneConfig.dependencies){
          Unload(GetSceneConfig(dependency.sceneName));
        }
      }
      Unload(sceneConfig);
      PrepareFinishUnload();
    }

    private void Unload(SceneConfig config){
      UnloadAsync(config.sceneName);
    }

    private void PrepareFinishLoad(){    
      checkLoad = CheckAllLoaded;
    }

    private void PrepareFinishUnload(){
      checkUnload = CheckAllUnloaded;
    }

    private void PrepareLoad(System.Action callback){    
      checkLoad = delegate {};
      finishLoad = callback;
      loadProgress.Clear();
    }

    private void PrepareUnload(System.Action callback){
      checkUnload = delegate {};
      finishUnload = callback;
      unloadProgress.Clear();
    }

    private void CheckAllUnloaded(){
      if(unloadProgress.Count == 0){
        this.finishUnload.Invoke();
      }
    }

    private void CheckAllLoaded(){
      if(loadProgress.Count == 0){
        this.finishLoad.Invoke();  
      }     
    }

    private SceneConfig GetSceneConfig(string name){
      if(!scenes.ContainsKey(name)){
        Helpers.Error(string.Format("Scene with name {0} not found in the scenes config", name));
      }
      return scenes[name];
    }

    private void UnloadAsync(string scene){
      if(scene != none){
        unloadProgress.Add(scene);
        StartCoroutine(UnloadingScene(scene));
      }
    }

    private void LoadAsync(string scene) {
      loadProgress.Add(scene);
      StartCoroutine(LoadingScene(scene));
    }

    private IEnumerator LoadingScene(string scene) {
      var operation = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
      while (!operation.isDone) {
        yield return null;
        OnProgressTick(operation.progress);
      }
    }

    private IEnumerator UnloadingScene(string scene) {
      var operation = SceneManager.UnloadSceneAsync(scene);
      while (operation != null && !operation.isDone) {
        yield return null;
        OnProgressTick(operation.progress);
      }
    }
  }
}
