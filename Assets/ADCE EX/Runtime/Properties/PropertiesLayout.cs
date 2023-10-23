using System.Collections.Generic;
using AD.BASE;
using AD.Utility;
using Unity.VisualScripting;
using UnityEngine;
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
            Space,
            Box
        }

        public GUIContent(string text)
        {
            this.text = text;
        }

        internal string _text;
        public string text { get => _text.Translate(); set => _text = value; }
        internal UnityEngine.GameObject UIObject;
        internal UnityEngine.Sprite UIImage;
        internal int ExtensionalSpaceLine = 0;
        internal GUIContentType ContentType = GUIContentType.None;

        public void InitContent()
        {
            UIObject.GetComponent<AD.UI.Text>().SetText(text.Translate());
        }

        public void InitBox()
        {
            ContentType = GUIContentType.Box;
        }

        public void InitButton(UnityAction call)
        {
            var button = UIObject.GetComponent<AD.UI.Button>();
            button.SetTitle(text.Translate());
            button.AddListener(call);
            button.AddListener(() => GameEditorApp.instance.GetController<Properties>().RefreshPanel(null));
        }

        public void InitTextField(BindProperty<string> from)
        {
            var inputField = UIObject.GetComponent<AD.UI.InputField>();
            inputField.Bind(from);
            inputField.SetPlaceholderText(text.Translate());
            inputField.AddListener(T => GameEditorApp.instance.GetController<Properties>().RefreshPanel(null));
        }

        public void InitToggle(BindProperty<bool> from)
        {
            var toggle = UIObject.GetComponent<AD.UI.Toggle>();
            toggle.SetTitle(text.Translate());
            //toggle.AddListener(T => from.Set(T));
            toggle.AddListener(T => GameEditorApp.instance.GetController<Properties>().RefreshPanel(null));
            toggle.Bind(from);
        }

        public void InitToggle(UnityAction<bool> callback)
        {
            var toggle = UIObject.GetComponent<AD.UI.Toggle>();
            toggle.SetTitle(text.Translate());
            toggle.AddListener(callback);
            toggle.AddListener(T => GameEditorApp.instance.GetController<Properties>().RefreshPanel(null));
            //toggle.Bind(from);
        }

        public void InitColor(BindProperty<Color> from)
        {
            var colorM = UIObject.GetComponent<AD.UI.ColorManager>();
            colorM.ColorProperty = from;
            colorM.title.text = this.text;
            ExtensionalSpaceLine = 13;
        }

        public void InitVectorField(BindProperty<Vector2> from)
        {
            var m2F = UIObject.GetComponent<MulVectorField>();
            m2F.AddListener(T=>
            {
                float x,y;
                if(float.TryParse(T[0],out x)&&float.TryParse(T[1],out y))
                    from.Set(new Vector2(x,y));
                    else
                    {
                        Vector2 temp=from.Get();
                        m2F.Set(temp.x.ToString(),temp.y.ToString());
                    }
            });
        }

        public void InitVectorField(BindProperty<Vector3> from)
        {
            var m3F = UIObject.GetComponent<MulVectorField>();
            m3F.AddListener(T=>
            {
                float x,y,z;
                if(float.TryParse( T[0],out x)&&
                float.TryParse(T[1],out y)&&
                float.TryParse(T[2],out z))
                    from.Set(new Vector3(x,y,z));
                    else
                    {
                        Vector3 temp=from.Get();
                        m3F.Set(temp.x.ToString(),temp.y.ToString(),temp.z.ToString());
                    }
            });
        }
        
        public void InitVectorField(BindProperty<Vector4> from)
        {
            var m4F = UIObject.GetComponent<MulVectorField>();
            m4F.AddListener(T=>
            {
                float x,y,z,w;
                if(float.TryParse(T[0],out x)&&
                float.TryParse(T[1],out y)&&
                float.TryParse(T[2],out z)&&
                float.TryParse(T[3],out w))
                    from.Set(new Vector3(x,y,z));
                    else
                    {
                        Vector3 temp=from.Get();
                        m4F.Set(temp.x.ToString(),temp.y.ToString(),temp.z.ToString());
                    }
            });
        }

        public void InitEnum<_Enum>()
        {
            var enumL = UIObject.GetComponent<DropDown>();
        }

        public void Init_Action(UnityAction<GameObject,string> init)
        {
            init.Invoke(this.UIObject, this.text);
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
                        case GUIContent.GUIContentType.Space:
                            {
                                CurrentEditorThat.MatchItem.AddNewLevelLine(content.ExtensionalSpaceLine);
                            }
                            break;
                        case GUIContent.GUIContentType.Box:
                            {
                                if (content.ExtensionalSpaceLine > 0)
                                    CurrentEditorThat.MatchItem.AddNewLevelLine(content.ExtensionalSpaceLine);
                                rect.GetOrAddComponent<UnityEngine.UI.Image>().sprite = content.UIImage;
                            }
                            break;
                        default:
                            {
                                if (content.ExtensionalSpaceLine > 0)
                                    CurrentEditorThat.MatchItem.AddNewLevelLine(content.ExtensionalSpaceLine);
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

        public static void LabelTextField(string label, string text)
        {
            BindProperty<string> temp = new();
            temp.Set(text);
            DoLabelTextField(GUIContent.Temp(label), temp, GUI.skin.label, GUI.skin.textArea);
        }
        public static void LabelTextField(string label, BindProperty<string> text)
        {
            DoLabelTextField(GUIContent.Temp(label), text, GUI.skin.label, GUI.skin.textArea);
        }
        public static void LabelTextField(string label, string text, GUIStyle label_style)
        {
            BindProperty<string> temp = new();
            temp.Set(text);
            DoLabelTextField(GUIContent.Temp(label), temp, label_style, GUI.skin.textArea);
        }
        public static void LabelTextField(string label, BindProperty<string> text, GUIStyle text_style)
        {
            DoLabelTextField(GUIContent.Temp(label), text, GUI.skin.label, text_style);
        }
        public static void LabelTextField(string label, BindProperty<string> text, GUIStyle label_style, GUIStyle text_style)
        {
            DoLabelTextField(GUIContent.Temp(label), text, label_style, text_style);
        }
        private static void DoLabelTextField(GUIContent label, BindProperty<string> text, GUIStyle label_style, GUIStyle text_style)
        {
            BeginHorizontal();
            Label(label, label_style);
            TextArea(text, text_style);
            EndHorizontal();
        }

        public static void LabelValueField(string label, float value)
        {
            BindProperty<float> temp = new();
            temp.Set(value);
            DoLabelValueField(GUIContent.Temp(label), temp, GUI.skin.label, GUI.skin.textField);
        }
        public static void LabelValueField(string label, BindProperty<float> text)
        {
            DoLabelValueField(GUIContent.Temp(label), text, GUI.skin.label, GUI.skin.textField);
        }
        public static void LabelValueField(string label, float value, GUIStyle label_style)
        {
            BindProperty<float> temp = new();
            temp.Set(value);
            DoLabelValueField(GUIContent.Temp(label), temp, label_style, GUI.skin.textField);
        }
        public static void LabelValueField(string label, BindProperty<float> value, GUIStyle text_style)
        {
            DoLabelValueField(GUIContent.Temp(label), value, GUI.skin.label, text_style);
        }
        public static void LabelValueField(string label, BindProperty<float> value, GUIStyle label_style, GUIStyle text_style)
        {
            DoLabelValueField(GUIContent.Temp(label), value, label_style, text_style);
        }
        private static void DoLabelValueField(GUIContent label, BindProperty<float> text, GUIStyle label_style, GUIStyle text_style)
        {
            BeginHorizontal();
            Label(label, label_style);
            ValueField(text, text_style);
            EndHorizontal();
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

        public static void Toggle(UnityAction<bool> call, string text)
        {
            DoToggle(call, GUIContent.Temp(text), GUI.skin.toggle);
        }
        public static void Toggle(UnityAction<bool> call, GUIContent content)
        {
            DoToggle(call, content, GUI.skin.toggle);
        }
        public static void Toggle(UnityAction<bool> call, string text, GUIStyle style)
        {
            DoToggle(call, GUIContent.Temp(text), style);
        }
        public static void Toggle(UnityAction<bool> call, GUIContent content, GUIStyle style)
        {
            DoToggle(call, content, style);
        }
        private static void DoToggle(UnityAction<bool> call, GUIContent content, GUIStyle style)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitToggle(call);
            DoGUILine(content);
        }

        public static void Space(int line)
        {
            EndHorizontal();
            var temp = GUIContent.Temp("");
            temp.ContentType = GUIContent.GUIContentType.Space;
            temp.ExtensionalSpaceLine = line;
            DoGUILine(temp);
        }

        public static void Color(string text, BindProperty<Color>  property)
        {
            DoColor(GUIContent.Temp(text), GUI.skin.colorField, property);
        }
        public static void Color(GUIContent content, BindProperty<Color> property)
        {
            DoColor(content, GUI.skin.colorField, property);
        }
        public static void Color(string text, GUIStyle style, BindProperty<Color> property)
        {
            DoColor(GUIContent.Temp(text), style, property);
        }
        public static void Color(GUIContent content, GUIStyle style, BindProperty<Color> property)
        {
            DoColor(content, style, property);
        }
        private static void DoColor(GUIContent content, GUIStyle style, BindProperty<Color> property)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitColor(property);
            EndHorizontal();
            DoGUILine(content);
        }

        public static void MulVector(BindProperty<Vector2> property)
        {
            DoMulVector(GUIContent.Temp(""), GUI.skin.MulVector2Field, property);
        }
        public static void MulVector(GUIStyle style, BindProperty<Vector2> property)
        {
            DoMulVector(GUIContent.Temp(""), style, property);
        }
        public static void MulVector(string text, GUIStyle style, BindProperty<Vector2> property)
        {
            DoMulVector(GUIContent.Temp(text), style, property);
        }
        public static void MulVector(GUIContent content, GUIStyle style, BindProperty<Vector2> property)
        {
            DoMulVector(content, style, property);
        }
        public static void MulVector(string text, BindProperty<Vector2> property)
        {
            DoMulVector(GUIContent.Temp(text), GUI.skin.MulVector2Field, property);
        }
        public static void MulVector(GUIContent content, BindProperty<Vector2> property)
        {
            DoMulVector(content, GUI.skin.MulVector2Field, property);
        }

        public static void MulVector(BindProperty<Vector3> property)
        {
            DoMulVector(GUIContent.Temp(""), GUI.skin.MulVector3Field, property);
        }
        public static void MulVector(GUIStyle style, BindProperty<Vector3> property)
        {
            DoMulVector(GUIContent.Temp(""), style, property);
        }
        public static void MulVector(string text, GUIStyle style, BindProperty<Vector3> property)
        {
            DoMulVector(GUIContent.Temp(text), style, property);
        }
        public static void MulVector(GUIContent content, GUIStyle style, BindProperty<Vector3> property)
        {
            DoMulVector(content, style, property);
        }
        public static void MulVector(string text, BindProperty<Vector3> property)
        {
            DoMulVector(GUIContent.Temp(text), GUI.skin.MulVector2Field, property);
        }
        public static void MulVector(GUIContent content, BindProperty<Vector3> property)
        {
            DoMulVector(content, GUI.skin.MulVector2Field, property);
        }

        public static void MulVector(BindProperty<Vector4> property)
        {
            DoMulVector(GUIContent.Temp(""), GUI.skin.MulVector4Field, property);
        }
        public static void MulVector(GUIStyle style, BindProperty<Vector4> property)
        {
            DoMulVector(GUIContent.Temp(""), style, property);
        }
        public static void MulVector(string text, GUIStyle style, BindProperty<Vector4> property)
        {
            DoMulVector(GUIContent.Temp(text), style, property);
        }
        public static void MulVector(GUIContent content, GUIStyle style, BindProperty<Vector4> property)
        {
            DoMulVector(content, style, property);
        }
        public static void MulVector(string text, BindProperty<Vector4> property)
        {
            DoMulVector(GUIContent.Temp(text), GUI.skin.MulVector4Field, property);
        }
        public static void MulVector(GUIContent content, BindProperty<Vector4> property)
        {
            DoMulVector(content, GUI.skin.MulVector4Field, property);
        }

        private static void DoMulVector(GUIContent content, GUIStyle style,BindProperty<Vector2> property)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitVectorField(property);
            EndHorizontal();
            Label(content._text);
            DoGUILine(content);
        }
        private static void DoMulVector(GUIContent content, GUIStyle style, BindProperty<Vector3> property)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitVectorField(property);
            EndHorizontal();
            Label(content._text);
            DoGUILine(content);
        }
        private static void DoMulVector(GUIContent content, GUIStyle style, BindProperty<Vector4> property)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitVectorField(property);
            EndHorizontal();
            Label(content._text);
            DoGUILine(content);
        }

        public static void Enum<_Enum>(GUIContent content, GUIStyle style,UnityAction<_Enum> call)
        {
            DoEnum(content,style,call);
        }
        public static void Enum<_Enum>(string text, GUIStyle style,UnityAction<_Enum> call)
        {
            DoEnum(GUIContent.Temp(text),style,call);
        }
        public static void Enum<_Enum>(GUIContent content,UnityAction<_Enum> call)
        {
            DoEnum(content,GUI.skin.DropDown,call);
        }
        public static void Enum<_Enum>(string text,UnityAction<_Enum> call)
        {
            DoEnum(GUIContent.Temp(text),GUI.skin.DropDown,call);
        }
        private static void DoEnum<_Enum>(GUIContent content, GUIStyle style,UnityAction<_Enum> call)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitEnum<_Enum>(call);
            EndHorizontal();
            BeginHorizontal()
            Label(content._text);
            DoGUILine(content);
            EndHorizontal();
        }

        public static void DropDown(GUIContent content, GUIStyle style,UnityAction<int> call,TOParam string[] operation)
        {
            DoDropDown(content,style,call,operation);
        }
        public static void DropDown(GUIContent content,UnityAction<int> call,TOParam string[] operation)
        {
            DoDropDown(content,GUI.skin.DropDown,call,operation);
        }
        public static void DropDown(string text, GUIStyle style,UnityAction<int> call,TOParam string[] operation)
        {
            DoDropDown(GUIContent.Temp(text),style,call,operation);
        }
        public static void DropDown(string text,UnityAction<int> call,TOParam string[] operation)
        {
            DoDropDown(GUIContent.Temp(text),GUI.skin.DropDown,call,operation);
        }
        private static void DoDropDown(GUIContent content, GUIStyle style,UnityAction<int> call,string[] operation)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.InitDropDown(operation,call);
            EndHorizontal();
            BeginHorizontal()
            Label(content._text);
            DoGUILine(content);
            EndHorizontal();
        }

        public static void Content(GameObject costom)
        {
            DoContent(costom);
        }
        private static void DoContent(GameObject costom)
        {
            var content = GUIContent.Temp("");
            content.UIObject = costom;
            DoGUILine(content);
        }

        public static void CostomField(GUIStyle style)
        {
            DoCostomField(GUIContent.Temp(""), style, (T, P) => { });
        }
        public static void CostomField(GUIStyle style, UnityAction<GameObject, string> init)
        {
            DoCostomField(GUIContent.Temp(""), style, init);
        }
        public static void CostomField(string text, GUIStyle style)
        {
            DoCostomField(GUIContent.Temp(text), style, (T, P) => { });
        }
        public static void CostomField(GUIContent content, GUIStyle style)
        {
            DoCostomField(content, style, (T,P) => { });
        }
        public static void CostomField(string text, GUIStyle style, UnityAction<GameObject, string> init)
        {
            DoCostomField(GUIContent.Temp(text), style, init);
        }
        public static void CostomField(GUIContent content, GUIStyle style, UnityAction<GameObject, string> init)
        {
            DoCostomField(content, style, init);
        }
        private static void DoCostomField(GUIContent content, GUIStyle style, UnityAction<GameObject, string> init)
        {
            content.UIObject = UnityEngine.GameObject.Instantiate(style.Perfab);
            content.Init_Action(init);
            DoGUILine(content);
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
