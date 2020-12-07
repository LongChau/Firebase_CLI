using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;

using UnityEngine;
using Random = UnityEngine.Random;
using Sirenix.Serialization;

#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace FirebaseCLI.Test.Asset
{
    [CreateAssetMenu(menuName = "Test/ClanDataGenerator")]
    public class ClanDataGenerator : DataAsset
    {
        #region Fields
        //[SerializeField]
        //private ClanData _clanData;
        //[SerializeField]
        [OdinSerialize]
        private Clan _clanData;

        private int _loop;
        #endregion

        #region Properties
        //public ClanData ClanData => _clanData;
        public Clan ClanData => _clanData;

        [DisplayAsString]
        [ShowInInspector]
        public int Loop => _loop;
        #endregion

        #region Functions
        [Button(ButtonSizes.Medium)]
        private void Test_ConvertJSON()
        {
            var value = JsonUtility.ToJson(ClanData);
            //var value = ClanData.ToJson();
            Debug.Log($"JSON value: <color=green> \n {value} \n </color>");
        }

        [Button(ButtonSizes.Medium)]
        [DisableIf("@_loop > 0")]
        public void Generate(int loop)
        {
#if UNITY_EDITOR
            //EditorCoroutineUtility.StartCoroutineOwnerless(Send(loop));
#endif
        }

        //IEnumerator Send(int loop)
        //{
        //    _loop = loop;
        //    while (_loop > 0)
        //    {
        //        var url = "https://us-central1-huge-game-Server.cloudfunctions.net/execute";
        //        var key = "clans";
        //        var value = ClanData;

        //        var body = new Dictionary<string, object>
        //        {
        //            {"TitleId", "server_dev_long"},
        //            {"FunctionName", "CreateClan"},
        //            { "ClanID", key},
        //            { "ClanData", value},
        //        };

        //        _loop--;
        //        yield return Post.Send(url, body, Handle_Post);

        //        Debug.Log("Finish");
        //    }

        //    void Handle_Post(string result)
        //    {
        //        Debug.Log(result);
        //    }
        //}
        #endregion
    }
}