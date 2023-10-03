using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.Experimental.GameEditor
{
    [Serializable]
    public class PropertiesEditorAssets
    {
        public PropertiesItem PropertiesItemPrefab;
        public ListView PropertiesListView;
        public BehaviourContext behaviourContext;
    }

    public class Properties : ADController
    {
        public PropertiesEditorAssets EditorAssets;

        public List<ISerializePropertiesEditor> TargetTopObjectEditors { get; private set; } = new();

        private void Start()
        {
            GameEditorApp.instance.RegisterController(this);

            EditorAssets.behaviourContext.OnPointerEnterEvent = ADUI.InitializeContextSingleEvent(EditorAssets.behaviourContext.OnPointerEnterEvent, RefreshPanel);
            EditorAssets.behaviourContext.OnPointerExitEvent = ADUI.InitializeContextSingleEvent(EditorAssets.behaviourContext.OnPointerExitEvent, RefreshPanel);
        }

        public override void Init()
        {

        }

        public ISerializePropertiesEditor this[int index]
        {
            get
            {
                if (index < 0 || index > TargetTopObjectEditors.Count)
                {
                    Debug.LogError("Over Bound");
                    return null;
                }
                return TargetTopObjectEditors[index];
            }
            set
            {
                if (index == -1)
                {
                    TargetTopObjectEditors.Add(value);
                }
                else if (index < 0 || index > TargetTopObjectEditors.Count)
                {
                    Debug.LogError("Over Bound");
                    return;
                }
                else TargetTopObjectEditors[index] = value;
            }
        }

        private PropertiesItem RegisterHierarchyItem(ISerializePropertiesEditor target)
        {
            PropertiesItem propertiesItem = EditorAssets.PropertiesListView.GenerateItem() as PropertiesItem;
            target.MatchItem = propertiesItem;
            propertiesItem.MatchEditor = target;
            target.OnSerialize();
            return propertiesItem;
        }

        public void ClearAndRefresh()
        {
            EditorAssets.PropertiesListView.Clear();
            foreach (var item in TargetTopObjectEditors)
            {
                item.MatchItem = RegisterHierarchyItem(item);
            }
        }

        public void RefreshPanel(PointerEventData axisEventData)
        {
            foreach (var target in TargetTopObjectEditors)
            {
                target.OnSerialize();
            }
        }
    }
}