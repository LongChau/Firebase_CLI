using System;
using System.Collections;
using System.Collections.Generic;
using FirebaseCLI.Test.Manager;
using Sirenix.OdinInspector;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using Firebase.Database;

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
        public int score;

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

        [Button()]
        private void Reset()
        {
            _loop = 0;
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
                //yield return waitEndFrame;
                var chat = FirebaseManager.ListOfChats.RandomItem();
                EnterChat(chat);
                _loop--;
                yield return wait;

                //Debug.Log("Finish");
            }
        }

        [Button]
        public void AddScore()
        {
            score = UnityEngine.Random.RandomRange(50, 1000);
            AddScoreToLeaders(username, score);
        }

        private void AddScoreToLeaders(string username, long score)
        {
            FirebaseManager.Instance.leaderBoardRef.RunTransaction(mutableData =>
            {
                List<object> leaders = mutableData.Value as List<object>;

                if (leaders == null)
                    leaders = new List<object>();
                else if (mutableData.ChildrenCount >= FirebaseManager.MaxLeaderBoard)
                {
                    long minScore = long.MaxValue;
                    object minVal = null;
                    foreach (var child in leaders)
                    {
                        if (!(child is Dictionary<string, object>)) continue;

                        long childScore = (long)((Dictionary<string, object>)child)["score"];

                        if (childScore < minScore)
                        {
                            minScore = childScore;
                            minVal = child;
                        }
                    }
                    if (minScore > score)
                    {
                        // The new score is lower than the existing 5 scores, abort.
                        return TransactionResult.Abort();
                    }
                    // Remove the lowest score.
                    leaders.Remove(minVal);
                }

                // Add the new high score.
                Dictionary<string, object> newScoreMap = new Dictionary<string, object>();
                newScoreMap["score"] = score;
                newScoreMap["username"] = username;
                leaders.Add(newScoreMap);
                mutableData.Value = leaders;
                return TransactionResult.Success(mutableData);
            });
        }
    }
}
