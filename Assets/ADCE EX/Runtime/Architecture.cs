using System.Collections;
using System.Collections.Generic;
using AD.BASE;
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

    public interface ISerializeHierarchyEditor
    {

    }

    public interface ISerializeFieldOnHierarchy<_Editor> where _Editor: ISerializeHierarchyEditor
    {

    }


}
