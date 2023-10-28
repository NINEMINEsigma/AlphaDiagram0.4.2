using System.Collections.Generic;
using System.Linq;
using AD.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace AD.Experimental.GameEditor
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New GUISkin", menuName = "AD/GUISkin", order = 10)]
    public class GUISkin : AD.Experimental.EditorAsset.Cache.CacheAssets<AD.Experimental.EditorAsset.Cache.CacheAssetsKey, GUIStyle>
    {
        public GUIStyle FindStyle(string name)
        {
            var result = this.GetData().FirstOrDefault(T => T.key.Equals(name));
            return result == null ? null : result.data;
        }
    }
}
