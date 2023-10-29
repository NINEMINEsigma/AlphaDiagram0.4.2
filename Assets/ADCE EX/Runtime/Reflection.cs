using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using AD.Utility;
using System.Reflection;
using System.Linq;
using static AD.Utility.ReflectionExtension;
using System.Collections.ObjectModel;
using System.Xml.Linq;

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
                        var cat = fields[T] = new();
                        SetupEntry(cat, cad.NamedArguments);
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
                        var cat = propertys[T] = new();
                        SetupEntry(cat, cad.NamedArguments);
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
                        var cat = methods[T] = new();
                        SetupEntry(cat, cad.NamedArguments);
                        break;
                    }
                }
                return true;
            });

            static void SetupEntry(ADSerializeEntry cat, IList<CustomAttributeNamedArgument> temp_catas)
            {
                foreach (var cata in temp_catas)
                {
                    switch (cata.MemberName)
                    {
                        case "LayerTitle":
                            {
                                cat.layer = (string)cata.TypedValue.Value;
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
                        case "style":
                            {
                                cat.style = (string)cata.TypedValue.Value;
                            }
                            break;
                        case "isDefaultStyle":
                            {
                                cat.isDefaultStyle = (bool)cata.TypedValue.Value;
                            }
                            break;
                    }
                }
            }
        }

        public class ADSerializeEntry
        {
            public string layer;
            public int index;
            public string message;
            public string style;
            public bool isDefaultStyle = true;

            public ADSerializeEntry()
            {
            }

            public ADSerializeEntry(string layer,
                                    int index,
                                    string message,
                                    string style,
                                    bool isDefaultStyle)
            {
                this.layer = layer;
                this.index = index;
                this.message = message;
                this.style = style;
                this.isDefaultStyle = isDefaultStyle;
            }
        }

        private readonly Dictionary<FieldInfo, ADSerializeEntry> fields = new();
        private readonly Dictionary<PropertyInfo, ADSerializeEntry> propertys = new();
        private readonly Dictionary<MethodInfo, ADSerializeEntry> methods = new();

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
            if (true == true)
                Debug.Log("X");
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

    }

    [System.AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public sealed class ADSerializeAttribute : Attribute
    {
        public string layer;
        public int index;
        public string message;
        public string style;
        public bool isDefaultStyle = true;

        public ADSerializeAttribute(string layer, int index, string message, string style)
        {
            this.layer = layer;
            this.index = index;
            this.message = message;
            this.style = style;
            this.isDefaultStyle = false;
        }
        public ADSerializeAttribute(string layer, int index, string message)
        {
            this.layer = layer;
            this.index = index;
            this.message = message;
            this.isDefaultStyle = true;
        }
    }

    [System.AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public sealed class ADActionButtonAttribute : Attribute
    {
        public string layer;
        public int index;
        public string message;
        public string style;
        public bool isDefaultStyle = true;

        public ADActionButtonAttribute(string layer, int index, string message, string style)
        {
            this.layer = layer;
            this.index = index;
            this.message = message;
            this.style = style;
            this.isDefaultStyle = false;
        }
        public ADActionButtonAttribute(string layer, int index, string message)
        {
            this.layer = layer;
            this.index = index;
            this.message = message;
            this.isDefaultStyle = true;
        }
    }

}
