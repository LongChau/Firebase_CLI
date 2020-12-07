using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Firebase;
using Firebase.Database;
using Sirenix.OdinInspector;
using System;
using HugeTools.Network;
//using TinyTactics.Test.Data;

#if UNITY_EDITOR
//using Firebase.Unity.Editor;
using Unity.EditorCoroutines.Editor;
#endif

namespace FirebaseCLI.Test.Manager
{
    public class FirebaseManager : MonoSingletonExt<FirebaseManager>
    {
        [SerializeField]
        private string _dbChat = "server1/server_test/ClanData/messageReceived";
        private string chatCountPath = "server1/server_test/ClanData/chatCount";
        private string leaderPath = "server1/server_test/ClanData/leader";
        private string leaderboardPath = "server1/server_test/ClanData/leaderboard";

        [ReadOnly, ShowInInspector]
        private int count = 0;
        [ReadOnly, ShowInInspector]
        private string latestMember = "";

        private DatabaseReference _dbRootReference;
        public DatabaseReference leaderBoardRef;

        public string DB_ClanChat => $"{_dbChat}";
        public static long MaxLeaderBoard = 3;

        public override void Init()
        {
            base.Init();

        }

        private void Start()
        {
#if UNITY_EDITOR
            // Set this before calling into the realtime database.
            //FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://huge-game-server.firebaseio.com");
#endif

            // Get the root reference location of the database.
            _dbRootReference = FirebaseDatabase.DefaultInstance.RootReference;
            leaderBoardRef = _dbRootReference.Database.GetReference(leaderboardPath);

            FirebaseDatabase.DefaultInstance
                .GetReference(DB_ClanChat).ChildAdded += HandleChildAdded;

            FirebaseDatabase.DefaultInstance
                .GetReference(chatCountPath).ValueChanged += Handle_ChatValueChanged;

            FirebaseDatabase.DefaultInstance
                .GetReference(leaderPath).ValueChanged += Handle_LeaderValueChanged;
        }

        private void OnDestroy()
        {
            FirebaseDatabase.DefaultInstance
                .GetReference(DB_ClanChat).ChildAdded -= HandleChildAdded;

            FirebaseDatabase.DefaultInstance
                .GetReference(chatCountPath).ValueChanged -= Handle_ChatValueChanged;

            FirebaseDatabase.DefaultInstance
                .GetReference(leaderPath).ValueChanged -= Handle_LeaderValueChanged;
        }

        private void Handle_LeaderValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            Debug.Log($"Leader: {args.Snapshot.Value}");
        }

        private void Handle_ChatValueChanged(object sender, ValueChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            //Debug.Log($"Count: {args.Snapshot.Value}");
        }

        private void HandleChildAdded(object sender, ChildChangedEventArgs args)
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            count++;
            AddChatCount(count);

            ChatMessage msg = JsonUtility.FromJson<ChatMessage>(args.Snapshot.GetRawJsonValue());
            latestMember = msg.SenderID;

            //Debug.Log($"Key: {args.Snapshot.Key} || value: {args.Snapshot.Value}");

            // Do something with the data in args.Snapshot
            //ChatMessage newMsg = JsonUtility.FromJson<ChatMessage>(args.Snapshot.GetRawJsonValue());
            //Debug.Log($"New message incoming: {JsonUtility.ToJson(newMsg)}");
        }

        public void MakeMeLeader(string leaderName)
        {
            //Dictionary<string, object> entryValues = new Dictionary<string, object>
            //{
            //    { "leader", leaderName }
            //};

            var a = FirebaseDatabase.DefaultInstance
                .GetReference(leaderPath).SetValueAsync(leaderName).ContinueWith(task =>
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

        public void CreateClan()
        {

        }

        public void PullMessage(string dbPath)
        {
            FirebaseDatabase.DefaultInstance
                .GetReference(dbPath)
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        // Handle the error...
                        Debug.Log($"PullMessage({dbPath}) failed. \n Reason: {task.Exception}");
                    }
                    else if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        // Do something with snapshot...

                        Debug.Log($"PullMessage({dbPath}) succeed. \n Snapshot: {snapshot}");
                        Debug.Log($"PullMessage({dbPath}) succeed. \n Snapshot raw Json: {snapshot.GetRawJsonValue()}");

                        ChatMessage[] arrChatMsg = new ChatMessage[snapshot.ChildrenCount];
                        int childIndex = 0;

                        foreach (var child in snapshot.Children)
                        {
                            //Debug.Log($"{child.GetRawJsonValue()}");
                            ChatMessage chatMsg = JsonUtility.FromJson<ChatMessage>(child.GetRawJsonValue());
                            Debug.Log($"{JsonUtility.ToJson(chatMsg)}");

                            arrChatMsg[childIndex] = chatMsg;
                            childIndex++;
                        }

                        Debug.Log($"ArrMessage count: {arrChatMsg.Length}");
                    }
                });
        }

        public void PushMessage(string dbPath, string messageID, ChatMessage msg)
        {
            string key = FirebaseDatabase.DefaultInstance
                .GetReference(dbPath).Push().Key;

            Dictionary<string, object> entryValues = new Dictionary<string, object>();
            entryValues.Add(key, msg.ToDictionary());

            var pushTask = FirebaseDatabase.DefaultInstance.GetReference(dbPath).UpdateChildrenAsync(entryValues);
            pushTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // Handle the error...
                    //Debug.Log($"PushMessage({dbPath} {key} {msg}) failed. \n Reason: {task.Exception}");
                }
                else if (task.IsCompleted)
                {
                    //Debug.Log($"PushMessage({dbPath} {key} {msg}) Succeed.");
                    //RetrieveChatCount();
                    //count++;
                    //AddChatCount(count);
                }
            });
        }

        public void PushMessage(string dbPath, ChatMessage msg)
        {
            string key = FirebaseDatabase.DefaultInstance
                .GetReference(dbPath).Push().Key;

            Dictionary<string, object> entryValues = new Dictionary<string, object>();
            entryValues.Add(key, msg.ToDictionary());

            var pushTask = FirebaseDatabase.DefaultInstance.GetReference(dbPath).UpdateChildrenAsync(entryValues);
            pushTask.ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    // Handle the error...
                    //Debug.Log($"PushMessage({dbPath} {key} {msg}) failed. \n Reason: {task.Exception}");
                }
                else if (task.IsCompleted)
                {
                    //Debug.Log($"PushMessage({dbPath} {key} {msg}) Succeed.");
                    //RetrieveChatCount();
                    //count++;
                    //AddChatCount(count);
                }
            });
        }

        public void PushMessage(ChatMessage msg)
        {
            // auto generate message ID.
            string messageID = StringExtensions.RandomString("UUNNUU");

            // get the key.
            string key = FirebaseDatabase.DefaultInstance
                .GetReference(DB_ClanChat).Push().Key;

            Dictionary<string, object> entryValues = new Dictionary<string, object>();
            entryValues.Add(messageID, msg.ToDictionary());

            var a = FirebaseDatabase.DefaultInstance
                .GetReference(DB_ClanChat).UpdateChildrenAsync(entryValues).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        // Handle the error...
                        //Debug.Log($"PushMessage({DB_ClanChat} {messageID} {msg}) failed. \n Reason: {task.Exception}");
                        //return false;
                    }
                    else if (task.IsCompleted)
                    {
                        //Debug.Log($"PushMessage({DB_ClanChat} {messageID} {msg}) Succeed.");
                        //return true;
                    }
                });
            //a.Wait();
        }

        //[Button]
        private void Test_PullMessage()
        {
            string dbPath = $"{DB_ClanChat}/messages";
            PullMessage(dbPath);
        }

        #region CHAT AND PULL MESSAGE
        //[Button]
        private void Test_PushMessage()
        {
            string dbPath = $"{DB_ClanChat}";
            string messageID = StringExtensions.RandomString("UUNNUU");
            ChatMessage message = new ChatMessage
            {
                SenderID = StringExtensions.RandomString("UUNNUU"),
                Message = StringExtensions.RandomString("UUNNUU"),
                TimeStamp = DateTime.Now.ToString()
            };

            PushMessage(dbPath, messageID, message);
        }

        [Button]
        private void EnterChat(string username = "", string input = "")
        {
            string dbPath = DB_ClanChat;
            string messageID = StringExtensions.RandomString("UUNNUU");
            string senderID = username;

            if (senderID.IsNullOrEmpty()) senderID = StringExtensions.RandomString("UUNNUU");

            ChatMessage message = new ChatMessage
            {
                SenderID = senderID,
                Message = input,
                TimeStamp = DateTime.Now.ToString()
            };

            PushMessage(dbPath, messageID, message);
        }
        #endregion

        #region Transaction
        private void AddChatCount(int count)
        {
            //Debug.Log($"AddChatCount({count})");
            Dictionary<string, object> entryValues = new Dictionary<string, object>
            {
                { "chatCount", count }
            };

            string dbPath = $"server1/server_test/ClanData";

            var a = FirebaseDatabase.DefaultInstance
                .GetReference(dbPath).UpdateChildrenAsync(entryValues).ContinueWith(task =>
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

        //[Button]
        private void RetrieveChatCount()
        {
            Debug.Log("RetrieveChatCount()");
            string dbPath = $"server1/server_test/ClanData/chatCount";
            FirebaseDatabase.DefaultInstance
                .GetReference(dbPath)
                .GetValueAsync().ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        // Handle the error...
                        Debug.Log($"RetrieveChatCount failed: {task.Exception}");
                    }
                    else if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;
                        Debug.Log($"snapshot = {snapshot.Key} {snapshot.Value}");
                        int chatCount = (int)snapshot.Value;
                        Debug.Log($"chatCount = {chatCount}");
                        chatCount++;
                        //AddChatCount(chatCount);
                    }
                });
        }
        #endregion

        #region AUTO_CHAT
        public static List<string> ListOfUserNames = new List<string>
        {
            "Long", "Hiep", "Tina", "Huyen", "Kawamura", "Lien", "Hisasue", "Thong",
            "Anh_1", "Anh_2", "Nguyen", "Khoi", "Nhan", "Hang_1", "Hang_2"
        };

        public static List<string> ListOfChats = new List<string>
        {
            "Long dep trai qua", "Xin chao", "Em chua 18", "Tuyen nguoi yeu", "Deadline is coming", "I dunno", "Thunder, fire. Heed my call", "You need something?",
            "HaoHao", "Sanrio is waiting", "My leige?", "I'm Emperor!", "For Laichi!", "Bla bla bla", "Who is your daddy?"
        };

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
            var wait = new WaitForEndOfFrame();
            while (_loop > 0)
            {
                var username = ListOfUserNames.RandomItem();
                var chat = ListOfChats.RandomItem();
                EnterChat(username, chat);
                _loop--;
                yield return wait;

                Debug.Log("Finish");
            }
        }
        #endregion
        
        //[Button]
        public void AddScoreToLeaders(string username, long score)
        {
            leaderBoardRef.RunTransaction(mutableData =>
            {
                List<object> leaders = mutableData.Value as List<object>;

                if (leaders == null)
                    leaders = new List<object>();
                else if (mutableData.ChildrenCount >= MaxLeaderBoard)
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
