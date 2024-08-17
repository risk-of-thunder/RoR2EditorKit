using EntityStates;
using R2API.ScriptableObjects;
using RoR2.Projectile;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ThunderKit.Core.Manifests.Datums;
using UnityEngine;
using UnityEngine.Networking;

namespace RoR2EditorKit.R2APIRelated
{
    public static class SerializableContentPackUtil
    {
        public static void SetScriptableObjects<T>(IEnumerable<ScriptableObject> objects, ContentArray contentArray)
        {
            SetScriptableObjects(typeof(T) , objects, contentArray);
        }

        public static void SetScriptableObjects(Type arrayType,  IEnumerable<ScriptableObject> objects, ContentArray contentArray)
        {
            var obj = objects.ToArray();
            var array = Array.CreateInstance(contentArray.contentType, obj.Length);
            for(int i = 0; i < array.Length; i++)
            {
                array.SetValue(obj[i], i);
            }
            contentArray.arrayField.SetValue(contentArray.tiedContentPack, array);
        }

        public static void SetGameObjects(IEnumerable<GameObject> gameObjects, ContentArray contentArray)
        {
            contentArray.arrayField.SetValue(contentArray.tiedContentPack, gameObjects);
        }

        public static void SetEntityStates(IEnumerable<Assembly> assemblies, ContentArray contentArray)
        {
            List<SerializableEntityStateType> serializableTypes = new List<SerializableEntityStateType>();
            foreach (var assembly in assemblies)
            {
                var entityStateTypes = assembly.GetTypesSafe()
                    .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(EntityState)))
                    .Select(t => new SerializableEntityStateType(t));
                serializableTypes.AddRange(entityStateTypes);
            }

            contentArray.arrayField.SetValue(contentArray.tiedContentPack, serializableTypes.ToArray());
        }

        public class ContentArray
        {
            public R2APISerializableContentPack tiedContentPack;
            public FieldInfo arrayField;
            public Type contentType;
            public string fieldName;

            private ContentArray()
            {

            }

            public ContentArray(R2APISerializableContentPack contentPack, string collectionName)
            {
                Type t = contentPack.GetType();
                FieldInfo fieldInfo = t.GetField(collectionName, BindingFlags.Instance | BindingFlags.Public);
                tiedContentPack = contentPack;
                arrayField = fieldInfo;
                contentType = fieldInfo.FieldType.GetElementType();
                fieldName = collectionName;
            }
        }
    }
}
