// Released under the MIT Licence as held at https://opensource.org/licenses/MIT
// https://github.com/garettbass/UnityExtensions.InspectInline
// https://gist.github.com/tomkail/ba4136e6aa990f4dc94e0d39ec6a058c

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

using Object = UnityEngine.Object;

namespace m4k
{

    [CustomPropertyDrawer(typeof(InspectInlineAttribute))]
    public class InspectInlineDrawer : PropertyDrawer
    {
        private static readonly int s_controlIdHash =
            nameof(InspectInlineDrawer).GetHashCode();

        private class GUIResources
        {
            public readonly GUIStyle
            inDropDownStyle = new GUIStyle("IN DropDown");

            public readonly GUIContent
            selectContent = new GUIContent("Select..."),
            createSubassetContent = new GUIContent("CREATE SUBASSET"),
            deleteSubassetContent = new GUIContent("Delete Subasset");
        }

        private static GUIResources s_gui;
        private static GUIResources gui
        {
            get
            {
                if (s_gui == null)
                    s_gui = new GUIResources();
                return s_gui;
            }
        }

        //----------------------------------------------------------------------

        private static readonly Dictionary<Type, Type[]>
        s_concreteTypes = new Dictionary<Type, Type[]>();

        private static Type[] GetConcreteTypes(Type type)
        {
            var concreteTypes = default(Type[]);
            if (s_concreteTypes.TryGetValue(type, out concreteTypes))
                return concreteTypes;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(a => a.GetTypes());
            concreteTypes =
                types
                .Where(t =>
                    t.IsAbstract == false &&
                    t.IsGenericTypeDefinition == false &&
                    type.IsAssignableFrom(t))
                .OrderBy(t => t.FullName.ToLower())
                .ToArray();

            s_concreteTypes.Add(type, concreteTypes);
            return concreteTypes;
        }

        //----------------------------------------------------------------------

        public new InspectInlineAttribute attribute
        {
            get { return (InspectInlineAttribute)base.attribute; }
        }

        // public InspectInlineDrawer() {
        //     EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
        //     EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
        // }

        // ~InspectInlineDrawer() {
        //     EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
        // }

        // Creates a new ScriptableObject via the default Save File panel
        static ScriptableObject CreateAssetWithSavePrompt (Type type, string path) {
            path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", type.Name+".asset", "asset", "Enter a file name for the ScriptableObject.", path);
            if (path == "") return null;
            ScriptableObject asset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset (asset, path);
            AssetDatabase.SaveAssets ();
            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            EditorGUIUtility.PingObject(asset);
            return asset;
        }
        
        Type GetFieldType () {
            Type type = fieldInfo.FieldType;
            if(type.IsArray) type = type.GetElementType();
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>)) type = type.GetGenericArguments()[0];
            return type;
        }

        static bool AreAnySubPropertiesVisible(SerializedProperty property) {
            var data = (ScriptableObject)property.objectReferenceValue;
            SerializedObject serializedObject = new SerializedObject(data);
            SerializedProperty prop = serializedObject.GetIterator();
            while (prop.NextVisible(true)) {
                if (prop.name == "m_Script") continue;
                return true; //if theres any visible property other than m_script
            }
            return false;
        }

        static string GetSelectedAssetPath(SerializedProperty property) {
            string selectedAssetPath = "Assets";
            if (property.serializedObject.targetObject is MonoBehaviour) {
                MonoScript ms = MonoScript.FromMonoBehaviour((MonoBehaviour) property.serializedObject.targetObject);
                selectedAssetPath = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(ms));
            }
            return selectedAssetPath;
        }

        //----------------------------------------------------------------------

        public override bool CanCacheInspectorGUI(SerializedProperty property)
        {
            return false;
        }

        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label)
        {
            // float totalHeight = EditorGUIUtility.singleLineHeight;
            // if(property.objectReferenceValue == null || !AreAnySubPropertiesVisible(property)){
            //     return totalHeight;
            // }
            // if(property.isExpanded) {
            //     var data = property.objectReferenceValue as ScriptableObject;
            //     if( data == null ) return EditorGUIUtility.singleLineHeight;
            //     SerializedObject serializedObject = new SerializedObject(data);
            //     SerializedProperty prop = serializedObject.GetIterator();
            //     if (prop.NextVisible(true)) {
            //         do {
            //             if(prop.name == "m_Script") continue;
            //             var subProp = serializedObject.FindProperty(prop.name);
            //             float height = EditorGUI.GetPropertyHeight(subProp, null, true) + EditorGUIUtility.standardVerticalSpacing;
            //             totalHeight += height;
            //         }
            //         while (prop.NextVisible(false));
            //     }
            //     // Add a tiny bit of height if open for the background
            //     totalHeight += EditorGUIUtility.standardVerticalSpacing;
            // }
            // return totalHeight;

            var height = EditorGUIUtility.singleLineHeight;
            if (property.isExpanded)
            {
                var serializedObject = property.serializedObject;
                var asset = serializedObject.targetObject;
                using (new ObjectScope(asset))
                {
                    var target = property.objectReferenceValue;
                    var targetExists = target != null;
                    if (targetExists && !ObjectScope.Contains(target))
                    {
                        var spacing = EditorGUIUtility.standardVerticalSpacing;
                        height += spacing;
                        height += GetInlinePropertyHeight(target);
                        height += 1;
                    }
                }
            }
            return height;
        }

        const int buttonWidth = 20;

        public override void OnGUI(
            Rect position,
            SerializedProperty property,
            GUIContent label)
        {
            EditorGUI.BeginProperty (position, label, property);
            // var type = GetFieldType();
            
            // if(type == null || ignoreClassFullNames.Contains(type.FullName)) {
            // 	EditorGUI.PropertyField(position, property, label);	
            // 	EditorGUI.EndProperty ();
            // 	return;
            // }
            
            ScriptableObject propertySO = null;
            if(!property.hasMultipleDifferentValues && property.serializedObject.targetObject != null && property.serializedObject.targetObject is ScriptableObject) {
                propertySO = (ScriptableObject)property.serializedObject.targetObject;
            }
            
            var propertyRect = Rect.zero;
            var guiContent = new GUIContent(property.displayName);

            // foldout 

            var foldoutRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);

            if(property.objectReferenceValue != null 
            && AreAnySubPropertiesVisible(property)
            ) {
                property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, guiContent, true);
            } else {
                foldoutRect.x += 12;
                EditorGUI.Foldout(foldoutRect, property.isExpanded, guiContent, true, EditorStyles.label);
            }

            var indentedPosition = EditorGUI.IndentedRect(position);
            var indentOffset = indentedPosition.x - position.x;
            propertyRect = new Rect(position.x + (EditorGUIUtility.labelWidth - indentOffset), position.y, position.width - (EditorGUIUtility.labelWidth - indentOffset), EditorGUIUtility.singleLineHeight);

            if(propertySO != null || property.objectReferenceValue == null) {
            	propertyRect.width -= buttonWidth;
            }

            // object
            
            EditorGUI.ObjectField(propertyRect, property, GUIContent.none);
            if (GUI.changed) property.serializedObject.ApplyModifiedProperties();

            // draw property
                
            if(property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null) {
                var data = (ScriptableObject)property.objectReferenceValue;
                
                if(property.isExpanded) {
                    // GUI.Box(new Rect(0, position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing - 1, Screen.width, position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing), "");

                    // EditorGUI.indentLevel++;
                    // SerializedObject serializedObject = new SerializedObject(data);
                    
                    // // Iterate over all the values and draw them
                    // SerializedProperty prop = serializedObject.GetIterator();
                    // float y = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    // if (prop.NextVisible(true)) {
                    //     do {
                    //         // Don't bother drawing the class file
                    //         if(prop.name == "m_Script") continue;
                    //         float height = EditorGUI.GetPropertyHeight(prop, new GUIContent(prop.displayName), true);
                    //         EditorGUI.PropertyField(new Rect(position.x, y, position.width, height), prop, true);
                    //         y += height + EditorGUIUtility.standardVerticalSpacing;
                    //     }
                    //     while (prop.NextVisible(false));
                    // }
                    // if (GUI.changed)
                    //     serializedObject.ApplyModifiedProperties();

                    // EditorGUI.indentLevel--;

                    var inlineRect = position;
                    inlineRect.yMin = propertyRect.yMax;
                    var spacing = EditorGUIUtility.standardVerticalSpacing;
                    // inlineRect.xMin += 2;
                    // inlineRect.xMax -= 18;
                    inlineRect.yMin += spacing;
                    // inlineRect.yMax -= 1;
                    DoInlinePropertyGUI(inlineRect, property.objectReferenceValue, true);
                }
            } 

            var buttonRect = new Rect(position.x + position.width - buttonWidth, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);

            if(GUI.Button(buttonRect, "")) {
                OnPropertyContextMenu(new GenericMenu(), property);
            }

            property.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndProperty ();

        // var propertyRect = position;
            // propertyRect.height = EditorGUIUtility.singleLineHeight;

            // DoContextMenuGUI(propertyRect, property);
            // DoObjectFieldGUI(propertyRect, property, label);
            // DoFoldoutGUI(propertyRect, property);

            // if (property.isExpanded)
            // {
            //     var serializedObject = property.serializedObject;
            //     var asset = serializedObject.targetObject;
            //     using (new ObjectScope(asset))
            //     {
            //         var target = property.objectReferenceValue;
            //         var targetExists = target != null;
            //         if (targetExists && !ObjectScope.Contains(target))
            //         {
            //             var enabled =
            //                 attribute.canEditRemoteTarget ||
            //                 TargetIsSubassetOf(asset, target);
            //             var inlineRect = position;
            //             inlineRect.yMin = propertyRect.yMax;
            //             var spacing = EditorGUIUtility.standardVerticalSpacing;
            //             inlineRect.xMin += 2;
            //             inlineRect.xMax -= 18;
            //             inlineRect.yMin += spacing;
            //             inlineRect.yMax -= 1;
            //             DoInlinePropertyGUI(inlineRect, target, enabled);
            //         }
            //     }
        // }
        
            DiscardObsoleteSerializedObjectsOnNextEditorUpdate();
        }

        void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
        {
            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            var type = GetFieldType();
            // var types = GetConcreteTypes(type);
            // Debug.Log(property.type);
            var splitInternalType = property.type.Split(new char[] {'$', '>'});
            
            if(property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null) {
                // Delete element and subasset option if existing
                if (TargetIsSubassetOf(property))
                    menu.AddItem(
                        gui.deleteSubassetContent,
                        on: false,
                        func: () => DestroyTarget(property));
                else
                    menu.AddDisabledItem(gui.deleteSubassetContent);
            }
            
            if (type.IsAbstract) {
                // var typeIndex = 0;

                foreach (var elem in type.Assembly.GetTypes().Where(t => type.IsAssignableFrom(t))) 
                {
                    if (elem.IsAbstract) 
                        continue;
                    menu.AddItem(new GUIContent($"Create/{elem.Name}"), false, (elem) => {
                        property.objectReferenceValue = CreateAssetWithSavePrompt(elem as Type, GetSelectedAssetPath(property));
                        property.serializedObject.ApplyModifiedProperties();
                    }, elem);

                    if(!attribute.canCreateSubasset)
                        continue;
                    menu.AddItem(new GUIContent($"Create subasset/{elem.Name}"), false, (elem) => {
                        AddSubasset(property, elem as Type);
                    }, elem);
                }
            }
            else {
                menu.AddItem(new GUIContent("Create"), false, () => {
                    property.objectReferenceValue = CreateAssetWithSavePrompt(type, GetSelectedAssetPath(property));
                });

                menu.AddItem(new GUIContent("Create subasset"), false, () => {
                    AddSubasset(property, type);
                });
            }
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete All Unreferenced Subassets"), false, () => {
                property.objectReferenceValue.DestroyAllUnreferencedSubassetsInAsset();
            });

            menu.ShowAsContext();
        }

        //----------------------------------------------------------------------

        private int GetControlID(Rect position)
        {
            var hint = s_controlIdHash;
            var focus = FocusType.Keyboard;
            return GUIUtility.GetControlID(hint, focus, position);
        }

    //----------------------------------------------------------------------

        // private void DoContextMenuGUI(
        //     Rect position,
        //     SerializedProperty property)
        // {
        //     if (attribute.canCreateSubasset == false)
        //         return;

        //     var controlID = GetControlID(position);
        //     ObjectSelector.DoGUI(controlID, property, SetObjectReferenceValue);

        //     var buttonRect = position;
        //     buttonRect.xMin = buttonRect.xMax - 16;
        //     var buttonStyle = EditorStyles.label;

        //     var isRepaint = Event.current.type == EventType.Repaint;
        //     if (isRepaint)
        //     {
        //         var dropDownStyle = gui.inDropDownStyle;
        //         var rect = buttonRect;
        //         rect.x += 2;
        //         rect.y += 6;
        //         dropDownStyle.Draw(rect, false, false, false, false);
        //     }

        //     var noLabel = GUIContent.none;
        //     if (GUI.Button(buttonRect, noLabel, buttonStyle))
        //     {
        //         var types = GetConcreteTypes(fieldInfo.FieldType);
        //         ShowContextMenu(
        //             buttonRect,
        //             controlID,
        //             property,
        //             types);
        //     }
        // }
    //----------------------------------------------------------------------

        private static void SetObjectReferenceValue(
            SerializedProperty property,
            Object newTarget)
        {
            var serializedObject = property.serializedObject;
            var oldSubassets = property.FindReferencedSubassets();
            property.objectReferenceValue = newTarget;
            property.isExpanded = true;
            if (oldSubassets.Any())
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                serializedObject.DestroyUnreferencedSubassets(oldSubassets);
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        //----------------------------------------------------------------------

        private bool AllowSceneObjects(SerializedProperty property)
        {
            var asset = property.serializedObject.targetObject;
            return asset != null && !EditorUtility.IsPersistent(asset);
        }

    //----------------------------------------------------------------------

        // private void DoObjectFieldGUI(
        //     Rect position,
        //     SerializedProperty property,
        //     GUIContent label)
        // {
        //     label = EditorGUI.BeginProperty(position, label, property);

        //     var objectType = fieldInfo.FieldType;
        //     var oldTarget = property.objectReferenceValue;
        //     var newTarget =
        //         EditorGUI.ObjectField(
        //             position,
        //             label,
        //             oldTarget,
        //             objectType,
        //             AllowSceneObjects(property));

        //     EditorGUI.EndProperty();
        //     if (!ReferenceEquals(newTarget, oldTarget))
        //     {
        //         SetObjectReferenceValue(property, newTarget);
        //     }
        // }

    //----------------------------------------------------------------------

        // private void DoFoldoutGUI(
        //     Rect position,
        //     SerializedProperty property)
        // {
        //     var foldoutRect = position;
        //     // foldoutRect.width = EditorGUIUtility.labelWidth;

        //     var target = property.objectReferenceValue;
        //     var targetExists = target != null;
        //     var isExpanded = targetExists && property.isExpanded;

        //     var noLabel = GUIContent.none;
        //     isExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, noLabel);

        //     if (targetExists)
        //     {
        //         property.isExpanded = isExpanded;
        //     }
        // }

    //----------------------------------------------------------------------

        // private void ShowContextMenu(
        //     Rect position,
        //     int controlID,
        //     SerializedProperty property,
        //     Type[] types)
        // {
        //     var menu = new GenericMenu();

        //     menu.AddItem(
        //         gui.selectContent,
        //         on: false,
        //         func: () => ShowObjectSelector(controlID, property));

        //     menu.AddSeparator("");

        //     var target = property.objectReferenceValue;
        //     if (target != null && TargetIsSubassetOf(property))
        //         menu.AddItem(
        //             gui.deleteSubassetContent,
        //             on: false,
        //             func: () => DestroyTarget(property));
        //     else
        //         menu.AddDisabledItem(gui.deleteSubassetContent);

        //     if (types.Length > 0)
        //     {
        //         menu.AddSeparator("");

        //         menu.AddDisabledItem(gui.createSubassetContent);

        //         var typeIndex = 0;
        //         var useTypeFullName = types.Length > 16;
        //         foreach (var type in types)
        //         {
        //             var createAssetMenuAttribute =
        //                 (CreateAssetMenuAttribute)
        //                 type.GetCustomAttribute(
        //                     typeof(CreateAssetMenuAttribute));
        //             var menuPath =
        //                 createAssetMenuAttribute != null
        //                 ? createAssetMenuAttribute.menuName
        //                 : useTypeFullName
        //                 ? type.FullName.Replace('.', '/')
        //                 : type.Name;
        //             var menuTypeIndex = typeIndex++;
        //             menu.AddItem(
        //                 new GUIContent(menuPath),
        //                 on: false,
        //                 func: () =>
        //                     AddSubasset(property, types, menuTypeIndex));
        //         }
        //     }

        //     menu.DropDown(position);
        // }

        //----------------------------------------------------------------------

        // private void ShowObjectSelector(
        //     int controlID,
        //     SerializedProperty property)
        // {
        //     var target = property.objectReferenceValue;
        //     var objectType = fieldInfo.FieldType;
        //     var allowSceneObjects = AllowSceneObjects(property);
        //     ObjectSelector.Show(
        //         controlID,
        //         target,
        //         objectType,
        //         property,
        //         allowSceneObjects);
        // }

    //----------------------------------------------------------------------

        private float GetInlinePropertyHeight(Object target)
        {
            var serializedObject = GetSerializedObject(target);
            serializedObject.Update();
            var height = 2f;
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var properties = serializedObject.EnumerateChildProperties();
            foreach (var property in properties)
            {
                height += spacing;
                height +=
                    EditorGUI
                    .GetPropertyHeight(property, includeChildren: true);
            }
            if (height > 0)
                height += spacing;
            return height;
        }

        private void DoInlinePropertyGUI(
            Rect position,
            Object target,
            bool enabled)
        {
            DrawInlineBackground(position);
            var serializedObject = GetSerializedObject(target);
            serializedObject.Update();
            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var properties = serializedObject.EnumerateChildProperties();
            position.xMin += 14;
            position.xMax -= 5;
            position.yMin += 1;
            position.yMax -= 1;
            EditorGUI.BeginDisabledGroup(!enabled);
            foreach (var property in properties)
            {
                position.y += spacing;
                position.height =
                    EditorGUI
                    .GetPropertyHeight(property, includeChildren: true);
                EditorGUI
                .PropertyField(position, property, includeChildren: true);
                position.y += position.height;
            }
            EditorGUI.EndDisabledGroup();
            if (enabled)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private static void DrawInlineBackground(Rect position)
        {
            var isRepaint = Event.current.type == EventType.Repaint;
            if (isRepaint)
            {
                // var style = new GUIStyle("ProgressBarBack");
                // var style = new GUIStyle("Badge");
                // var style = new GUIStyle("HelpBox");
                // var style = new GUIStyle("ObjectFieldThumb");
                var style = new GUIStyle("ShurikenEffectBg");
                using (ColorAlphaScope(0.5f))
                {
                    style.Draw(position, false, false, false, false);
                }
                // EditorGUI.DrawRect()
            }
        }

        //----------------------------------------------------------------------

        private readonly Dictionary<Object, SerializedObject>
        m_serializedObjectMap = new Dictionary<Object, SerializedObject>();

        private SerializedObject GetSerializedObject(Object target)
        {
            Debug.Assert(target != null);
            var serializedObject = default(SerializedObject);
            if (m_serializedObjectMap.TryGetValue(target, out serializedObject))
                return serializedObject;

            serializedObject = new SerializedObject(target);
            m_serializedObjectMap.Add(target, serializedObject);
            return serializedObject;
        }

        private void DiscardObsoleteSerializedObjects()
        {
            var map = m_serializedObjectMap;
            var destroyedObjects = map.Keys.Where(key => key == null);
            if (destroyedObjects.Any())
            {
                foreach (var @object in destroyedObjects.ToArray())
                {
                    map.Remove(@object);
                }
            }
        }

        private void DiscardObsoleteSerializedObjectsOnNextEditorUpdate()
        {
            EditorApplication.delayCall -= DiscardObsoleteSerializedObjects;
            EditorApplication.delayCall += DiscardObsoleteSerializedObjects;
        }

        //----------------------------------------------------------------------

        private static Object CreateInstance(Type type)
        {
            Debug.Assert(typeof(Object).IsAssignableFrom(type));
            return
                typeof(ScriptableObject).IsAssignableFrom(type)
                ? ScriptableObject.CreateInstance(type)
                : (Object)Activator.CreateInstance(type);
        }

        //----------------------------------------------------------------------

        private static bool TargetIsSubassetOf(SerializedProperty property)
        {
            var serializedObject = property.serializedObject;
            var asset = serializedObject.targetObject;
            var target = property.objectReferenceValue;
            return TargetIsSubassetOf(asset, target);
        }

        private static bool TargetIsSubassetOf(
            Object asset,
            Object target)
        {
            if (asset == null)
                return false;

            if (asset == target)
                return false;

            if (target == null)
                return false;

            var assetPath = AssetDatabase.GetAssetPath(asset);
            if (assetPath == null)
                return false;

            var targetPath = AssetDatabase.GetAssetPath(target);
            if (targetPath == null)
                return false;

            return assetPath == targetPath;
        }

        //----------------------------------------------------------------------

        private static bool CanAddSubasset(Object obj)
        {
            var hideFlags = obj.hideFlags;
            var dontSaveInBuild = HideFlags.DontSaveInBuild;
            if ((hideFlags & dontSaveInBuild) == dontSaveInBuild)
                return false;

            var dontSaveInEditor = HideFlags.DontSaveInEditor;
            if ((hideFlags & dontSaveInEditor) == dontSaveInEditor)
                return false;

            return true;
        }

        private void AddSubasset(
            SerializedProperty property,
            Type type)
        {
            var subassetType = type;

            var subasset = CreateInstance(subassetType);
            if (subasset == null)
            {
                Debug.LogErrorFormat(
                    "Failed to create subasset of type {0}",
                    subassetType.FullName);
                return;
            }

            if (!CanAddSubasset(subasset))
            {
                Debug.LogErrorFormat(
                    "Cannot save subasset of type {0}",
                    subassetType.FullName);
                TryDestroyImmediate(subasset, allowDestroyingAssets: true);
                return;
            }

            subasset.name = subassetType.Name;

            var serializedObject = property.serializedObject;
            serializedObject.targetObject.AddSubasset(subasset);
            SetObjectReferenceValue(property, subasset);
        }

        private void AddSubasset(
            SerializedProperty property,
            Type[] types,
            int typeIndex)
        {
            var subassetType = types[typeIndex];

            var subasset = CreateInstance(subassetType);
            if (subasset == null)
            {
                Debug.LogErrorFormat(
                    "Failed to create subasset of type {0}",
                    subassetType.FullName);
                return;
            }

            if (!CanAddSubasset(subasset))
            {
                Debug.LogErrorFormat(
                    "Cannot save subasset of type {0}",
                    subassetType.FullName);
                TryDestroyImmediate(subasset, allowDestroyingAssets: true);
                return;
            }

            subasset.name = subassetType.Name;

            var serializedObject = property.serializedObject;
            serializedObject.targetObject.AddSubasset(subasset);
            SetObjectReferenceValue(property, subasset);
        }

        //----------------------------------------------------------------------

        private void DestroyTarget(SerializedProperty property)
        {
            var target = property.objectReferenceValue;
            if (target != null)
            {
                SetObjectReferenceValue(property, null);
            }
        }

        //----------------------------------------------------------------------

        private static void TryDestroyImmediate(
            Object obj,
            bool allowDestroyingAssets = false)
        {
            try
            {
                if (obj != null)
                    Object.DestroyImmediate(obj, allowDestroyingAssets);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        //----------------------------------------------------------------------

        private struct ObjectScope : IDisposable
        {
            private static readonly HashSet<int> s_objectScopeSet =
                new HashSet<int>();

            private readonly int m_instanceID;

            public ObjectScope(Object obj)
            {
                m_instanceID = obj.GetInstanceID();
                s_objectScopeSet.Add(m_instanceID);
            }

            public void Dispose()
            {
                s_objectScopeSet.Remove(m_instanceID);
            }

            public static bool Contains(Object obj)
            {
                if (obj == null)
                    return false;
                var instanceID = obj.GetInstanceID();
                return s_objectScopeSet.Contains(instanceID);
            }
        }

        //======================================================================

        protected struct Deferred : IDisposable
        {
            private readonly Action _onDispose;

            public Deferred(Action onDispose)
            {
                _onDispose = onDispose;
            }

            public void Dispose()
            {
                if (_onDispose != null)
                    _onDispose();
            }
        }

        protected static Deferred ColorScope(Color newColor)
        {
            var oldColor = GUI.color;
            GUI.color = newColor;
            return new Deferred(() => GUI.color = oldColor);
        }

        protected static Deferred ColorAlphaScope(float a)
        {
            var oldColor = GUI.color;
            GUI.color = new Color(1, 1, 1, a);
            return new Deferred(() => GUI.color = oldColor);
        }

        protected static Deferred IndentLevelScope(int indent = 1)
        {
            EditorGUI.indentLevel += indent;
            return new Deferred(() => EditorGUI.indentLevel -= indent);
        }
    }

}
