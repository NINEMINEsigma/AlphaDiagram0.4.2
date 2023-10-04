using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.Experimental.GameEditor
{
    [Serializable]
    public class HierarchyEditorAssets
    {
        public HierarchyItem HierarchyItemPrefab;
        public ListView HierarchyListView;
        public BehaviourContext behaviourContext;
        public GUISkin skin;
    }

    public class Hierarchy : ADController
    {
        public HierarchyEditorAssets EditorAssets;

        public List<ISerializeHierarchyEditor> TargetTopObjectEditors { get; private set; } = new();

        private void Start()
        {
            GameEditorApp.instance.RegisterController(this);
        }

        public override void Init()
        {
            EditorAssets.behaviourContext.OnPointerEnterEvent = ADUI.InitializeContextSingleEvent(EditorAssets.behaviourContext.OnPointerEnterEvent, RefreshPanel);
            EditorAssets.behaviourContext.OnPointerExitEvent = ADUI.InitializeContextSingleEvent(EditorAssets.behaviourContext.OnPointerExitEvent, RefreshPanel);

            GUI.skin = EditorAssets.skin;
            TargetTopObjectEditors = new();
            ClearAndRefresh();
        }

        public ISerializeHierarchyEditor this[int index]
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

        private HierarchyItem RegisterHierarchyItem(ISerializeHierarchyEditor target)
        {
            HierarchyItem hierarchyItem = EditorAssets.HierarchyListView.GenerateItem() as HierarchyItem;
            target.MatchItem = hierarchyItem;
            hierarchyItem.MatchEditor = target;
            target.OnSerialize();
            return hierarchyItem;
        }

        public void ClearAndRefresh()
        {
            EditorAssets.HierarchyListView.Clear();
            foreach (var item in TargetTopObjectEditors)
            {
                item.MatchItem = RegisterHierarchyItem(item);
                item.MatchItem.SortIndex = item.SerializeIndex;
            }
            EditorAssets.HierarchyListView.SortChilds();
        }

        public void RefreshPanel(PointerEventData axisEventData)
        {
            foreach (var target in TargetTopObjectEditors)
            {
                target.OnSerialize();
                target.MatchItem.SortIndex = target.SerializeIndex;
            }
            EditorAssets.HierarchyListView.SortChilds();
        }

        private void OnApplicationQuit()
        {
            GameEditorApp.instance.SaveRecord();
        }
    }
}
