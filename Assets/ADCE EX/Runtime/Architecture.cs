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

    }

    public interface ICanSerialize
    {
        void OnSerialize();
    }

    public interface ISerializeHierarchyEditor: ICanSerialize
    {
        HierarchyItem MatchItem { get; set; }
        ISerializeFieldOnHierarchy MatchTarget { get; }
        void OnGUI(Vector3[] Rect);
    }

    public interface ISerializeFieldOnHierarchy 
    {
        ISerializeHierarchyEditor MatchEditor { get; set; }
        ISerializeFieldOnHierarchy ParentItem { get; set; }
        ISerializeFieldOnHierarchy GetChild(int index);
        List<ISerializeFieldOnHierarchy> GetChilds();
        void ClickOnLeft();
        void ClickOnRight();
    }

    public interface ISerializePropertiesEditor: ICanSerialize
    {
        //PropertiesItem MatchItem { get; set; }
        void OnGUI(Vector3[] Rect);
    }

    public interface ISerializeFieldOnProperties 
    {

    }


}
