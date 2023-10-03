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
    }

    public class Hierarchy : ADController
    {
        public HierarchyEditorAssets EditorAssets;

        List<ISerializeHierarchyEditor> TargetTopObjectEditors = new();

        private void Start()
        {
            GameEditorApp.instance.RegisterController(this);

            //EditorAssets.behaviourContext.OnPointerEnterEvent = ADUI.InitializeContextSingleEvent(EditorAssets.behaviourContext.OnPointerEnterEvent, RefreshPanel);
            //EditorAssets.behaviourContext.OnPointerExitEvent = ADUI.InitializeContextSingleEvent(EditorAssets.behaviourContext.OnPointerExitEvent, RefreshPanel);
        }

        public override void Init()
        {

        }

        public HierarchyItem RegisterHierarchyItem(ISerializeHierarchyEditor target)
        {
            HierarchyItem hierarchyItem = EditorAssets.HierarchyListView.GenerateItem() as HierarchyItem;
            target.MatchItem = hierarchyItem;
            hierarchyItem.MatchEditor = target;
            target.OnSerialize();
            target.OnGUI(target.MatchItem.transform.As<RectTransform>().GetRect());
            return hierarchyItem;
        }

        public void RefreshPanel(PointerEventData axisEventData)
        {
            EditorAssets.HierarchyListView.Clear();
            foreach (var item in TargetTopObjectEditors)
            {
                item.MatchItem = RegisterHierarchyItem(item);
            }
        }
    }
}
