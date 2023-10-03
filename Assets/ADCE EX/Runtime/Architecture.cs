using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using AD.Utility;
using UnityEditor;
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

        //需要在自定义的OnSerialize的最前面使用
        public static void BaseSerialize(this ICanSerializeOnCustomEditor self)
        {
            var temp = self.GetChilds();
            if (temp != null)
                foreach (var item in temp)
                {
                    //self.RegisterHierarchyItem(item.MatchEditor);
                    item.MatchHierarchyEditor.OnSerialize();
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

    public interface ICanSerialize
    {
        //根据MatchObject进行实际操作
        void OnSerialize();
    }

    public interface ISerializeHierarchyEditor: ICanSerialize
    {
        HierarchyItem MatchItem { get; set; }
        ICanSerializeOnCustomEditor MatchTarget { get; }
    }

    public interface ICanSerializeOnCustomEditor 
    {
        ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
        ISerializePropertiesEditor MatchPropertiesEditor { get; set; }
        ICanSerializeOnCustomEditor ParentTarget { get; set; }
        List<ICanSerializeOnCustomEditor> GetChilds();
        void ClickOnLeft();
        void ClickOnRight();
    }

    public interface ISerializePropertiesEditor: ICanSerialize
    {
        PropertiesItem MatchItem { get; set; }
        ICanSerializeOnCustomEditor MatchTarget { get; }
    }
}
