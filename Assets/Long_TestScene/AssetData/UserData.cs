using System;
using System.Collections;
using System.Collections.Generic;
using FirebaseCLI.Test.Manager;
using Sirenix.OdinInspector;
using Unity.EditorCoroutines.Editor;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FirebaseCLI.Test.Asset
{
    [CreateAssetMenu(menuName = "AssetData/UserData")]
    public class UserData : DataAsset
    {
        [OnValueChanged("UpdateObjectName")]
        public string username;

#if UNITY_EDITOR
        private void UpdateObjectName()
        {
            if (username.IsNullOrEmpty()) return;
            string assetPath = AssetDatabase.GetAssetPath(this.GetInstanceID());
            AssetDatabase.RenameAsset(assetPath, username);
        }
#endif

        [Button]
        private void MakeMeLeader()
        {
            FirebaseManager.Instance.MakeMeLeader(username);
        }

        [Button]
        private void EnterChat(string input = "")
        {
            string dbPath = FirebaseManager.Instance.DB_ClanChat;
            //string messageID = StringExtensions.RandomString("UUNNUU");
            string senderID = username;

            if (senderID.IsNullOrEmpty()) senderID = StringExtensions.RandomString("UUNNUU");

            ChatMessage message = new ChatMessage
            {
                SenderID = senderID,
                Message = input,
                TimeStamp = DateTime.Now.ToString()
            };

            FirebaseManager.Instance.PushMessage(dbPath, message);
        }

        [Button(ButtonSizes.Medium)]
        [DisableIf("@_loop > 0")]
        public void AutoChat(int loop)
        {
#if UNITY_EDITOR
            EditorCoroutineUtility.StartCoroutineOwnerless(Send(loop));
#endif
        }

        private int _loop;
        [DisplayAsString, ShowInInspector]
        public int Loop => _loop;
        IEnumerator Send(int loop)
        {
            _loop = loop;
            var waitEndFrame = new WaitForEndOfFrame();
            var wait = new WaitForSecondsRealtime(1f);
            while (_loop > 0)
            {
                yield return waitEndFrame;
                var chat = FirebaseManager.ListOfChats.RandomItem();
                EnterChat(chat);
                _loop--;
                yield return wait;

                //Debug.Log("Finish");
            }
        }
    }
}
