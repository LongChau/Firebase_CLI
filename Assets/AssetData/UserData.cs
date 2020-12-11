using System;
using System.Collections;
using System.Collections.Generic;
using FirebaseCLI.Test.Manager;
using Sirenix.OdinInspector;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;

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
        public int counter;

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
            var chat = FirebaseManager.ListOfChats.RandomItem();
            EditorCoroutineUtility.StartCoroutineOwnerless(Send(loop, () => EnterChat(chat)));
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
        private IEnumerator Send(int loop, Action callback)
        {
            _loop = loop;
            var waitEndFrame = new WaitForEndOfFrame();
            var wait = new WaitForSecondsRealtime(1f);
            while (_loop > 0)
            {
                //yield return waitEndFrame;
                callback?.Invoke();
                _loop--;
                yield return wait;

                //Debug.Log("Finish");
            }
        }

        [Button(ButtonSizes.Medium)]
        [DisableIf("@_loop > 0")]
        public void AutoUploadScore(int loop)
        {
#if UNITY_EDITOR
            EditorCoroutineUtility.StartCoroutineOwnerless(Send(loop, () => UploadScore()));
#endif
        }

        [Button]
        public void UploadScore()
        {
            score = UnityEngine.Random.Range(50, 1000);
            AddScoreToLeaders(username, score);
        }

        [Button]
        public void UploadScoreWithoutTransaction()
        {
            score = UnityEngine.Random.Range(50, 1000);
            AddScoreToLeadersWithTransaction(username, score);
        }

        private void AddScoreToLeadersWithTransaction(string username, long score)
        {
            Dictionary<string, object> dictLeaderBoards = new Dictionary<string, object>();
            FirebaseManager.Instance.leaderBoardRef.GetValueAsync().ContinueWith(task_1 =>
            {
                if (task_1.IsFaulted)
                {
                    // Handle the error...
                }
                else if (task_1.IsCompleted)
                {
                    DataSnapshot snapshot = task_1.Result;
                    List<object> leaders = snapshot.Value as List<object>;

                    dictLeaderBoards = snapshot.Value as Dictionary<string, object>;
                    if (leaders == null)
                        leaders = new List<object>();
                    else if (snapshot.ChildrenCount >= FirebaseManager.MaxLeaderBoard)
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
                            return;
                        }
                        // Remove the lowest score.
                        leaders.Remove(minVal);
                    }

                    // Add the new high score.
                    Dictionary<string, object> newScoreMap = new Dictionary<string, object>();
                    newScoreMap["score"] = score;
                    newScoreMap["username"] = username;
                    leaders.Add(newScoreMap);
                    //mutableData.Value = leaders;

                    FirebaseManager.Instance.leaderBoardRef.SetValueAsync(leaders).ContinueWith(task_2 =>
                    {
                        if (task_2.IsFaulted)
                        {
                            // Handle the error...
                        }
                        else if (task_2.IsCompleted)
                        {

                        }
                    });
                }
            });
            //FirebaseManager.Instance.leaderBoardRef.UpdateChildrenAsync(dictLeaderBoards).ContinueWith(task =>
            //{
            //    if (task.IsFaulted)
            //    {
            //        // Handle the error...
            //    }
            //    else if (task.IsCompleted)
            //    {

            //    }
            //});
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

        [Button]
        public void UpdateCounter()
        {
            FirebaseDatabase.DefaultInstance
                .GetReference(FirebaseManager.Instance.counterPath)
                .SetValueAsync(counter)
                .ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        // Handle the error...
                        //Debug.Log($"PushMessage({DB_ClanChat} failed. \n Reason: {task.Exception}");
                    }
                    else if (task.IsCompleted)
                    {
                        //Debug.Log($"AddChatCount Succeed. {count}");
                    }
                });
        }
    }
}
