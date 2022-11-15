using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.Leaderboard
{
    public class LeaderboardToSocialNetworkMediator : MonoBehaviour
    {
        protected List<SocialNetworkFriend> _friends = new List<SocialNetworkFriend>();

        public bool IsDataReady { get; protected set; }

        public Action OnDataReady = () => { };

        public virtual List<SocialNetworkFriend> Friends
        {
            get { return _friends; }
        }

        protected virtual IEnumerator Start()
        {
            yield return new WaitForSeconds(3f); 

            var ids = new []
            {
                "1889445011083769",
                "1142410789228464",
                "509333642759929",
                "101892137238243"
            };

            foreach (var id in ids)
            {
                var friendModel = new SocialNetworkFriend
                {
                    Name = "Test FriendName #" + id,
                    Uid = id
                };

                StartCoroutine(LoadUserImage(friendModel.Uid, friendModel));

                _friends.Add(friendModel);
            }

            IsDataReady = true;
            OnDataReady();
        }

        public virtual IEnumerator LoadUserName(string socialNetworkId, Action<string> onComplete)
        {
            if (string.IsNullOrEmpty(socialNetworkId)) yield break;

            yield return new WaitForSeconds(UnityEngine.Random.Range(0, 5));
            onComplete("Debug Loaded Name for uid " + socialNetworkId);
        }

        public virtual IEnumerator LoadUserImage(string socialNetworkId, SocialNetworkFriend target)
        {
            if (target == null) yield break;

            if (string.IsNullOrEmpty(socialNetworkId)) yield break;

            var uri = Application.isEditor
                ? "http://gt-race.net/temp/400x400.jpg"
                : "https://graph.facebook.com/" + socialNetworkId + "/picture?type=square";

            var request = new HTTPRequest(new Uri(uri), HTTPMethods.Get, (req, resp) =>
            {
                if (resp != null)
                {
                    target.Avatar = resp.DataAsTexture2D;
                }
            });

            request.Send();
        }

        public virtual IEnumerator LoadUserImage(string socialNetworkId, RawImage targetImage)
        {
            if (targetImage == null) yield break;

            if (string.IsNullOrEmpty(socialNetworkId)) yield break;

            var uri = Application.isEditor
                ? "http://gt-race.net/temp/400x400.jpg"
                : "https://graph.facebook.com/" + socialNetworkId + "/picture?type=square";

            var request = new HTTPRequest(new Uri(uri), HTTPMethods.Get, (req, resp) =>
            {
                if (resp != null && targetImage.gameObject != null)
                {
                    targetImage.texture = resp.DataAsTexture2D;
                }
            });

            request.Send();
        }

        public virtual string PlayerSocialId
        {
            get { return string.Empty; }
        }

        public virtual string UserName
        {
            get { return "UndefinedPlayerName" + UnityEngine.Random.Range(0, 10000); }
        }

        public virtual bool Connected
        {
            get { return true; }
        }
    }
}