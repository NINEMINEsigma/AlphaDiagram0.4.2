using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AD.Utility
{
    public static class TransformExtension
    {
        private static Transform[] GetTransforms(this GameObject self, Func<GameObject, GameObject, int> predicate, bool IsActiveInHierarchy)
        {
            List<Transform> transforms = new List<Component>(
                self.GetComponentsInChildren(typeof(Transform))).ConvertAll(c => (Transform)c
                );
            transforms.RemoveAll(T => { return (T.gameObject.activeInHierarchy || !IsActiveInHierarchy) && T != self.transform; });

            transforms.Sort((T, P) => predicate(T.gameObject, P.gameObject));

            return transforms.ToArray();
        }

        public static void SortChilds(this GameObject self, Func<GameObject, GameObject, int> predicate, bool IsActiveInHierarchy = true)
        {
            foreach (Transform item in self.GetTransforms(predicate, IsActiveInHierarchy))
            {
                item.SetAsLastSibling();
            }
        }
    }
}
