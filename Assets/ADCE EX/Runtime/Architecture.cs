using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using AD.Utility;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    public class GameEditorApp : ADArchitecture<GameEditorApp>
    {
        public override bool FromMap(IBaseMap from)
        {
            throw new System.NotImplementedException();
        }

        public override IBaseMap ToMap()
        {
            throw new System.NotImplementedException();
        }

        public override void Init()
        {

        }

        public HierarchyItem CurrentHierarchyItem;

    }

    public static class CustomEditorUtility
    {
        public static HierarchyItem RegisterHierarchyItem(this ISerializeHierarchyEditor self,ISerializeHierarchyEditor target)
        {
            HierarchyItem hierarchyItem = self.MatchItem.ListSubListView.GenerateItem() as HierarchyItem;
            target.MatchItem = hierarchyItem;
            hierarchyItem.MatchEditor = target;
            target.OnSerialize();
            return hierarchyItem;
        }

        public static void SetParent(this ICanSerializeOnCustomEditor self, ICanSerializeOnCustomEditor _Right)
        {
            self.ParentTarget?.GetChilds().Remove(self);
            self.ParentTarget = _Right;
            _Right.GetChilds().Add(self);
        }

        //需要在自定义的ISerializeHierarchyEditor.OnSerialize的最前面使用
        public static void BaseHierarchyItemSerialize(this ISerializeHierarchyEditor self)
        {
            if (self.IsOpenListView)
            {
                foreach (var item in self.MatchTarget.GetChilds())
                {
                    item.MatchHierarchyEditor.BaseHierarchyItemSerialize();
                    item.MatchHierarchyEditor.OnSerialize();
                }
            }
        }

        public static void SetTitle(ISerializeHierarchyEditor editor,string title)
        {
            editor.MatchItem.SetTitle(title);
        }

        public static void SetMaxSubItemCount(ISerializeHierarchyEditor editor, int max)
        {
            editor.MatchItem.ExtensionOpenSingleItemSum = (max - HierarchyItem.MaxOpenSingleItemSum) > 0 ? max - HierarchyItem.MaxOpenSingleItemSum : 1;
        }
    }

    public class CurrentItemSelectOnHierarchyPanel : ADCommand
    {
        public override void OnExecute()
        {
            var cat = Architecture.As<GameEditorApp>().CurrentHierarchyItem;
            if (cat != null)
            {
                Architecture.GetController<Properties>().MatchTarget = cat.MatchEditor.MatchTarget;
                Architecture.GetController<Properties>().ClearAndRefresh();
            }
        }
    }

    public interface ICanSerialize
    {
        void OnSerialize();
        int SerializeIndex { get; set; }
    }

    public interface ISerializeHierarchyEditor: ICanSerialize
    {
        HierarchyItem MatchItem { get; set; }
        ICanSerializeOnCustomEditor MatchTarget { get; }
        bool IsOpenListView { get; set; }
    }

    public interface ICanSerializeOnCustomEditor 
    {
        ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
        List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; }
        ICanSerializeOnCustomEditor ParentTarget { get; set; }
        List<ICanSerializeOnCustomEditor> GetChilds();
        void ClickOnLeft();
        void ClickOnRight();
        int SerializeIndex { get; }
    }

    public interface ISerializePropertiesEditor : ICanSerialize
    {
        PropertiesItem MatchItem { get; set; }
        ICanSerializeOnCustomEditor MatchTarget { get; }
    }
}
