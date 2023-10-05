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
        public int ExtensionOpenSingleItemSum = 0;

        public ISerializeHierarchyEditor MatchEditor;

        public override int SortIndex { get => MatchEditor.SerializeIndex; set { } }

        public override ListViewItem Init()
        {
            ListSubListView.SetPrefab(GameEditorApp.instance.GetController<Hierarchy>().EditorAssets.HierarchyItemPrefab);
            MatchEditor = null;
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

        private void Update()
        {
            if (Mouse.current.rightButton.wasPressedThisFrame) OnRightClick();
        }

        private void OnRightClick()
        {
            if (!ListToggle.Selected) return; 
            MatchEditor?.MatchTarget.ClickOnRight();
        }

        public void Refresh(bool boolen)
        {
            MatchEditor.IsOpenListView = boolen;
            if (boolen == true) OpenListView(); else CloseListView();
            MatchEditor?.MatchTarget.ClickOnLeft();
            GameEditorApp.instance.CurrentHierarchyItem = this;
            GameEditorApp.instance.SendImmediatelyCommand<CurrentItemSelectOnHierarchyPanel>();
        }

        public void AddRectHightLevel(int t)
        {
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, temp.y + DefaultHight * t);
            MatchEditor.MatchTarget.ParentTarget?.MatchHierarchyEditor.MatchItem.AddRectHightLevel(t);
        }

        public void ClearRectHightLevel()
        {
            Vector2 temp = this.transform.As<RectTransform>().sizeDelta;
            this.transform.As<RectTransform>().sizeDelta = new Vector2(temp.x, DefaultHight);
        }

        private void OpenListView()
        {
            List<ICanSerializeOnCustomEditor> ChildItems = MatchEditor.MatchTarget.GetChilds();
            int ChildsSum = MatchEditor.MatchTarget.GetChilds().Count;

            ListSubListView.gameObject.SetActive(true);
            int OpenSingleItemSum = MaxOpenSingleItemSum + ExtensionOpenSingleItemSum;
            int t = Mathf.Clamp(ChildsSum, 1, OpenSingleItemSum);
            this.AddRectHightLevel(t);
            SetUpSubListView(ChildItems);
        }

        private void CloseListView()
        {
            ClearRectHightLevel();
            ListSubListView.gameObject.SetActive(false);
            ClearSubListView();
        }

        private void SetUpSubListView(List<ICanSerializeOnCustomEditor> childs)
        {
            ListSubListView.Clear();
            foreach (var item in childs)
            {
                item.MatchHierarchyEditor.MatchItem = RegisterHierarchyItem(item.MatchHierarchyEditor);
            }
            ListSubListView.SortChilds();
        }

        private HierarchyItem RegisterHierarchyItem(ISerializeHierarchyEditor target)
        {
            HierarchyItem hierarchyItem = ListSubListView.GenerateItem() as HierarchyItem;
            target.MatchItem = hierarchyItem;
            hierarchyItem.MatchEditor = target;
            target.OnSerialize();
            hierarchyItem.name = hierarchyItem.SortIndex.ToString();
            return hierarchyItem;
        }

        private void ClearSubListView()
        {
            ListSubListView.gameObject.SetActive(false);
            ListSubListView.Clear();
        }

    }
}
