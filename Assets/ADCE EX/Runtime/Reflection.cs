using System;
using System.Collections.Generic;
using UnityEngine;
using AD.Utility;
using System.Reflection;
using System.Linq;
using static AD.Utility.ReflectionExtension;
using Unity.VisualScripting;

namespace AD.Experimental.GameEditor
{
    public class HiearchyBlock<T> : ISerializeHierarchyEditor where T : ICanSerializeOnCustomEditor
    {
        public HiearchyBlock(T that, string name)
        {
            this.that = that;
            this.Title = name;
        }

        protected readonly T that;

        public HierarchyItem MatchItem { get; set; }

        public ICanSerializeOnCustomEditor MatchTarget { get => that; }

        public bool IsOpenListView { get; set; }
        public int SerializeIndex { get => MatchTarget.SerializeIndex; set => throw new NotImplementedException(); }

        public void OnSerialize()
        {
            MatchItem.SetTitle(Title);
        }

        private string _Title;
        public virtual string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
                MatchItem.SetTitle(_Title);
            }
        }
    }

    public class PropertiesBlock<T> : ISerializePropertiesEditor where T : ICanSerializeOnCustomEditor
    {
        public static readonly Type CanSerialize = typeof(ADSerializeAttribute);
        public static readonly Type CanSetupAction = typeof(ADActionButtonAttribute);
        public static readonly BindingFlags bindings = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public PropertiesBlock(T that, string layer, int index = 0)
        {
            this.that = that;
            this.SerializeIndex = index;
            this.LayerTitle = layer;

            GetSerializableFields(that.GetType()).GetSubList(T =>
            {
                foreach (CustomAttributeData cad in T.GetCustomAttributesData())
                {
                    if (cad.AttributeType == CanSerialize)
                    {
                        var cat = new ADSerializeEntry();
                        if (SetupEntry(cat, cad.NamedArguments))
                        {
                            fields[cat] = T;
                            maxLine.Add(cat);
                        }
                        break;
                    }
                }
                return true;
            });
            GetSerializableProperties(that.GetType()).GetSubList(T =>
            {
                foreach (CustomAttributeData cad in T.GetCustomAttributesData())
                {
                    if (cad.AttributeType == CanSerialize)
                    {
                        var cat = new ADSerializeEntry();
                        if (SetupEntry(cat, cad.NamedArguments))
                        {
                            propertys[cat] = T;
                            maxLine.Add(cat);
                        }
                        break;
                    }
                }
                return true;
            });
            GetSerializableMethods(that.GetType()).GetSubList(T =>
            {
                foreach (CustomAttributeData cad in T.GetCustomAttributesData())
                {
                    if (cad.AttributeType == CanSetupAction)
                    {
                        var cat = new ADSerializeEntry();
                        if (SetupEntry(cat, cad.NamedArguments))
                        {
                            methods[cat] = T;
                            maxLine.Add(cat);
                        }
                        break;
                    }
                }
                return true;
            });

            maxLine.Sort((T, P) => T.index.CompareTo(P.index));

            bool SetupEntry(ADSerializeEntry cat, IList<CustomAttributeNamedArgument> temp_catas)
            {
                bool isTure = false;
                foreach (var cata in temp_catas)
                {
                    switch (cata.MemberName)
                    {
                        case "layer":
                            {
                                cat.layer = (string)cata.TypedValue.Value;
                                if (cat.layer != layer) return false;
                                else isTure = true;
                            }
                            break;
                        case "index":
                            {
                                cat.index = (int)cata.TypedValue.Value;
                            }
                            break;
                        case "message":
                            {
                                cat.message = (string)cata.TypedValue.Value;
                            }
                            break;
                        case "methodName":
                            {
                                cat.methodName = (string)cata.TypedValue.Value;
                            }
                            break;
                    }
                }
                return isTure;
            }
        }

        public class ADSerializeEntry
        {
            public string layer;
            public int index;
            public string message;
            public string methodName;
        }

        private readonly Dictionary<ADSerializeEntry, FieldInfo> fields = new();
        private readonly Dictionary<ADSerializeEntry, PropertyInfo> propertys = new();
        private readonly Dictionary<ADSerializeEntry, MethodInfo> methods = new();
        private readonly List<ADSerializeEntry> maxLine = new();

        protected readonly T that;

        public string LayerTitle { get; private set; }

        public PropertiesItem MatchItem { get; set; }

        public ICanSerializeOnCustomEditor MatchTarget { get => that; }

        public int SerializeIndex { get; set; }
        public bool IsDirty { get; set; }

        public void OnSerialize()
        {
            PropertiesLayout.SetUpPropertiesLayout(this);

            MatchItem.SetTitle(LayerTitle);
            HowSerialize();

            PropertiesLayout.ApplyPropertiesLayout();
        }

        protected virtual void HowSerialize()
        {
            foreach (var item in maxLine)
            {
                if (fields.TryGetValue(item, out var field)) DoGUI_Field(item, field);
                else if (propertys.TryGetValue(item, out var _proerty)) DoGUI_Property(item, _proerty);
                else if (methods.TryGetValue(item, out var method)) PropertiesLayout.ModernUIButton(item.methodName, item.message, () => method.Invoke(that, null));
            }
        }

        public static List<FieldInfo> GetSerializableFields(Type type,
                                                            List<FieldInfo> serializableFields = null,
                                                            string[] memberNames = null)
        {
            if (type == null)
                return new List<FieldInfo>();

            var fields = type.GetFields(bindings);

            serializableFields ??= new List<FieldInfo>();

            foreach (var field in fields)
            {
                var fieldName = field.Name;

                // If a members array was provided as a parameter, only include the field if it's in the array.
                if (memberNames != null)
                    if (!memberNames.Contains(fieldName))
                        continue;

                var fieldType = field.FieldType;

                // If the field is private, only serialize it if it's explicitly marked as ADSerialize.
                if (!AttributeIsDefined(field, CanSerialize))
                    continue;

                // Exclude const or readonly fields.
                if (field.IsLiteral || field.IsInitOnly)
                    continue;

                // Don't store fields whose type is the same as the class the field is housed in unless it's stored by reference (to prevent cyclic references)
                if (fieldType == type && !IsAssignableFrom(typeof(UnityEngine.Object), fieldType))
                    continue;

                // If property is marked as obsolete or non-serialized, don't serialize it.
                if (AttributeIsDefined(field, nonSerializedAttributeType) || AttributeIsDefined(field, obsoleteAttributeType))
                    continue;

                if (!TypeIsSerializable(field.FieldType))
                    continue;

                // Don't serialize member fields.
                if (fieldName.StartsWith(memberFieldPrefix) && field.DeclaringType.Namespace != null && field.DeclaringType.Namespace.Contains("UnityEngine"))
                    continue;

                serializableFields.Add(field);
            }

            var baseType = BaseType(type);
            if (baseType != null && baseType != typeof(System.Object) && baseType != typeof(UnityEngine.Object))
                GetSerializableFields(BaseType(type), serializableFields, memberNames);

            return serializableFields;
        }

        public static List<PropertyInfo> GetSerializableProperties(Type type,
                                                                   List<PropertyInfo> serializableProperties = null,
                                                                   string[] memberNames = null)
        {
            bool isComponent = IsAssignableFrom(typeof(UnityEngine.Component), type);

            var properties = type.GetProperties(bindings);

            serializableProperties ??= new List<PropertyInfo>();

            foreach (var p in properties)
            {
                var propertyName = p.Name;

                if (excludedPropertyNames.Contains(propertyName))
                    continue;

                // If a members array was provided as a parameter, only include the property if it's in the array.
                if (memberNames != null)
                    if (!memberNames.Contains(propertyName))
                        continue;

                // If safe serialization is enabled, only get methods which are explicitly marked as CanSerialize.
                if (!AttributeIsDefined(p, CanSerialize))
                    continue;

                var propertyType = p.PropertyType;

                // Don't store methods whose type is the same as the class the property is housed in unless it's stored by reference (to prevent cyclic references)
                if (propertyType == type && !IsAssignableFrom(typeof(UnityEngine.Object), propertyType))
                    continue;

                if (!p.CanRead || !p.CanWrite)
                    continue;

                // Only support methods with indexing if they're an array.
                if (p.GetIndexParameters().Length != 0 && !propertyType.IsArray)
                    continue;

                // Check that the type of the property is one which we can serialize.
                if (!TypeIsSerializable(propertyType))
                    continue;

                // Ignore certain methods on components.
                if (isComponent)
                {
                    // Ignore methods which are accessors for GameObject fields.
                    if (propertyName == componentTagFieldName || propertyName == componentNameFieldName)
                        continue;
                }

                // If property is marked as obsolete or non-serialized, don't serialize it.
                if (AttributeIsDefined(p, obsoleteAttributeType) || AttributeIsDefined(p, nonSerializedAttributeType))
                    continue;

                serializableProperties.Add(p);
            }

            var baseType = BaseType(type);
            if (baseType != null && baseType != typeof(System.Object))
                GetSerializableProperties(baseType, serializableProperties, memberNames);

            return serializableProperties;
        }

        public static List<MethodInfo> GetSerializableMethods(Type type,
                                                                   List<MethodInfo> serializableMethods = null,
                                                                   string[] memberNames = null)
        {
            var methods = type.GetMethods(bindings);

            serializableMethods ??= new List<MethodInfo>();

            foreach (var m in methods)
            {
                var methodName = m.Name;

                // If a members array was provided as a parameter, only include the method if it's in the array.
                if (memberNames != null)
                    if (!memberNames.Contains(methodName))
                        continue;

                // If safe serialization is enabled, only get methods which are explicitly marked as CanSetupAction.
                if (!AttributeIsDefined(m, CanSetupAction))
                    continue;

                var argTypes = m.GetGenericArguments();

                if (argTypes.Length > 0)
                    continue;

                serializableMethods.Add(m);
            }

            var baseType = BaseType(type);
            if (baseType != null && baseType != typeof(System.Object))
                GetSerializableMethods(baseType, serializableMethods, memberNames);

            return serializableMethods;
        }

        private void DoGUI_Field(ADSerializeEntry entry, FieldInfo field)
        {
            Type type = field.FieldType;
            if (type == typeof(bool))
            {
                PropertiesLayout.ModernUISwitch(field.Name, (bool)field.GetValue(that), entry.message, T => field.SetValue(that, T));
            }
            else if (type == typeof(char))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name, entry.message);
                var cat = PropertiesLayout.InputField(((char)field.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    field.SetValue(that, T[0]);
                    cat.SetTextWithoutNotify(T[..1]);
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(double))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name, entry.message);
                var cat = PropertiesLayout.InputField(((double)field.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (double.TryParse(T, out double value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((double)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(float))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name, entry.message);
                var cat = PropertiesLayout.InputField(((float)field.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (float.TryParse(T, out float value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((float)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(int))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(integer)", entry.message);
                var cat = PropertiesLayout.InputField(((int)field.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (int.TryParse(T, out int value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((int)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(uint))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(unsigned integer)", entry.message);
                var cat = PropertiesLayout.InputField(((uint)field.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (uint.TryParse(T, out uint value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((uint)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(long))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(integer)", entry.message);
                var cat = PropertiesLayout.InputField(((long)field.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (long.TryParse(T, out long value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((long)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(ulong))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(unsigned integer)", entry.message);
                var cat = PropertiesLayout.InputField(((ulong)field.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (ulong.TryParse(T, out ulong value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((ulong)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(short))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(integer)", entry.message);
                var cat = PropertiesLayout.InputField(((short)field.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (short.TryParse(T, out short value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((short)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(ushort))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name + "(unsigh integer)", entry.message);
                var cat = PropertiesLayout.InputField(((ushort)field.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (ushort.TryParse(T, out ushort value))
                    {
                        field.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((ushort)field.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(string))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name, entry.message);
                var cat = PropertiesLayout.InputField((string)field.GetValue(that), entry.message);
                cat.AddListener(T =>
                {
                    field.SetValue(that, T);
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(Vector2))
            {
                PropertiesLayout.Label(field.Name, entry.message);
                var value = (Vector2)field.GetValue(that);

                PropertiesLayout.BeginHorizontal();
                var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                PropertiesLayout.EndHorizontal();

                catX.AddListener(T =>
                {
                    var value = (Vector2)field.GetValue(that);
                    if (float.TryParse(T, out float nx))
                    {
                        value.x = nx;
                        field.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.x.ToString());
                });
                catY.AddListener(T =>
                {
                    var value = (Vector2)field.GetValue(that);
                    if (float.TryParse(T, out float ny))
                    {
                        value.y = ny;
                        field.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.y.ToString());
                });

            }
            else if (type == typeof(Vector3))
            {
                PropertiesLayout.Label(field.Name, entry.message);
                var value = (Vector3)field.GetValue(that);

                PropertiesLayout.BeginHorizontal();
                var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                var catZ = PropertiesLayout.InputField(value.z.ToString(), entry.message);
                PropertiesLayout.EndHorizontal();

                catX.AddListener(T =>
                {
                    var value = (Vector3)field.GetValue(that);
                    if (float.TryParse(T, out float nx))
                    {
                        value.x = nx;
                        field.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.x.ToString());
                });
                catY.AddListener(T =>
                {
                    var value = (Vector3)field.GetValue(that);
                    if (float.TryParse(T, out float ny))
                    {
                        value.y = ny;
                        field.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.y.ToString());
                });
                catZ.AddListener(T =>
                {
                    var value = (Vector3)field.GetValue(that);
                    if (float.TryParse(T, out float nz))
                    {
                        value.z = nz;
                        field.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.z.ToString());
                });
            }
            else if (type == typeof(Vector4))
            {
                PropertiesLayout.Label(field.Name, entry.message);
                var value = (Vector4)field.GetValue(that);

                PropertiesLayout.BeginHorizontal();
                var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                PropertiesLayout.EndHorizontal();
                PropertiesLayout.BeginHorizontal();
                var catZ = PropertiesLayout.InputField(value.z.ToString(), entry.message);
                var catW = PropertiesLayout.InputField(value.w.ToString(), entry.message);
                PropertiesLayout.EndHorizontal();

                catX.AddListener(T =>
                {
                    var value = (Vector4)field.GetValue(that);
                    if (float.TryParse(T, out float nx))
                    {
                        value.x = nx;
                        field.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.x.ToString());
                });
                catY.AddListener(T =>
                {
                    var value = (Vector4)field.GetValue(that);
                    if (float.TryParse(T, out float ny))
                    {
                        value.y = ny;
                        field.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.y.ToString());
                });
                catZ.AddListener(T =>
                {
                    var value = (Vector4)field.GetValue(that);
                    if (float.TryParse(T, out float nz))
                    {
                        value.z = nz;
                        field.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.z.ToString());
                });
                catW.AddListener(T =>
                {
                    var value = (Vector4)field.GetValue(that);
                    if (float.TryParse(T, out float nw))
                    {
                        value.w = nw;
                        field.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.w.ToString());
                });
            }
            else if (type == typeof(Color))
            {
                PropertiesLayout.ColorPanel(field.Name, (Color)field.GetValue(that), entry.message, T =>
                {
                    field.SetValue(that, T);
                });
            }
            else if (type == typeof(Texture2D))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(field.Name, entry.message);
                PropertiesLayout.RawImage((Texture2D)field.GetValue(that), entry.message);
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(Sprite))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Image(field.Name, entry.message).CurrentImagePair = new() { SpriteSource = (Sprite)field.GetValue(that) };
                PropertiesLayout.EndHorizontal();
            }
            else DoGUI_Field_Extension(entry, field);
        }

        private void DoGUI_Property(ADSerializeEntry entry, PropertyInfo property)
        {
            Type type = property.PropertyType;
            if (type == typeof(bool))
            {
                PropertiesLayout.ModernUISwitch(property.Name, (bool)property.GetValue(that), entry.message, T => property.SetValue(that, T));
            }
            else if (type == typeof(char))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name, entry.message);
                var cat = PropertiesLayout.InputField(((char)property.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    property.SetValue(that, T[0]);
                    cat.SetTextWithoutNotify(T[..1]);
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(double))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name, entry.message);
                var cat = PropertiesLayout.InputField(((double)property.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (double.TryParse(T, out double value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((double)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(float))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name, entry.message);
                var cat = PropertiesLayout.InputField(((float)property.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (float.TryParse(T, out float value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((float)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(int))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(integer)", entry.message);
                var cat = PropertiesLayout.InputField(((int)property.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (int.TryParse(T, out int value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((int)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(uint))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(unsigned integer)", entry.message);
                var cat = PropertiesLayout.InputField(((uint)property.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (uint.TryParse(T, out uint value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((uint)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(long))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(integer)", entry.message);
                var cat = PropertiesLayout.InputField(((long)property.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (long.TryParse(T, out long value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((long)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(ulong))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(unsigned integer)", entry.message);
                var cat = PropertiesLayout.InputField(((ulong)property.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (ulong.TryParse(T, out ulong value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((ulong)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(short))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(integer)", entry.message);
                var cat = PropertiesLayout.InputField(((short)property.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (short.TryParse(T, out short value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((short)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(ushort))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name + "(unsigh integer)", entry.message);
                var cat = PropertiesLayout.InputField(((ushort)property.GetValue(that)).ToString(), entry.message);
                cat.AddListener(T =>
                {
                    if (ushort.TryParse(T, out ushort value))
                    {
                        property.SetValue(that, value);
                    }
                    else
                    {
                        cat.SetTextWithoutNotify(((ushort)property.GetValue(that)).ToString());
                    }
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(string))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name, entry.message);
                var cat = PropertiesLayout.InputField((string)property.GetValue(that), entry.message);
                cat.AddListener(T =>
                {
                    property.SetValue(that, T);
                });
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(Vector2))
            {
                PropertiesLayout.Label(property.Name, entry.message);
                var value = (Vector2)property.GetValue(that);

                PropertiesLayout.BeginHorizontal();
                var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                PropertiesLayout.EndHorizontal();

                catX.AddListener(T =>
                {
                    var value = (Vector2)property.GetValue(that);
                    if (float.TryParse(T, out float nx))
                    {
                        value.x = nx;
                        property.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.x.ToString());
                });
                catY.AddListener(T =>
                {
                    var value = (Vector2)property.GetValue(that);
                    if (float.TryParse(T, out float ny))
                    {
                        value.y = ny;
                        property.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.y.ToString());
                });

            }
            else if (type == typeof(Vector3))
            {
                PropertiesLayout.Label(property.Name, entry.message);
                var value = (Vector3)property.GetValue(that);

                PropertiesLayout.BeginHorizontal();
                var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                var catZ = PropertiesLayout.InputField(value.z.ToString(), entry.message);
                PropertiesLayout.EndHorizontal();

                catX.AddListener(T =>
                {
                    var value = (Vector3)property.GetValue(that);
                    if (float.TryParse(T, out float nx))
                    {
                        value.x = nx;
                        property.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.x.ToString());
                });
                catY.AddListener(T =>
                {
                    var value = (Vector3)property.GetValue(that);
                    if (float.TryParse(T, out float ny))
                    {
                        value.y = ny;
                        property.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.y.ToString());
                });
                catZ.AddListener(T =>
                {
                    var value = (Vector3)property.GetValue(that);
                    if (float.TryParse(T, out float nz))
                    {
                        value.z = nz;
                        property.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.z.ToString());
                });
            }
            else if (type == typeof(Vector4))
            {
                PropertiesLayout.Label(property.Name, entry.message);
                var value = (Vector4)property.GetValue(that);

                PropertiesLayout.BeginHorizontal();
                var catX = PropertiesLayout.InputField(value.x.ToString(), entry.message);
                var catY = PropertiesLayout.InputField(value.y.ToString(), entry.message);
                PropertiesLayout.EndHorizontal();
                PropertiesLayout.BeginHorizontal();
                var catZ = PropertiesLayout.InputField(value.z.ToString(), entry.message);
                var catW = PropertiesLayout.InputField(value.w.ToString(), entry.message);
                PropertiesLayout.EndHorizontal();

                catX.AddListener(T =>
                {
                    var value = (Vector4)property.GetValue(that);
                    if (float.TryParse(T, out float nx))
                    {
                        value.x = nx;
                        property.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.x.ToString());
                });
                catY.AddListener(T =>
                {
                    var value = (Vector4)property.GetValue(that);
                    if (float.TryParse(T, out float ny))
                    {
                        value.y = ny;
                        property.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.y.ToString());
                });
                catZ.AddListener(T =>
                {
                    var value = (Vector4)property.GetValue(that);
                    if (float.TryParse(T, out float nz))
                    {
                        value.z = nz;
                        property.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.z.ToString());
                });
                catW.AddListener(T =>
                {
                    var value = (Vector4)property.GetValue(that);
                    if (float.TryParse(T, out float nw))
                    {
                        value.w = nw;
                        property.SetValue(that, value);
                    }
                    else catX.SetTextWithoutNotify(value.w.ToString());
                });
            }
            else if (type == typeof(Color))
            {
                PropertiesLayout.ColorPanel(property.Name, (Color)property.GetValue(that), entry.message, T =>
                {
                    property.SetValue(that, T);
                });
            }
            else if (type == typeof(Texture2D))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Label(property.Name, entry.message);
                PropertiesLayout.RawImage((Texture2D)property.GetValue(that), entry.message);
                PropertiesLayout.EndHorizontal();
            }
            else if (type == typeof(Sprite))
            {
                PropertiesLayout.BeginHorizontal();
                PropertiesLayout.Image(property.Name, entry.message).CurrentImagePair = new() { SpriteSource = (Sprite)property.GetValue(that) };
                PropertiesLayout.EndHorizontal();
            }
            else DoGUI_Property_Extension(entry, property);
        }

        protected virtual void DoGUI_Field_Extension(ADSerializeEntry entry, FieldInfo field)
        {
            Debug.LogWarning("Cannt Generate This Type's UIComponent By Auto");
        }
        protected virtual void DoGUI_Property_Extension(ADSerializeEntry entry, PropertyInfo property)
        {
            Debug.LogWarning("Cannt Generate This Type's UIComponent By Auto");
        }

    }

    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public sealed class ADSerializeAttribute : Attribute
    {
        public string layer;
        public int index;
        public string message;

        /*
        public ADSerializeAttribute(string layer, int index, string message)
        {
            this.layer = layer;
            this.index = index;
            this.message = message;
        }
        */
    }

    [System.AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class ADActionButtonAttribute : Attribute
    {
        public string layer;
        public int index;
        public string message;
        public string methodName;

        /*
        public ADActionButtonAttribute(string layer, int index, string message, string methodName)
        {
            this.layer = layer;
            this.index = index;
            this.message = message;
            this.methodName = methodName;
        }
        */
    }

}
