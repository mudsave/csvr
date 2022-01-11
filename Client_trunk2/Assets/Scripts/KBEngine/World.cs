using KBEngine;
using UnityEngine;
using System; 
using System.IO;  
using System.Collections; 
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace KBEngine
{
	public class World : MonoBehaviour
	{
		private static World s_instance;

        private bool m_isLoadComplete = false; //资源是否加载完毕
        public bool isShowLoading = true; //下次切场景,是否显示loading界面

        void Awake()
		{
			if (s_instance != null)
			{
				throw new System.Exception( "World类不允许有超过一份以上的实例!" );
			}
			
			s_instance = this;
		}

		public static World instance
		{
			get {
				if (s_instance == null)
				{
					//new World();
				}
				return s_instance;
			}
		}

        public void OnDestroy()
        {
            s_instance = null;
        }


        /// <summary>
        /// 异步加载（切換）地图
        /// </summary>
        public void LoadSceneAsync(string scene)
		{
            var sceneLoader = SceneManager.LoadSceneAsync(scene);
            SceneManager.sceneLoaded += onSceneLoaded;
        }

        public void onSceneLoaded(Scene scene, LoadSceneMode model)
        {
            Dbg.DEBUG_MSG(string.Format("World::onSceneLoaded(), name '{0}', is loaded '{1}', model '{2}'", scene.name, scene.isLoaded, model));
        }

        public void enterWorld( string scenesName = "Scenes/Demo")
        {
            GlobalEvent.fire("onLoginSuccessfully", new object[] { (UInt64)0, (Int32)0 });

            var sceneLoader = SceneManager.LoadSceneAsync(scenesName);
            SceneManager.sceneLoaded += onSceneLoadedForEnterWorld;
            GlobalEvent.fire("EVENT_OnSceneLoading", new object[] { scenesName, sceneLoader });
        }

        public void onSceneLoadedForEnterWorld(Scene scene, LoadSceneMode model)
        {
            m_isLoadComplete = true;
            Dbg.DEBUG_MSG(string.Format("World::onSceneLoadedForEnterWorld(), name '{0}', is loaded '{1}', model '{2}'", scene.name, scene.isLoaded, model));
            GlobalEvent.fire("playerEnterSpace", new object[] { });

            makePlayerObject(Vector3.zero, Vector3.zero);
            SceneManager.sceneLoaded -= onSceneLoadedForEnterWorld;
        }

        public void leaveWorld()
        {

        }

        /// <summary>
        /// 判断服务器要求的场景客户端是否已加载完成
        /// </summary>
        public bool ScenesIsLoaded()
        {
            return m_isLoadComplete;
        }

        public UnityEngine.GameObject makePlayerObject(Vector3 position, Vector3 direction)
        {
            var rotation = Quaternion.Euler(direction);
            //var obj = UnityEngine.Object.Instantiate(ResourceManager.LoadAssetBundleResource("EntityPrefab/Common"), position, rotation) as GameObject;
            var obj  = Instantiate(Resources.Load("EntityPrefab/Common"), position, rotation) as GameObject;
            obj.name = "PlayerObject";

            //模型加载
            //UnityEngine.Object @object = ResourceManager.LoadAssetBundleResource("[CameraRig]");
            //GameObject cameraRig = (GameObject)UnityEngine.Object.Instantiate(@object, position, rotation);
            UnityEngine.Object @object = Resources.Load("[CameraRig]");
            GameObject cameraRig = (GameObject)Instantiate(@object, position, rotation);
            cameraRig.name = "[CameraRig]";
            cameraRig.transform.parent = obj.transform;
            cameraRig.transform.position = cameraRig.transform.position + new Vector3(0, 0.1f, 0);

            var component = obj.AddComponent<PlayerComponent>();
            component.__init__();
            UnityEngine.Object.DontDestroyOnLoad(obj);
            SceneManager.sceneLoaded += component.OnSceneLoaded;

            return obj;
        }
    }
}
