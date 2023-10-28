using System.Collections.Generic;
using System.Linq;
using AD.BASE;
using AD.UI;
using AD.Utility;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static AD.Experimental.GameEditor.GUIContent;

namespace AD.Experimental.GameEditor
{
    public class GUI
    {
        public static GUISkin skin;
    }

    public class GUIContent
    {
        public ADUI TargetItem;
        public GUIContentType ContentType = GUIContentType.Default;
        public int ExtensionalSpaceLine = 1;

        public enum GUIContentType
        {
            Space,
            Default
        }

        public GUIContent(ADUI targetItem, GUIContentType contentType = GUIContentType.Default, int extensionalSpaceLine = 1)
        {
            TargetItem = targetItem;
            ContentType = contentType;
            ExtensionalSpaceLine = extensionalSpaceLine;
        }
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
        public static ADUI GUIField(string text, string style, int extensionalSpaceLine = 1, GUIContentType contentType = GUIContentType.Default)
        {
            GUIStyle targetStyle = ADGlobalSystem.FinalCheckWithThrow(GUI.skin.FindStyle(style), "cannt find this GUIStyle");
            GameObject cat = targetStyle.Perfab.PrefabInstantiate();
            var targetADUIs = cat.GetComponents<ADUI>();
            if (targetADUIs == null || targetADUIs.Length == 0) targetADUIs = cat.GetComponentsInChildren<ADUI>();
            if (targetADUIs == null || targetADUIs.Length == 0)
            {
                GameEditorApp.instance.AddMessage("PropertiesLayout.DoGUI Error");
                return null;
            }
            ADUI targeADUI = targetADUIs.FirstOrDefault(T => T.GetType().Name == targetStyle.TypeName);
            GUIContent content = new(targeADUI, contentType, extensionalSpaceLine);
            if (extensionalSpaceLine != 1)
            {
                EndHorizontal();
            }
            DoGUILine(new GUIContent(ADGlobalSystem.GenerateElement<Text>().SetText(text)));
            DoGUILine(content);
            return targeADUI;
        }
        public static ADUI GUIField(string style, int extensionalSpaceLine = 1, GUIContentType contentType = GUIContentType.Default)
        {
            GUIStyle targetStyle = ADGlobalSystem.FinalCheckWithThrow(GUI.skin.FindStyle(style), "cannt find this GUIStyle");
            GameObject cat = targetStyle.Perfab.PrefabInstantiate();
            var targetADUIs = cat.GetComponents<ADUI>();
            if (targetADUIs == null || targetADUIs.Length == 0) targetADUIs = cat.GetComponentsInChildren<ADUI>();
            if (targetADUIs == null || targetADUIs.Length == 0)
            {
                GameEditorApp.instance.AddMessage("PropertiesLayout.DoGUI Error");
                return null;
            }
            ADUI targeADUI = targetADUIs.FirstOrDefault(T => T.GetType().Name == targetStyle.TypeName);
            GUIContent content = new(targeADUI, contentType, extensionalSpaceLine);
            DoGUILine(content);
            return targeADUI;
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
                        default:
                            {
                                if (content.ExtensionalSpaceLine > 0)
                                    CurrentEditorThat.MatchItem.AddNewLevelLine(content.ExtensionalSpaceLine);
                                content.TargetItem.As<ADUI>().transform.SetParent(rect, false);
                                var contentRect = content.TargetItem.As<ADUI>().transform.As<UnityEngine.RectTransform>();
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

        //public static T GenerateElement<T>() where T : ADUI
        //{
        //    T element = ADGlobalSystem.FinalCheck<T>(ADGlobalSystem.GenerateElement<T>(), "On PropertiesLayout , you try to obtain a null object with some error");
        //}


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
