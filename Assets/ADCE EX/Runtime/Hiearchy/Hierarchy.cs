using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    [Serializable]
    public class HierarchyEditorAssets
    {
        public GameObject HierarchyItem;
    }

    public class Hierarchy : ADController
    {
        [SerializeField] private HierarchyEditorAssets EditorAssets;

        public override void Init()
        {

        }
    }
}
