/// <summary>
/// Adopted from Unity Wiki's implementation
/// </summary>

using UnityEngine;

namespace m4k {
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
   protected static bool m_ShuttingDown = false;
   private static object m_Lock = new object();
   protected static T _instance;
   public static T I
   {
      get
      {
         if (m_ShuttingDown)
         {
               // Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
               //    "' already destroyed. Returning null.");
               return null;
         }
         lock (m_Lock)
         {
               if(_instance != null) {
                  return _instance;
               }

               _instance = (T)FindObjectOfType(typeof(T));
               // if (_instance == null)
               // {
               //    if (_instance == null) {
               //       // GameObject obj = new GameObject ();
               //       // obj.name = typeof ( T ).Name;
               //       // _instance = obj.AddComponent<T> ();  
               //       Debug.LogError("No instance for: " + typeof(T));
               //    }
               // }

               return _instance;
         }
      }
   }
	protected virtual void Awake ()
	{
		if (_instance == null )
		{
			_instance = this as T;
         m_ShuttingDown = false;
		}
		else if(_instance != this)
		{
			Destroy ( transform.root.gameObject );
         m_ShuttingDown = true;
         Debug.LogWarning($"Existing instance of type {typeof(T).ToString()}");
		}
   }

   private void OnApplicationQuit() {
      m_ShuttingDown = true;
   }

   void OnDisable() {
      m_ShuttingDown = true;
   }

   private void OnDestroy() {
      m_ShuttingDown = true;
   }
}
}