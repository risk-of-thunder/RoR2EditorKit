using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor;
using System.Text.RegularExpressions;
using System.Reflection;

namespace RoR2.Editor
{
    public class CheckablePropertyField : PropertyField
    {
        private static float _tickCount;
        private static HashSet<CheckablePropertyField> _instances = new HashSet<CheckablePropertyField>();

        private static Type _serializedPropertyBindEventType = Type.GetType("UnityEditor.UIElements.SerializedPropertyBindEvent, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        private static FieldInfo _serializedPropertyBindEvent_m_BindPropertyField = Type.GetType("UnityEditor.UIElements.SerializedPropertyBindEvent, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetField("m_BindProperty", BindingFlags.NonPublic | BindingFlags.Instance);

        public SerializedProperty boundProperty { get; private set; }
        public SerializedProperty initialPopertyState;
        public event Action<ChangeEvent<object>> onChangeEventCaught;

        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
            var type = evt.GetType();
            if (type == _serializedPropertyBindEventType)
            {
                boundProperty = (SerializedProperty)_serializedPropertyBindEvent_m_BindPropertyField.GetValue(evt);
                initialPopertyState = new SerializedObject(boundProperty.serializedObject.targetObject).FindProperty(boundProperty.propertyPath);
            }
        }

        private void OnAttached(AttachToPanelEvent evt)
        {
            _instances.Add(this);
        }

        private void OnDetached(DetachFromPanelEvent evt)
        {
            _instances.Remove(this);
        }

        private void CheckPropertyEqualContents()
        {
            if (boundProperty == null)
                return;

            
            if(SerializedProperty.DataEquals(initialPopertyState, boundProperty))
            {
                Debug.Log("Property bound to " + this + " has not changed.");
                return;
            }

            Debug.Log("Property bound to " + this + " has changed!");
            boundProperty.serializedObject.ApplyModifiedProperties();
        }

        public CheckablePropertyField() : base()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        static CheckablePropertyField()
        {
            EditorApplication.update += TickChecks;
        }


        private static void TickChecks()
        {
            if (_instances.Count == 0)
                return;

            _tickCount++;
            if(_tickCount > R2EKSettings.instance.callsBetweenCheckablePropertyChecks)
            {
                _tickCount = 0;
                CheckProperties();
            }
        }

        private static void CheckProperties()
        {
            foreach(var instance in _instances)
            {
                instance.CheckPropertyEqualContents();
            }
        }

        public new class UxmlFactory : UxmlFactory<CheckablePropertyField, UxmlTraits>
        {
            //
            // Summary:
            //     Constructor.
            public UxmlFactory()
            {
            }
        }

        //
        // Summary:
        //     UxmlTraits for the PropertyField.
        public new class UxmlTraits : PropertyField.UxmlTraits
        {

            //
            // Summary:
            //     Constructor.
            public UxmlTraits() : base()
            {

            }
        }
    }
}