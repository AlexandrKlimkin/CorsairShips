using System;
using System.Collections;
using System.Collections.Generic;
using PestelLib.UI;
using UnityDI;
using UnityEngine;
using UnityEngine.UI;

namespace PestelLib.Leaderboard
{
    public class LeaderboardToSocialNetworkMediatorMock : LeaderboardToSocialNetworkMediator
    {
        public override List<SocialNetworkFriend> Friends => new List<SocialNetworkFriend>() {};

        protected override IEnumerator Start()
        {
            yield break;
        }

        public override IEnumerator LoadUserName(string socialNetworkId, Action<string> onComplete)
        {
            yield break;
        }

        public override IEnumerator LoadUserImage(string socialNetworkId, SocialNetworkFriend target)
        {
            yield break;
        }

        public override IEnumerator LoadUserImage(string socialNetworkId, RawImage targetImage)
        {
            yield break;
        }

        public override string PlayerSocialId => PlayerPrefs.GetString("PlayerSocialID", "debug_social_id");

        public override string UserName => PlayerPrefs.GetString("PlayerName", "Player");

        public override bool Connected => true;
    }
}