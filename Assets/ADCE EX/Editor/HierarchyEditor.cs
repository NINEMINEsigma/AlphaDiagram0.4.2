using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Experimental.GameEditor;
using UnityEditor;
using UnityEngine;

public class TestObject : AD.Experimental.GameEditor.ICanSerializeOnCustomEditor
{
    public ISerializeHierarchyEditor MatchHierarchyEditor { get; set; }
    public List<ISerializePropertiesEditor> MatchPropertiesEditors { get; set; } = new();
    public ICanSerializeOnCustomEditor ParentTarget { get; set; }

    public int SerializeIndex { get; set; }

    public TestObject()
    {
        MatchHierarchyEditor = new TestSerializeHierarchyEditor(this);
        MatchPropertiesEditors.Add(new TestSerializePropertiesEditor1(this));
        MatchPropertiesEditors.Add(new TestSerializePropertiesEditor2(this));
    }

    public void ClickOnLeft()
    {
    }

    public void ClickOnRight()
    {
        Debug.Log(SerializeIndex);
    }

    List<ICanSerializeOnCustomEditor> Childs = new();
    public List<ICanSerializeOnCustomEditor> GetChilds() => Childs;
}

public class TestSerializeHierarchyEditor : ISerializeHierarchyEditor
{
    public HierarchyItem MatchItem { get ; set ; }

    public ICanSerializeOnCustomEditor MatchTarget { get; private set; }

    public TestSerializeHierarchyEditor(TestObject target)
    {
        MatchTarget = target;
    }

    public bool IsOpenListView { get ; set ; }

    public int SerializeIndex { get => MatchTarget.SerializeIndex; set => throw new ADException(); }

    public void OnSerialize()
    {
        MatchItem.SetTitle(MatchTarget.SerializeIndex.ToString());
        this.BaseHierarchyItemSerialize();
    }
}

public class TestSerializePropertiesEditor1 : ISerializePropertiesEditor
{
    public PropertiesItem MatchItem { get; set; }

    public ICanSerializeOnCustomEditor MatchTarget { get; private set; }

    public int SerializeIndex { get => 0; set => throw new ADException(); }

    public TestSerializePropertiesEditor1(TestObject target)
    {
        MatchTarget = target;
        _Color.Set(Random.ColorHSV());
    }

    BindProperty<Color> _Color = new();

    public void OnSerialize()
    {
        AD.Experimental.GameEditor.PropertiesLayout.SetUpPropertiesLayout(this);

        MatchItem.SetTitle("Test 1");


        AD.Experimental.GameEditor.PropertiesLayout.ApplyPropertiesLayout();
    }
}

public class TestSerializePropertiesEditor2 : ISerializePropertiesEditor
{
    public PropertiesItem MatchItem { get; set; }

    public ICanSerializeOnCustomEditor MatchTarget { get; private set; }

    public int SerializeIndex { get => 200; set => throw new ADException(); }
    BindProperty<string> Property { get; set; } = new();

    public TestSerializePropertiesEditor2(TestObject target)
    {
        MatchTarget = target;
    }

    public void OnSerialize()
    {
        AD.Experimental.GameEditor.PropertiesLayout.SetUpPropertiesLayout(this);

        MatchItem.SetTitle("Test 2");


        AD.Experimental.GameEditor.PropertiesLayout.ApplyPropertiesLayout();
    }
}

[CustomEditor(typeof(Hierarchy))]
public class HierarchyEditor : AbstractCustomADEditor
{
    Hierarchy that;

    int currentIndex = 0;

    SerializedProperty EditorAssets;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = (Hierarchy)target;

        EditorAssets = serializedObject.FindProperty("EditorAssets");
    }

    public override void OnContentGUI()
    {

    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(EditorAssets);
    }

    readonly List<TestObject> TestObjects = new();

    public override void OnSettingsGUI()
    {
        if (GUILayout.Button("Generate One"))
        {
            TestObject temp = new TestObject();
            temp.SerializeIndex = currentIndex++;
            that.TargetTopObjectEditors.Add(temp.MatchHierarchyEditor);
            TestObjects.Add(temp);
            that.ClearAndRefresh();
        }
        if (GUILayout.Button("Delete All"))
        {
            that.Init();
            TestObjects.Clear();
        }
    }
}
