using System.Collections.Generic;
using AD.BASE;
using UnityEditor;
using UnityEngine;
using AD.UI;
using System;

namespace AD
{
    [CustomEditor(typeof(ADGlobalSystem))]
    public class GlobalSystemEditor : AbstractCustomADEditor
    {
        ADGlobalSystem that = null;

        List<string> buttons = new List<string>();

        protected override void OnEnable()
        {
            base.OnEnable();
            that = target as ADGlobalSystem;

            buttons = new List<string>();
            foreach (var key in that.multipleInputController)
            {
                foreach (var button in key.Key)
                {
                    buttons.Add(button.ToString() + "  ");
                }
            }
            that._IsOnValidate = false;
        }

        public override void OnContentGUI()
        {
            if (that._IsOnValidate)
            {
                buttons = new List<string>();
                foreach (var key in that.multipleInputController)
                {
                    foreach (var button in key.Key)
                    {
                        buttons.Add(button.ToString() + "  ");
                    }
                }
                that._IsOnValidate = false;
            }

            if (GUILayout.Button("Init All ADUI"))
            {
                foreach (var item in ADUI.Items)
                {
                    if (item.Is<ListView>(out var listView)) listView.Init();
                    else if (item.Is<Toggle>(out var toggle)) toggle.Init();
                }
            }
            if (Application.isPlaying)
            {

                EditorGUILayout.Space(25);

                if (buttons.Count == 0) EditorGUILayout.TextArea("No Event was register");
                else foreach (var key in buttons) EditorGUILayout.TextArea(key);

                if (GUILayout.Button("SaveRecord"))
                {
                    that.SaveRecord();
                }
            }
        }

        public override void OnResourcesGUI()
        {
            base.OnDefaultInspectorGUI();
        }

        public override void OnSettingsGUI()
        {
            UnityEngine.Object @object = null;

            EditorGUI.BeginChangeCheck();
            ADGlobalSystem temp_cat = EditorGUILayout.ObjectField("Instance", ADGlobalSystem._m_instance, typeof(ADGlobalSystem), @object) as ADGlobalSystem;
            if (EditorGUI.EndChangeCheck()) ADGlobalSystem._m_instance = temp_cat;

            EditorGUILayout.ObjectField("CurrentADUI", ADUI.CurrentSelect, typeof(ADUI), @object);
        }
    }

}
