using AD.BASE;
using AD.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public abstract class IADUIEditor : Editor
{
    private AD.UI.IADUI __target = null;

    private void OnEnable()
    {
        this.__target = (AD.UI.IADUI)target;
    }

    public virtual void OnADUIInspectorGUI()
    {
        GUI.enabled = false;

        if (Application.isPlaying)
        {
            EditorGUILayout.IntSlider("SerialNumber", __target.SerialNumber, 0, AD.UI.ADUI.TotalSerialNumber - 1);
            EditorGUILayout.TextField("ElementName", __target.ElementName);
            //EditorGUILayout.TextField("ElementArea", that.ElementArea);
        }

        GUI.enabled = true;
    }
}

public abstract class ADUIEditor : IADUIEditor
{
    private AD.UI.ADUI _target = null;
    protected int currentTab;

    /// <summary>
    /// Get your custom GUI Skin
    /// </summary>
    /// <returns></returns>
    protected virtual GUISkin GetGUISkin()
    {
        return EditorGUIUtility.isProSkin == true
            ? (GUISkin)Resources.Load("Editor\\MUI Skin Dark")
            : (GUISkin)Resources.Load("Editor\\MUI Skin Light");
    }

    protected virtual string TopHeader => "CM Top Header";

    protected virtual void OnEnable()
    {
        _target = (AD.UI.ADUI)target;
    }

    /// <summary>
    /// Make ADUI's default InspectorGUI part
    /// </summary>
    public override void OnADUIInspectorGUI()
    {
        if (Application.isPlaying)
        {
            GUI.enabled = false;

            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.IntSlider("SerialNumber", _target.SerialNumber, 0, AD.UI.ADUI.TotalSerialNumber - 1);
            EditorGUILayout.TextField("ElementName", _target.ElementName);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.TextField("ElementArea", _target.ElementArea);

            GUI.enabled = true;

            EditorGUILayout.Toggle("IsSelect", _target.Selected, customSkin.GetStyle("Toggle"));
            GUILayout.EndHorizontal();
        }
        else HelpBox("ADUI Element Detail Will SerializeField When Playing Mode", MessageType.Info);
    }

    public void OnNotChangeGUI(UnityAction action)
    {
        GUI.enabled = false;
        action();
        GUI.enabled = true;
    }

    public void HelpBox(string message,MessageType messageType)
    {
        EditorGUILayout.HelpBox(message, messageType);
    }

    public void HorizontalBlock(UnityAction action)
    {
        GUILayout.BeginHorizontal();
        action();
        GUILayout.EndHorizontal();
    }

    public void HorizontalBlockWithBox(UnityAction action)
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);
        action();
        GUILayout.EndHorizontal();
    }

    public abstract void OnContentGUI();
    public abstract void OnResourcesGUI();
    public abstract void OnSettingsGUI();

    protected GUISkin customSkin;
    protected Color defaultColor;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        defaultColor = GUI.color;

        if (EditorGUIUtility.isProSkin == true)
            customSkin = (GUISkin)Resources.Load("Editor\\MUI Skin Dark");
        else
            customSkin = (GUISkin)Resources.Load("Editor\\MUI Skin Light");

        GUILayout.BeginHorizontal();
        GUI.backgroundColor = defaultColor;

        GUILayout.Box(new GUIContent(""), customSkin.FindStyle(TopHeader));

        GUILayout.EndHorizontal();
        GUILayout.Space(-42);

        GUIContent[] toolbarTabs = new GUIContent[3];
        toolbarTabs[0] = new GUIContent("Content");
        toolbarTabs[1] = new GUIContent("Resources");
        toolbarTabs[2] = new GUIContent("Settings");

        GUILayout.BeginHorizontal();
        GUILayout.Space(17);

        currentTab = GUILayout.Toolbar(currentTab, toolbarTabs, customSkin.FindStyle("Tab Indicator"));

        GUILayout.EndHorizontal();
        GUILayout.Space(-40);
        GUILayout.BeginHorizontal();
        GUILayout.Space(17);

        if (GUILayout.Button(new GUIContent("Content", "Content"), customSkin.FindStyle("Tab Content")))
            currentTab = 0;
        if (GUILayout.Button(new GUIContent("Resources", "Resources"), customSkin.FindStyle("Tab Resources")))
            currentTab = 1;
        if (GUILayout.Button(new GUIContent("Settings", "Settings"), customSkin.FindStyle("Tab Settings")))
            currentTab = 2;

        GUILayout.EndHorizontal();

        switch (currentTab)
        {
            case 0:
                {
                    OnADUIInspectorGUI();
                    OnContentGUI();
                }
                break;

            case 1:
                {
                    OnResourcesGUI();
                }
                break;

            case 2:
                {
                    OnSettingsGUI();
                }
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(AD.UI.Text)), CanEditMultipleObjects]
public class TextEdior : ADUIEditor
{
    private AD.UI.Text that = null;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.Text;
    }

    public override void OnContentGUI()
    {
        EditorGUI.BeginChangeCheck();
        string str = EditorGUILayout.TextField(new GUIContent("Text"), that.text);
        if (EditorGUI.EndChangeCheck()) that.text = str;
    }

    public override void OnResourcesGUI()
    {
    }

    public override void OnSettingsGUI()
    {

    }
}

[CustomEditor(typeof(AD.UI.Slider)), CanEditMultipleObjects]
public class SliderEdior : ADUIEditor
{
    private AD.UI.Slider that = null;

    private SerializedProperty background = null;
     private SerializedProperty handle = null;
    private SerializedProperty fill = null;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.Slider;

        background = serializedObject.FindProperty("background");
        handle = serializedObject.FindProperty("handle");
        fill = serializedObject.FindProperty("fill");
    }

    public override void OnContentGUI()
    {
        UnityEngine.Object @object = null;
        Sprite sprite = null;

        EditorGUI.BeginChangeCheck();
        GUIContent gUIContent = new GUIContent("Background");
        sprite = EditorGUILayout.ObjectField(gUIContent, that.backgroundView as UnityEngine.Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.backgroundView = sprite;

        EditorGUI.BeginChangeCheck();
        GUIContent fUIContent = new GUIContent("Fill");
        sprite = EditorGUILayout.ObjectField(fUIContent, that.fillView as UnityEngine.Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.fillView = sprite;

        EditorGUI.BeginChangeCheck();
        GUIContent hUIContent = new GUIContent("Handle");
        sprite = EditorGUILayout.ObjectField(hUIContent, that.handleView as UnityEngine.Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.handleView = sprite;

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(background);
        EditorGUILayout.PropertyField(handle);
        EditorGUILayout.PropertyField(fill);
    }

    public override void OnSettingsGUI()
    {

    }
}

[CustomEditor(typeof(AD.UI.Toggle)), CanEditMultipleObjects]
public class ToggleEdior : ADUIEditor
{
    private AD.UI.Toggle that = null;

    private SerializedProperty background = null;
    private SerializedProperty tab = null;
    private SerializedProperty mark = null;
    private SerializedProperty title = null; 
    private SerializedProperty actions = null;

    private SerializedProperty _IsCheck = null; 

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.Toggle;

        background = serializedObject.FindProperty("background");
        tab = serializedObject.FindProperty("tab");
        mark = serializedObject.FindProperty("mark");
        title = serializedObject.FindProperty("title");
        _IsCheck = serializedObject.FindProperty("_IsCheck");
        actions = serializedObject.FindProperty("actions");
    }

    public override void OnContentGUI()
    {
        OnNotChangeGUI(() => HorizontalBlockWithBox(() => EditorGUILayout.Toggle("IsCheck", that.IsCheck, customSkin.toggle)));

        EditorGUILayout.PropertyField(actions);

        Sprite sprite = null;
        UnityEngine.Object @object = null;
        string str = "";

        EditorGUI.BeginChangeCheck();
        GUIContent gUIContent = new GUIContent("Background");
        sprite = EditorGUILayout.ObjectField(gUIContent, that.background.sprite as Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.background.sprite = sprite;

        EditorGUI.BeginChangeCheck();
        GUIContent tUIContent = new GUIContent("Tab");
        sprite = EditorGUILayout.ObjectField(tUIContent, that.tab.sprite as Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.tab.sprite = sprite;

        EditorGUI.BeginChangeCheck();
        GUIContent mUIContent = new GUIContent("Mark");
        sprite = EditorGUILayout.ObjectField(mUIContent, that.mark.sprite as Object, typeof(Sprite), @object) as Sprite;
        if (EditorGUI.EndChangeCheck()) that.mark.sprite = sprite;

        EditorGUI.BeginChangeCheck();
        GUIContent tiUIContent = new GUIContent("Title");
        str = EditorGUILayout.TextField(tiUIContent, that.title.text);
        if (EditorGUI.EndChangeCheck()) that.title.text = str;

        serializedObject.ApplyModifiedProperties();
    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(background);
        EditorGUILayout.PropertyField(tab);
        EditorGUILayout.PropertyField(mark);
        EditorGUILayout.PropertyField(title);
    }

    public override void OnSettingsGUI()
    {
        if (GUILayout.Button("Init")) that.Init();
    }
}

[CustomEditor(typeof(AD.UI.Button)), CanEditMultipleObjects]
public class ButtonEdior : ADUIEditor
{
    private AD.UI.Button that = null;

    SerializedProperty animator;
    SerializedProperty ChooseMode;
    SerializedProperty AnimatorBoolString;
    SerializedProperty AnimatorONString, AnimatorOFFString;
    SerializedProperty OnClick, OnRelease;
    SerializedProperty _IsClick;
    SerializedProperty IsKeepState;
    SerializedProperty title;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.Button;

        animator = serializedObject.FindProperty(nameof(animator));
        ChooseMode = serializedObject.FindProperty(nameof(ChooseMode));
        AnimatorBoolString = serializedObject.FindProperty(nameof(AnimatorBoolString));
        AnimatorONString = serializedObject.FindProperty(nameof(AnimatorONString));
        AnimatorOFFString = serializedObject.FindProperty(nameof(AnimatorOFFString));
        OnClick = serializedObject.FindProperty(nameof(OnClick));
        OnRelease = serializedObject.FindProperty(nameof(OnRelease));
        _IsClick = serializedObject.FindProperty(nameof(_IsClick));
        IsKeepState = serializedObject.FindProperty(nameof(IsKeepState));
        title = serializedObject.FindProperty(nameof(title));
    }

    public override void OnContentGUI()
    {
        EditorGUILayout.PropertyField(OnClick);
        EditorGUILayout.PropertyField(OnRelease);
        HorizontalBlockWithBox(() => OnNotChangeGUI(() => EditorGUILayout.PropertyField(_IsClick)));
    }

    public override void OnResourcesGUI()
    {
        HorizontalBlockWithBox(() => {
            HelpBox("You Can Set Animator Null To Close Animation",MessageType.Info);
            EditorGUILayout.PropertyField(animator);
            });
        EditorGUILayout.PropertyField(title);
    }

    public override void OnSettingsGUI()
    {
        if(that.animator != null)
        {
            HorizontalBlockWithBox(() => OnNotChangeGUI(() => EditorGUILayout.TextArea("Animatior")));
            EditorGUILayout.PropertyField(ChooseMode);
            EditorGUILayout.PropertyField(AnimatorBoolString);
            EditorGUILayout.PropertyField(AnimatorONString);
            EditorGUILayout.PropertyField(AnimatorOFFString);
            EditorGUILayout.Space(20);
        }
        else HorizontalBlockWithBox(() => OnNotChangeGUI(() => EditorGUILayout.TextArea("No Animatior")));
        HorizontalBlockWithBox(() => {
            HelpBox("Do You Need Animation And Keep Button State", MessageType.Info);
            EditorGUILayout.PropertyField(IsKeepState);
        });
    }
} 

[CustomEditor(typeof(AD.UI.RawImage)), CanEditMultipleObjects]
public class RawImageEdior : ADUIEditor
{
    private AD.UI.RawImage that = null;

    public override void OnContentGUI()
    {

    }

    public override void OnResourcesGUI()
    {

    }

    public override void OnSettingsGUI()
    {

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.RawImage;
    }
}
 
[CustomEditor(typeof(AD.UI.InputField)), CanEditMultipleObjects]
public class InputFieldEdior : ADUIEditor
{
    private AD.UI.InputField that = null;

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.InputField;
    }

    public override void OnContentGUI()
    {
        EditorGUI.BeginChangeCheck();
        GUIContent gUIContent = new GUIContent("InputField");
        string str = EditorGUILayout.TextField(gUIContent, that.text);
        if (EditorGUI.EndChangeCheck()) that.text = str;
    }

    public override void OnResourcesGUI()
    {

    }

    public override void OnSettingsGUI()
    {

    }
}

[CustomEditor(typeof(AD.UI.ModernUIButton)), CanEditMultipleObjects]
public class ModernUIButtonEditor : ADUIEditor
{
    private AD.UI.ModernUIButton that;

    SerializedProperty buttonText;
    SerializedProperty hoverSound;
    SerializedProperty clickSound;
    SerializedProperty clickEvent;
    SerializedProperty hoverEvent;
    SerializedProperty normalText;
    SerializedProperty highlightedText;
    SerializedProperty soundSource;
    SerializedProperty useCustomContent;
    SerializedProperty enableButtonSounds;
    SerializedProperty useHoverSound;
    SerializedProperty useClickSound;
    SerializedProperty rippleParent;
    SerializedProperty useRipple;
    SerializedProperty renderOnTop;
    SerializedProperty centered;
    SerializedProperty rippleShape;
    SerializedProperty speed;
    SerializedProperty maxSize;
    SerializedProperty startColor;
    SerializedProperty transitionColor;
    SerializedProperty animationSolution;
    SerializedProperty fadingMultiplier;
    SerializedProperty rippleUpdateMode;

    protected override string TopHeader => "Button Top Header";

    protected override void OnEnable()
    {
        base.OnEnable();
        that = target as AD.UI.ModernUIButton;

        buttonText = serializedObject.FindProperty("buttonText");
        hoverSound = serializedObject.FindProperty("hoverSound");
        clickSound = serializedObject.FindProperty("clickSound");
        clickEvent = serializedObject.FindProperty("clickEvent");
        hoverEvent = serializedObject.FindProperty("hoverEvent");
        normalText = serializedObject.FindProperty("normalText");
        highlightedText = serializedObject.FindProperty("highlightedText");
        soundSource = serializedObject.FindProperty("soundSource");
        useCustomContent = serializedObject.FindProperty("useCustomContent");
        enableButtonSounds = serializedObject.FindProperty("enableButtonSounds");
        useHoverSound = serializedObject.FindProperty("useHoverSound");
        useClickSound = serializedObject.FindProperty("useClickSound");
        rippleParent = serializedObject.FindProperty("rippleParent");
        useRipple = serializedObject.FindProperty("useRipple");
        renderOnTop = serializedObject.FindProperty("renderOnTop");
        centered = serializedObject.FindProperty("centered");
        rippleShape = serializedObject.FindProperty("rippleShape");
        speed = serializedObject.FindProperty("speed");
        maxSize = serializedObject.FindProperty("maxSize");
        startColor = serializedObject.FindProperty("startColor");
        transitionColor = serializedObject.FindProperty("transitionColor");
        animationSolution = serializedObject.FindProperty("animationSolution");
        fadingMultiplier = serializedObject.FindProperty("fadingMultiplier");
        rippleUpdateMode = serializedObject.FindProperty("rippleUpdateMode");
    }

    public override void OnContentGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Button Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(buttonText, new GUIContent(""));

        GUILayout.EndHorizontal();

        if (useCustomContent.boolValue == false && that.normalText != null)
        {
            that.normalText.text = buttonText.stringValue;
            that.highlightedText.text = buttonText.stringValue;
        }

        else if (useCustomContent.boolValue == false && that.normalText == null)
        {
            GUILayout.Space(2);
            EditorGUILayout.HelpBox("'Text Object' is not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
        }

        if (enableButtonSounds.boolValue == true && useHoverSound.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Hover Sound"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(hoverSound, new GUIContent(""));

            GUILayout.EndHorizontal();
        }

        if (enableButtonSounds.boolValue == true && useClickSound.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Click Sound"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(clickSound, new GUIContent(""));

            GUILayout.EndHorizontal();
        }

        GUILayout.Space(4);
        EditorGUILayout.PropertyField(clickEvent, new GUIContent("On Click Event"), true);
        EditorGUILayout.PropertyField(hoverEvent, new GUIContent("On Hover Event"), true);
    }

    public override void OnResourcesGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Normal Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(normalText, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Highlighted Text"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(highlightedText, new GUIContent(""));

        GUILayout.EndHorizontal();

        if (enableButtonSounds.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Sound Source"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(soundSource, new GUIContent(""));

            if (that.soundSource == null)
            {
                EditorGUILayout.HelpBox("'Sound Source' is not assigned. Go to Resources tab or click the button to create a new audio source.", MessageType.Warning);

                if (GUILayout.Button("Create a new one", customSkin.button))
                {
                    that.soundSource = that.gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
                    currentTab = 1;
                }
            }

            GUILayout.EndHorizontal();
        }

        if (useRipple.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Ripple Parent"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(rippleParent, new GUIContent(""));

            GUILayout.EndHorizontal();
        }
    }

    public override void OnSettingsGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Animation Solution"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(animationSolution, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Fading Multiplier"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(fadingMultiplier, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        useCustomContent.boolValue = GUILayout.Toggle(useCustomContent.boolValue, new GUIContent("Use Custom Content"), customSkin.FindStyle("Toggle"));
        useCustomContent.boolValue = GUILayout.Toggle(useCustomContent.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        enableButtonSounds.boolValue = GUILayout.Toggle(enableButtonSounds.boolValue, new GUIContent("Enable Button Sounds"), customSkin.FindStyle("Toggle"));
        enableButtonSounds.boolValue = GUILayout.Toggle(enableButtonSounds.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();

        if (enableButtonSounds.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Sound Source"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(soundSource, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            useHoverSound.boolValue = GUILayout.Toggle(useHoverSound.boolValue, new GUIContent("Enable Hover Sound"), customSkin.FindStyle("Toggle"));
            useHoverSound.boolValue = GUILayout.Toggle(useHoverSound.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            useClickSound.boolValue = GUILayout.Toggle(useClickSound.boolValue, new GUIContent("Enable Click Sound"), customSkin.FindStyle("Toggle"));
            useClickSound.boolValue = GUILayout.Toggle(useClickSound.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            GUILayout.EndHorizontal();

            if (that.soundSource == null)
            {
                EditorGUILayout.HelpBox("'Sound Source' is not assigned. Go to Resources tab or click the button to create a new audio source.", MessageType.Warning);

                if (GUILayout.Button("Create a new one", customSkin.button))
                {
                    that.soundSource = that.gameObject.AddComponent(typeof(AudioSource)) as AudioSource;
                    currentTab = 2;
                }
            }
        }

        GUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Space(-2);
        GUILayout.BeginHorizontal();

        useRipple.boolValue = GUILayout.Toggle(useRipple.boolValue, new GUIContent("Use Ripple"), customSkin.FindStyle("Toggle"));
        useRipple.boolValue = GUILayout.Toggle(useRipple.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
        GUILayout.Space(4);

        if (useRipple.boolValue == true)
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            renderOnTop.boolValue = GUILayout.Toggle(renderOnTop.boolValue, new GUIContent("Render On Top"), customSkin.FindStyle("Toggle"));
            renderOnTop.boolValue = GUILayout.Toggle(renderOnTop.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            centered.boolValue = GUILayout.Toggle(centered.boolValue, new GUIContent("Centered"), customSkin.FindStyle("Toggle"));
            centered.boolValue = GUILayout.Toggle(centered.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Update Mode"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(rippleUpdateMode, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Shape"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(rippleShape, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Speed"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(speed, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Max Size"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(maxSize, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Start Color"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(startColor, new GUIContent(""));

            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField(new GUIContent("Transition Color"), customSkin.FindStyle("Text"), GUILayout.Width(120));
            EditorGUILayout.PropertyField(transitionColor, new GUIContent(""));

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
    }
}

[CustomEditor(typeof(ModernUIFillBar))]
public class ProgressBarEditor : ADUIEditor
{
    private ModernUIFillBar pbTarget;

    SerializedProperty minValue;
    SerializedProperty maxValue;
    SerializedProperty loadingBar;
    SerializedProperty textPercent;
    SerializedProperty textValue;
    SerializedProperty IsPercent;
    SerializedProperty IsInt;

    protected override void OnEnable()
    {
        base.OnEnable();
        pbTarget = (ModernUIFillBar)target;

        minValue = serializedObject.FindProperty("minValue");
        maxValue = serializedObject.FindProperty("maxValue");
        loadingBar = serializedObject.FindProperty("loadingBar");
        textPercent = serializedObject.FindProperty("textPercent");
        textValue = serializedObject.FindProperty("textValue");
        IsPercent = serializedObject.FindProperty("IsPercent");
        IsInt = serializedObject.FindProperty("IsInt");
    }

    public override void OnContentGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Current Percent"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        pbTarget.currentPercent = EditorGUILayout.Slider(pbTarget.currentPercent, 0, 1);

        GUILayout.EndHorizontal();

        if (pbTarget.loadingBar != null && pbTarget.textPercent != null && pbTarget.textValue != null)
        {
            pbTarget.Update();
        }
        else
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox("Some resources are not assigned. Go to Resources tab and assign the correct variable.", MessageType.Error);
            GUILayout.EndHorizontal();
        }
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Min Value"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(minValue, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Max Value"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(maxValue, new GUIContent(""));

        GUILayout.EndHorizontal();
    }

    public override void OnResourcesGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Loading Bar"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(loadingBar, new GUIContent(""));

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Text Indicator"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(textPercent, new GUIContent(""));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        EditorGUILayout.LabelField(new GUIContent("Text Value"), customSkin.FindStyle("Text"), GUILayout.Width(120));
        EditorGUILayout.PropertyField(textValue, new GUIContent(""));

        GUILayout.EndHorizontal();
    }

    public override void OnSettingsGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        IsPercent.boolValue = GUILayout.Toggle(IsPercent.boolValue, new GUIContent("Is Percent"), customSkin.FindStyle("Toggle"));
        IsPercent.boolValue = GUILayout.Toggle(IsPercent.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(EditorStyles.helpBox);

        IsInt.boolValue = GUILayout.Toggle(IsInt.boolValue, new GUIContent("Is Int"), customSkin.FindStyle("Toggle"));
        IsInt.boolValue = GUILayout.Toggle(IsInt.boolValue, new GUIContent(""), customSkin.FindStyle("Toggle Helper"));

        GUILayout.EndHorizontal();
    }
}

[CustomEditor(typeof(AD.UI.ListView))]
public class ListViewEditor : ADUIEditor
{
    private ListView that;

    SerializedProperty _Scroll;
    SerializedProperty _List;
    SerializedProperty _Title;
    SerializedProperty Prefab;
    SerializedProperty index;

    public override void OnContentGUI()
    {

    }

    public override void OnResourcesGUI()
    {
        EditorGUILayout.PropertyField(_Scroll);
        EditorGUILayout.PropertyField(_List);
        EditorGUILayout.PropertyField(_Title);
        EditorGUILayout.PropertyField(Prefab);
        HorizontalBlockWithBox(() => OnNotChangeGUI(() => EditorGUILayout.PropertyField(index)));
    }

    public override void OnSettingsGUI()
    {
        if (GUILayout.Button("Init ListView")) that.Init();
        if (GUILayout.Button("Generate New Item")) that.GenerateItem();
        if (GUILayout.Button("Clear All Childs")) that.Clear();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        that = (ListView)target;

        _Scroll = serializedObject.FindProperty("_Scroll");
        _List = serializedObject.FindProperty("_List");
        _Title = serializedObject.FindProperty("_Title");
        Prefab = serializedObject.FindProperty("Prefab");
        index = serializedObject.FindProperty("index");
    }

}