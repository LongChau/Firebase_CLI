using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace FirebaseCLI.Test.Asset
{
    public class DataAsset : SerializedScriptableObject
    {
        [ReadOnly]
        public int hash;

        public virtual void Reload()
        {
            hash = name.Hash();
        }

        private void OnEnable()
        {
            Reload();
        }
    }
}