using System.Collections;
using System.Collections.Generic;
using AD.UI;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    public class PropertiesItem : ListViewItem
    {
        [SerializeField] AD.UI.Toggle Lock_Tilie_Toggle;

        public ISerializePropertiesEditor MatchEditor;

        public override ListViewItem Init()
        {
            return this;
        }

        public void SetTitle(string title)
        {
            Lock_Tilie_Toggle.SetTitle(title);
        }
    }
}
