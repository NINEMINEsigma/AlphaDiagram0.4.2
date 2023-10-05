using System.Collections.Generic;
using AD.BASE;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace AD.Experimental.GameEditor
{
    public class GUI
    {
        public static GUISkin skin;
    }

    [System.Serializable]
    public class GUIContent
    {
        internal enum GUIContentType
        {
            None,
            Box
        }

        public GUIContent(string text)
        {
            this.text = text;
        }

        public string text;
        internal UnityEngine.GameObject UIObject;
        internal UnityEngine.Sprite UIImage;
        internal GUIContentType ContentType = GUIContentType.None;

        public void InitContent()
        {
            UIObject.GetComponent<AD.UI.Text>().SetText(text);
        }

        public void InitBox()
        {
            ContentType = GUIContentType.Box;
        }

        public void InitButton(UnityAction call)
        {
            var button = UIObject.GetComponent<AD.UI.Button>();
            button.SetTitle(text);
            button.AddListener(call);
            button.AddListener(() => GameEditorApp.instance.GetController<Properties>().RefreshPanel(null));
        }

        public void InitTextField(BindProperty<string> from)
        {
            var inputField = UIObject.GetComponent<AD.UI.InputField>();
            inputField.Bind(from);
            inputField.SetPlaceholderText(text);
            inputField.AddListener(T => GameEditorApp.instance.GetController<Properties>().RefreshPanel(null));
        }

        public void InitToggle(BindProperty<bool> from)
        {
            var toggle = UIObject.GetComponent<AD.UI.Toggle>();
            toggle.SetTitle(text);
            toggle.AddListener(T => from.Set(T));
            toggle.AddListener(T => GameEditorApp.instance.GetController<Properties>().RefreshPanel(null));
            toggle.Bind(from);
        }

        public static GUIContent Temp(string str)
        {
            GUIContent temp = new GUIContent(str);
            return temp;
        }
    }

    [System.Serializable]
    public class GUIStyle
    {
        public UnityEngine.GameObject Perfab;
        public UnityEngine.Sprite Image;
    }

    public static class PropertiesLayout
    {
        private static ISerializePropertiesEditor CurrentEditorThat;

        private static List<List<GUIContent>> GUILayoutLineList = new();
        private static bool IsNeedMulLine = true;
        private static void DoGUILine(GUIContent content)
        {
            if (!IsNeedMulLine)
            {
                GUILayoutLineList[^1].Add(content);
            }
            else
            {
                GUILayoutLineList.Add(new() { content });
            }
        }

        public static void SetUpPropertiesLayout(ISerializePropertiesEditor target)
        {
            CurrentEditorThat = ADGlobalSystem.FinalCheck(target);
            CurrentEditorThat.MatchItem.Init();
            IsNeedMulLine = true;
        }

        public static void ApplyPropertiesLayout()
        {
            foreach (var line in GUILayoutLineList)
            {
                var rect = CurrentEditorThat.MatchItem.AddNewLevelLine();
                int LineItemCount = line.Count;
                foreach (var content in line)
                {
                    switch (content.ContentType)
                    {
                        case GUIContent.GUIContentType.Box:
                            {
                                rect.GetOrAddComponent<UnityEngine.UI.Image>().sprite = content.UIImage;
                            }
                            break;
                        default:
                            {
                                content.UIObject.transform.SetParent(rect, false);
                                var contentRect = content.UIObject.transform.As<UnityEngine.RectTransform>();
                                contentRect.sizeDelta = new UnityEngine.Vector2(rect.sizeDelta.x / (float)LineItemCount, PropertiesItem.DefaultRectHightLevelSize);
                            }
                            break;
                    }
                }
            }
            CurrentEditorThat = null;
            GUILayoutLineList.Clear();
            EndHorizontal();
        }

        public static void Label(string text)
        {
            DoLabel(GUIContent.Temp(text), GUI.skin.label);
        }
        public static void Label(GUIContent content)
        {
            DoLabel(content, GUI.skin.label);
        }
        public static void Label(string text, GUIStyle style)
        {
            DoLabel(GUIContent.Temp(text), style);
        }
        public static void Label(GUIContent content, GUIStyle style)
        {
            DoLabel(content, style);
        }
        private static void DoLabel(GUIContent content, GUIStyle style)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitContent();
            DoGUILine(content);
        }

        public static void Box()
        {
            DoBox(GUIContent.Temp(""), GUI.skin.box);
        }
        public static void Box(GUIStyle style)
        {
            DoBox(GUIContent.Temp(""), style);
        }
        public static void Box(GUIContent content)
        {
            DoBox(content, GUI.skin.box);
        }
        public static void Box(GUIContent content, GUIStyle style)
        {
            DoBox(content, style);
        }
        private static void DoBox(GUIContent content, GUIStyle style)
        {
            BeginHorizontal();
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitBox();
            DoGUILine(content);
        }

        public static void EndBox()
        {
            EndHorizontal();
        }

        public static void Button(string text, UnityAction call)
        {
            DoButton(GUIContent.Temp(text), GUI.skin.button, call);
        }
        public static void Button(GUIContent content, UnityAction call)
        {
            DoButton(content, GUI.skin.button, call);
        }
        public static void Button(string text, GUIStyle style, UnityAction call)
        {
            DoButton(GUIContent.Temp(text), style, call);
        }
        public static void Button(GUIContent content, GUIStyle style, UnityAction call)
        {
            DoButton(content, style, call);
        }
        private static void DoButton(GUIContent content, GUIStyle style, UnityAction call)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitButton(call);
            DoGUILine(content);
        }

        public static void TextArea(string text)
        {
            var bindProperty = new BindProperty<string>();
            bindProperty.Set(text);
            DoTextField(GUIContent.Temp(""), bindProperty, GUI.skin.textField);
        }
        public static void TextArea(string text, GUIStyle style)
        {
            var bindProperty = new BindProperty<string>();
            bindProperty.Set(text);
            DoTextField(GUIContent.Temp(""), bindProperty, style);
        }
        public static void TextArea(BindProperty<string> bindProperty)
        {
            DoTextField(GUIContent.Temp(""), bindProperty, GUI.skin.textField);
        }
        public static void TextArea(BindProperty<string> bindProperty, GUIStyle style)
        {
            DoTextField(GUIContent.Temp(""), bindProperty, style);
        }
        public static void TextField(string text, BindProperty<string> bindProperty)
        {
            DoTextField(GUIContent.Temp(text), bindProperty, GUI.skin.textField);
        }
        public static void TextField(string text, BindProperty<string> bindProperty, GUIStyle style)
        {
            DoTextField(GUIContent.Temp(text), bindProperty, style);
        }
        public static void TextField(GUIContent content, BindProperty<string> bindProperty)
        {
            DoTextField(content, bindProperty, GUI.skin.textField);
        }
        public static void TextField(GUIContent content, BindProperty<string> bindProperty, GUIStyle style)
        {
            DoTextField(content, bindProperty, style);
        }
        private static void DoTextField(GUIContent content, BindProperty<string> text, GUIStyle style)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitTextField(text);
            DoGUILine(content);
        }

        public static void ValueField(BindProperty<float> bindProperty)
        {
            DoValueField(GUIContent.Temp(""), bindProperty, GUI.skin.textField);
        }
        public static void ValueField(BindProperty<float> bindProperty, GUIStyle style)
        {
            DoValueField(GUIContent.Temp(""), bindProperty, style);
        }
        public static void ValueField(string text, BindProperty<float> bindProperty)
        {
            DoValueField(GUIContent.Temp(text), bindProperty, GUI.skin.textField);
        }
        public static void ValueField(string text, BindProperty<float> bindProperty, GUIStyle style)
        {
            DoValueField(GUIContent.Temp(text), bindProperty, style);
        }
        public static void ValueField(GUIContent content, BindProperty<float> bindProperty)
        {
            DoValueField(content, bindProperty, GUI.skin.textField);
        }
        public static void ValueField(GUIContent content, BindProperty<float> bindProperty, GUIStyle style)
        {
            DoValueField(content, bindProperty, style);
        }
        private static void DoValueField(GUIContent content, BindProperty<float> bindProperty, GUIStyle style)
        {
            BindProperty<string> temp_catch_bind = new();
            PropertyExtension.BindToValue(temp_catch_bind, bindProperty);
            DoTextField(content, temp_catch_bind, style);
        }

        public static void Toggle(BindProperty<bool> value, string text)
        {
            DoToggle(value, GUIContent.Temp(text), GUI.skin.toggle);
        }
        public static void Toggle(BindProperty<bool> value, GUIContent content)
        {
            DoToggle(value, content, GUI.skin.toggle);
        }
        public static void Toggle(BindProperty<bool> value, string text, GUIStyle style)
        {
            DoToggle(value, GUIContent.Temp(text), style);
        }
        public static void Toggle(BindProperty<bool> value, GUIContent content, GUIStyle style)
        {
            DoToggle(value, content, style);
        }
        private static void DoToggle(BindProperty<bool> value, GUIContent content, GUIStyle style)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitToggle(value);
            DoGUILine(content);
        }

        public static void Space(float line)
        {
            EndHorizontal();
            for (int i = 0; i < line; i++)
            {
                DoGUILine(GUIContent.Temp(""));
            }
        }

        public static void BeginHorizontal()
        {
            IsNeedMulLine = false;
            if (GUILayoutLineList.Count == 0 || GUILayoutLineList[^1].Count > 0)
                GUILayoutLineList.Add(new());
        }
        public static void EndHorizontal()
        {
            IsNeedMulLine = true;
        }
    }
}
