using System.Collections.Generic;
using AD.BASE;
using AD.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AD.Experimental.GameEditor
{
    public class HierarchyItem : ListViewItem
    {
        public const float DefaultHight = 20;
        public const int MaxOpenSingleItemSum = 10;
        [SerializeField] private AD.UI.Toggle ListToggle;
        public AD.UI.ListView ListSubListView;
        public int ExtensionOpenSingleItemSum = -1;

        RegisterInfo __unregisterInfo, __unregisterInfo_click;

        public ISerializeHierarchyEditor MatchEditor;

        public override ListViewItem Init()
        {
            ListSubListView.SetPrefab(GameEditorApp.instance.GetController<Hierarchy>().EditorAssets.HierarchyItemPrefab);
            RegisterClickCallback();
            InitToggle();
            ClearRectHightLevel();
            return this;
        }

        public void SetTitle(string title)
        {
            ListToggle.SetTitle(title);
        }

        private void InitToggle()
        {
            ListToggle.Init();
            ListToggle.RemoveListener(Refresh);
            ListToggle.AddListener(Refresh);
            ListToggle.SetTitle("[ N U L L ]");
        }

        private void RegisterClickCallback()
        {
            __unregisterInfo?.UnRegister();
            if (__unregisterInfo == null)
                __unregisterInfo = ADGlobalSystem.AddListener(Mouse.current.rightButton, () =>
                {
                    if (!ListToggle.Selected) return;
                    MatchEditor?.MatchTarget.ClickOnRight();
                }, PressType.ThisFramePressed);
            else __unregisterInfo.TryRegister();

            __unregisterInfo_click?.UnRegister();
            if (__unregisterInfo_click == null)
                __unregisterInfo_click = ADGlobalSystem.AddListener(Mouse.current.leftButton, () =>
                {
                    if (!ListToggle.Selected) return;
                    MatchEditor?.MatchTarget.ClickOnLeft();
                }, PressType.ThisFramePressed);
            else __unregisterInfo_click.TryRegister();
        }

        public void Refresh(bool boolen)
        {
            if (boolen == true) OpenListView(); else CloseListView();
        }

        public void AddRectHightLevel(int t)
        {
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, temp.y + DefaultHight * t);
        }

        public void ClearRectHightLevel()
        {
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, DefaultHight);
        }

        private void OpenListView()
        {
            if (MatchEditor == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("MatchEditor is null");
                return;
#else
                throw new ADException("MatchEditor is null");
#endif
            }
            var ChildItems = MatchEditor.MatchTarget.GetChilds();
            if (ChildItems == null) return;
            int ChildsSum = MatchEditor.MatchTarget.GetChilds().Count;

            MatchEditor.MatchTarget.ParentTarget?.MatchHierarchyEditor.MatchItem.AddRectHightLevel(
                ChildsSum < (MaxOpenSingleItemSum + ExtensionOpenSingleItemSum)
                ? ChildsSum : (MaxOpenSingleItemSum + ExtensionOpenSingleItemSum));
            ListSubListView.gameObject.SetActive(true);
            SetUpSubListView();
        }

        private void CloseListView()
        {
            ClearRectHightLevel();
            ListSubListView.gameObject.SetActive(false);
            ClearSubListView();
        }

        private void SetUpSubListView()
        {
            foreach (var item in MatchEditor.MatchTarget.GetChilds())
            {
                var current = ListSubListView.GenerateItem() as HierarchyItem;
                current.MatchEditor = item.MatchHierarchyEditor;
            }
        }

        private void ClearSubListView()
        {
            ListSubListView.gameObject.SetActive(false);
            ListSubListView.Clear();
        }

    }
}
