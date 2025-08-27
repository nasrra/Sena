using Godot;
using System;
using System.Threading.Tasks;

public partial class SceneManager : Node{

	private const string levelsResourcePath = "res://scenes/levels/";
	private const string guiResourcePath = "res://scenes/gui/";
	public static SceneManager Singleton{get; private set;}


	public event Action On2DLoaded;
	public event Action On2DUnloaded;
	public event Action On2DDelayedLoad;
	public event Action On2DDelayedUnload;
	
	public event Action On3DLoaded;
	public event Action On3DUnloaded;
	public event Action On3DDelayedLoad;
	public event Action On3DDelayedUnload;

	public event Action OnGuiLoaded;
	public event Action OnGuiUnloaded;
	public event Action OnGuiDelayedLoad;
	public event Action OnGuiDelayedUnload;

	[Export] private Node2D world2D;
	[Export] private Node3D world3D;
	[Export] private Control worldGui;
	[Export] private CanvasLayer gui;

	[Export] private Timer load2DDelayTimer;
	[Export] private Timer unload2DDelayTimer;
	
	[Export] private Timer load3DDelayTimer;
	[Export] private Timer unload3DDelayTimer;
	
	[Export] private Timer loadGuiDelayTimer;
	[Export] private Timer unloadGuiDelayTimer;

	public Node2D Current2D{
		get{
			return (Node2D)world2D.GetChild(0); 
		}
		private set{
			world2D.MoveChild(value, 0);
		}
	}

	public Node3D Current3D{
		get{
			return (Node3D)world3D.GetChild(0); 
		}
		private set{
			world3D.MoveChild(value, 0);
		}
	}
	
	public Control CurrentGui{
		get{
			return (Control)worldGui.GetChild(0); 
		}
		private set{
			worldGui.MoveChild(value, 0);
		}
	}

	[Export] private string start2D;
	[Export] private string start3D;
	[Export] private string startGui;

	private bool is3DLoading 	= false;
	private bool is2DLoading 	= false;
	private bool isGuiLoading 		= false;

	public override void _Ready(){
		base._Ready();
		if(startGui != null){
			LoadScene(
				worldGui, 
				InvokeOnGuiLoaded, 
				startGui, 
				guiResourcePath
			);
		}
		if(start2D != null){
			LoadScene(
				world2D, 
				InvokeOn2DLoaded, 
				start2D, 
				levelsResourcePath
			);		
		}
		if(start3D != null){
			LoadScene(
				world3D, 
				InvokeOn3DLoaded, 
				start3D, 
				levelsResourcePath
			);		
		}
	}


	/// 
	/// Base.
	/// 


	public override void _EnterTree(){
		base._EnterTree();
		Singleton = this;
	}

	public override void _ExitTree(){
		base._ExitTree();
		Singleton = null;
	}


	/// 
	/// Gui.
	/// 


	public void SwapGui(string sceneToLoad, float delayTime){
		if(isGuiLoading==false){
			isGuiLoading = true;
			UnloadScene(
				worldGui, 
				OnGuiUnloaded, 
				OnGuiDelayedUnload, 
				delayedUnloadFinished: ()=>
					LoadScene(
						worldGui, 
						OnGuiLoaded, 
						OnGuiDelayedLoad, 
						InvokeOnGuiLoaded, 
						loadGuiDelayTimer, 
						sceneToLoad, 
						guiResourcePath, 
						delayTime
					),
				unloadGuiDelayTimer, 
				CurrentGui.Name, 
				delayTime
			);
		}
	}

	public void SwapGui(string sceneToLoad){
		if(isGuiLoading==false){
			isGuiLoading = true;
			UnloadScene(
				worldGui, 
				()=>{
					OnGuiUnloaded?.Invoke();
					LoadScene(
						worldGui, 
						InvokeOnGuiLoaded, 
						sceneToLoad, 
						guiResourcePath
					);
				},	
				sceneToLoad
			);
		}
	}

	private void InvokeOnGuiUnloaded(){
		OnGuiUnloaded?.Invoke();
	}

	private void InvokeOnGuiLoaded() {
		isGuiLoading = false;
		OnGuiLoaded?.Invoke();
	}


	/// <summary>
	/// Scene 2D.
	/// </summary>


	public void Swap2D(string sceneToLoad, float delayTime){
		if(is2DLoading==false){
			is2DLoading = true;
			UnloadScene(
				world2D, 
				On2DUnloaded, 
				On2DDelayedUnload, 
				delayedUnloadFinished: ()=>
					LoadScene(
						world2D, 
						On2DLoaded, 
						On2DDelayedLoad, 
						InvokeOn2DLoaded, 
						load2DDelayTimer, 
						sceneToLoad, 
						levelsResourcePath, 
						delayTime
					),
				unload2DDelayTimer, 
				Current2D.Name, 
				delayTime
			);
		}
	}

	public void Swap2D(string sceneToLoad){
		if(is2DLoading==false){
			is2DLoading = true;
			UnloadScene(
				world2D, 
				()=>{
					On2DUnloaded?.Invoke();
					LoadScene(
						world2D, 
						InvokeOn2DLoaded, 
						sceneToLoad, 
						guiResourcePath
					);
				},	
				sceneToLoad
			);
		}
	}

	private void InvokeOn2DLoaded() {
		is2DLoading = false;
		On2DLoaded?.Invoke();
	}

	/// 
	/// Scene 3D.
	/// 



	public void Swap3D(string sceneToLoad, float delayTime){
		if(is3DLoading==false){
			is3DLoading = true;
			UnloadScene(
				world3D, 
				On3DUnloaded, 
				On3DDelayedUnload, 
				delayedUnloadFinished: ()=>
					LoadScene(
						world3D, 
						On3DLoaded, 
						On3DDelayedLoad, 
						InvokeOn3DLoaded, 
						load3DDelayTimer, 
						sceneToLoad, 
						levelsResourcePath, 
						delayTime
					),
				unload3DDelayTimer, 
				Current3D.Name, 
				delayTime
			);
		}
	}

	public void Swap3D(string sceneToLoad){
		if(is3DLoading==false){
			is3DLoading = true;
			UnloadScene(
				world3D, 
				()=>{
					On3DUnloaded?.Invoke();
					LoadScene(
						world3D, 
						InvokeOn3DLoaded, 
						sceneToLoad, 
						guiResourcePath
					);
				},	
				sceneToLoad
			);
		}
	}

	private void InvokeOn3DLoaded() {
		is3DLoading = false;
		On3DLoaded?.Invoke();
	}


	///
	/// Generic.
	/// 

	private void UnloadScene<T>(T parentScene, Action unloadedCallback, string sceneToUnload) where T : Node{
		Node sceneToDelete = parentScene.GetNode<T>(sceneToUnload);
		parentScene.RemoveChild(sceneToDelete);
		sceneToDelete.QueueFree(); // safely queued
		CallDeferredCallback(unloadedCallback);
	}

	private void UnloadScene<T>(T parentScene, Action unloadedCallback, Action delayedUnloadStarted, Action delayedUnloadFinished, Timer delayTimer, string sceneToUnload, float delayTime) where T : Node{
		void TimeoutHandler(){
			delayTimer.Timeout -= TimeoutHandler;
			UnloadScene(parentScene,unloadedCallback, sceneToUnload);
			delayedUnloadFinished?.Invoke();
		}

		delayTimer.Timeout += TimeoutHandler;
		delayTimer.WaitTime = delayTime;
		delayTimer.OneShot = true;
		delayTimer.Start();
		
		delayedUnloadStarted?.Invoke();

	}

	private void LoadScene<T>(T parentScene, Action loadedCallback, string sceneName, string resourceFolderPath) where T : Node{
		PackedScene packedScene = GD.Load<PackedScene>(resourceFolderPath + sceneName + ".tscn");
		T newScene = (T)packedScene.Instantiate();
		parentScene.AddChild(newScene);
		CallDeferredCallback(loadedCallback);

		if (typeof(T) == typeof(Node3D)) {
			Current3D = newScene as Node3D;
		} else if (typeof(T) == typeof(Control)) {
			CurrentGui = newScene as Control;
		}
		else if(typeof(T) == typeof(Node2D)){
			Current2D = newScene as Node2D;
		}
		else{
			throw new Exception();
		}
	}

	private void LoadScene<T>(T parentScene, Action loadedCallback, Action delayedLoadStarted, Action delayedLoadFinished, Timer delayTimer, string sceneToLoad, string resourceFolderPath, float delayTime) where T : Node{
		void TimeoutHandler(){
			delayTimer.Timeout -= TimeoutHandler;
			LoadScene(parentScene, loadedCallback, sceneToLoad, resourceFolderPath);
			delayedLoadFinished?.Invoke();
		}

		delayTimer.Timeout += TimeoutHandler;
		delayTimer.WaitTime = delayTime;
		delayTimer.OneShot = true;
		delayTimer.Start();
		
		delayedLoadStarted?.Invoke();

	}

	private async void CallDeferredCallback(Action callback){
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		callback?.Invoke();
	}

}