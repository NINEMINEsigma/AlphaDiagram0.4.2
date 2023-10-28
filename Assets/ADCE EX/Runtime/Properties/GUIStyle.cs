using UnityEngine;

namespace AD.Experimental.GameEditor
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New GUIStyle", menuName = "AD/GUIStyle", order = 11)]
    public class GUIStyle:AD.Experimental.EditorAsset.Cache.AbstractScriptableObject
    {
        public UnityEngine.GameObject Perfab;
        public string TypeName;
    }
}
