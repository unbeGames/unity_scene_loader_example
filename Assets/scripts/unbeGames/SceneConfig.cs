namespace UnbeGames {
	public class SceneConfig {
		public struct Dependency {
			public string sceneName;			
		}

		public enum Type {
			ui,
			world,
			dependency
		}

		public string sceneName;
		public Type type;
		public bool setActive;
		public Dependency[] dependencies;
		public Dependency[] requires;

		public bool isDependency => type == Type.dependency;
	}
}
