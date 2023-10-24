using UnityEditor;
using UnityEngine;

namespace AD.UI
{
    public class Dropdown : ADUI
    {
        private UnityEngine.UI.Dropdown _source;
        public UnityEngine.UI.Dropdown source
        {
            get
            {
                if (_source == null) _source = GetComponent<UnityEngine.UI.Dropdown>();
                return _source;
            }
        }

        public Dropdown()
        {
            ElementArea = "Dropdown";
        }

        protected void Start()
        {
            AD.UI.ADUI.Initialize(this);
        }
        protected void OnDestroy()
        {
            AD.UI.ADUI.Destory(this);
        }

        public void Init()
        {
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/AD/Dropdown", false, 10)]
        private static void ADD(UnityEditor.MenuCommand menuCommand)
        {
            AD.UI.Dropdown dropDown = GameObject.Instantiate(ADGlobalSystem.instance._DropDown);
            dropDown.name = "New Dropdown";
            GameObjectUtility.SetParentAndAlign(dropDown.gameObject, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(dropDown.gameObject, "Create " + dropDown.name);
            Selection.activeObject = dropDown.gameObject;
        }
#endif

        public static AD.UI.Dropdown Generate(string name = "New Dropdown", Transform parent = null)
        {
            AD.UI.Dropdown dropDown = GameObject.Instantiate(ADGlobalSystem.instance._DropDown);
            dropDown.transform.SetParent(parent, false);
            dropDown.transform.localPosition = Vector3.zero;
            dropDown.name = name;
            return dropDown;
        }
    }
}
