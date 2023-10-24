using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace AD.Experimental.GameEditor
{
    [System.Serializable]
    public class GUISkin
    {
        public GUIStyle label;
        public GUIStyle box;
        public GUIStyle button;
        public GUIStyle textField;
        public GUIStyle textArea;
        public GUIStyle toggle;
        public GUIStyle colorField;
        public GUIStyle MulVector2Field;
        public GUIStyle MulVector3Field;
        public GUIStyle MulVector4Field;
        public GUIStyle DropDown;
        public List<GUIStyle> CustomStyles = new();

        public GUIStyle FindStyle(string name)
        {
            switch (name)
            {
                case "label":
                    {
                        return label;
                    }
                case "box":
                    {
                        return box;
                    }
                case "button":
                    {
                        return button;
                    }
                case "textField":
                    {
                        return textField;
                    }
                case "textArea":
                    {
                        return textArea;
                    }
                case "toggle":
                    {
                        return toggle;
                    }
                case "colorField":
                    {
                        return colorField;
                    }
                case "MulVector2Field":
                    {
                        return MulVector2Field;
                    }
                case "MulVector3Field":
                    {
                        return MulVector3Field;
                    }
                case "MulVector4Field":
                    {
                        return MulVector4Field;
                    }
                case "Dropdown":
                    {
                        return DropDown;
                    }
                default:
                    var result = CustomStyles.FirstOrDefault(T => T.Perfab.name.Equals(name));
                    if (result != null) return result;
                    else return null;
            }
        }
    }
}
