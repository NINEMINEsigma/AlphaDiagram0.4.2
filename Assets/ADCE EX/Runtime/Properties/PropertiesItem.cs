using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using UnityEngine;

namespace AD.Experimental.GameEditor
{
    public class PropertiesItem : ListViewItem
    {
        public const float DefaultHight = 30;
        public const float DefaultRectHightLevelSize = 20;

        [SerializeField] AD.UI.Toggle Lock_Tilie_Toggle;

        private bool IsLock = false;

        public ISerializePropertiesEditor MatchEditor;

        public override ListViewItem Init()
        {
            InitToggle();
            ClearRectHightLevel();
            return this;
        }

        public void SetTitle(string title)
        {
            Lock_Tilie_Toggle.SetTitle(title);
        }

        private void InitToggle()
        {
            Lock_Tilie_Toggle.Init();
            Lock_Tilie_Toggle.RemoveListener(SwitchLock);
            Lock_Tilie_Toggle.AddListener(SwitchLock);
            Lock_Tilie_Toggle.SetTitle("[ P R O P E R T Y ]");
        }

        private void SwitchLock(bool boolen)
        {
            IsLock = boolen;
        }

        public void ClearRectHightLevel()
        {
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, DefaultHight);
        }

        public void AddRectHightLevel()
        {
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, temp.y + DefaultRectHightLevelSize);
        }

    }
}
