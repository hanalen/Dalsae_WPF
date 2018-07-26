using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dalsae.API;
using Dalsae.Web;

namespace Dalsae.Manager
{
	public class ResponseAgent
	{
		private static ResponseAgent _instence;
		public static ResponseAgent responseInstence { get { if (_instence == null) _instence = new ResponseAgent(); return _instence; } }

		public ResponseAgent()
		{
			//SetEvent();
		}



		#region 이벤트 모음
		#region 메인윈도우용
		public delegate void DResponse(eResponse response);
		public event DResponse OnResponse = null;
		#endregion

		#region 인증 이벤트
		public delegate void DOAuth(ResOAuth oauth);
		public event DOAuth OnOAuth = null;
		public delegate void DUser(User user);
		public event DUser OnUser = null;
		#endregion

		#region 팔로잉 차단 목록
		public delegate void DFollowList(ClientUsers users);
		public event DFollowList OnFollowList = null;
		public delegate void DBlockIds(ClientBlockIds blockIds);
		public event DBlockIds OnBlockIds = null;
		public delegate void DRetweetOffIds(List<long> listUsers);
		public event DRetweetOffIds OnRetweetOffIds = null;
		public delegate void DRetweetOff(ClientFollowingUpdate relation);
		public event DRetweetOff OnRetweetOff = null;
		#endregion

		#region 트윗 목록 요청
		public delegate void DTweetList(List<ClientTweet> listTweet);
		public event DTweetList OnHome= null;
		public event DTweetList OnMention = null;
		public event DTweetList OnFavoriteList = null;
		public event DTweetList OnUserTweet = null;
		public event DTweetList OnUserMedia = null;
		public event DTweetList OnHomeMore = null;
		public event DTweetList OnMentionMore = null;
		public event DTweetList OnFavoriteMore = null;
		public event DTweetList OnUserMore = null;
		public event DTweetList OnUserMediaMore = null;
		public delegate void DDMList(List<ClientDirectMessage> listDM);
		public event DDMList OnDMList = null;
		public delegate void DSingleTweet(ClientTweet tweet, UIProperty property);
		public event DSingleTweet OnSingleTweet = null;
		#endregion

		#region 트윗 쪽지 등 전송
		public delegate void DMultimedia(ClientSendTweet tweet, ClientMultimedia media);
		public event DMultimedia OnMedia = null;
		public delegate void DTweet(ClientTweet tweet);
		public event DTweet OnTweet = null;
		public event DTweet OnRetweet = null;
		public event DTweet OnUnRetweet = null;
		public event DTweet OnUnFavorite = null;
		public event DTweet OnFavorite = null;
		public event DTweet OnDelete = null;
		public delegate void DDM(ClientDirectMessage dm);
		public event DDM OnDM = null;
		#endregion
		#endregion

		#region 메인윈도우용
		private void Response(eResponse response)
		{
			if (OnResponse != null)
				Application.Current.Dispatcher.BeginInvoke(OnResponse, new object[] { response });
		}
		#endregion

		#region 트윗 디엠 등 전송 모음
		public void Retweet(ClientTweet tweet)
		{
			if (OnRetweet != null)
				Application.Current.Dispatcher.BeginInvoke(OnRetweet, new object[] { tweet });
		}

		public void UnRetweet(ClientTweet tweet)
		{
			if (OnUnRetweet != null)
				Application.Current.Dispatcher.BeginInvoke(OnUnRetweet, new object[] { tweet });
		}

		public void Favorite(ClientTweet tweet)
		{
			if (OnFavorite != null)
				Application.Current.Dispatcher.BeginInvoke(OnFavorite, new object[] { tweet });
		}

		public void UnFavorite(ClientTweet tweet)
		{
			if (OnUnFavorite != null)
				Application.Current.Dispatcher.BeginInvoke(OnUnFavorite, new object[] { tweet });
		}

		public void Multimedia(ClientSendTweet tweet, ClientMultimedia media)
		{
			if (OnMedia != null)
				Application.Current.Dispatcher.BeginInvoke(OnMedia, new object[] { tweet, media });
		}

		public void Tweet(ClientTweet tweet)
		{
			if (OnTweet != null)
				Application.Current.Dispatcher.BeginInvoke(OnTweet, new object[] { tweet });
		}

		public void Delete(ClientTweet tweet)
		{
			if (OnDelete != null)
				Application.Current.Dispatcher.BeginInvoke(OnDelete, new object[] { tweet });
		}

		public void DM(ClientDirectMessage dm)
		{
			if (OnBlockIds != null)
				Application.Current.Dispatcher.BeginInvoke(OnDM, new object[] { dm });
		}

		#endregion

		#region 인증 및 차단 팔로잉 요청

		public void OAuth(ResOAuth oauth)
		{
			if (OnOAuth != null)
				Application.Current.Dispatcher.BeginInvoke(OnOAuth, new object[] { oauth });
		}

		public void MyInfo(User user)
		{
			if (OnUser != null)
				Application.Current.Dispatcher.BeginInvoke(OnUser, new object[] { user });
		}

		public void Followlist(ClientUsers user)
		{
			if (OnFollowList != null)
				Application.Current.Dispatcher.BeginInvoke(OnFollowList, new object[] { user });
			Response(eResponse.FOLLOWING_LIST);
		}

		public void BlockIds(ClientBlockIds blockIds)
		{
			if (OnBlockIds != null)
				Application.Current.Dispatcher.BeginInvoke(OnBlockIds, new object[] { blockIds });
			Response(eResponse.BLOCK_IDS);
		}

		public void RetweetOffIds(List<long> listUsers)
		{
			if (OnRetweetOffIds != null)
				Application.Current.Dispatcher.BeginInvoke(OnRetweetOffIds, new object[] { listUsers });
			Response(eResponse.RETWEET_OFF_IDS);
		}

		public void RetweetOff(ClientFollowingUpdate relation)
		{
			if (OnRetweetOff != null)
				Application.Current.Dispatcher.BeginInvoke(OnRetweetOff, new object[] { relation });
			Response(eResponse.FOLLOWING_UPDATE);
		}
		#endregion

		#region 트윗 목록 요청

		public void Home(List<ClientTweet> listTweet)
		{
			if (OnHome != null)
				Application.Current.Dispatcher.BeginInvoke(OnHome, new object[] { listTweet });
		}

		public void Mention(List<ClientTweet> listTweet)
		{
			if (OnMention != null)
				Application.Current.Dispatcher.BeginInvoke(OnMention, new object[] { listTweet });
		}

		public void FavoriteTweet(List<ClientTweet> listTweet)
		{
			if (OnFavoriteList != null)
				Application.Current.Dispatcher.BeginInvoke(OnFavoriteList, new object[] { listTweet });
		}

		public void UserTweet(List<ClientTweet> listTweet)
		{
			if (OnUserTweet != null)
				Application.Current.Dispatcher.BeginInvoke(OnUserTweet, new object[] { listTweet });
		}

		public void UserTweetMedia(List<ClientTweet> listTweet)
		{
			List<ClientTweet> listmedia = new List<ClientTweet>();
			for (int i = 0; i < listTweet.Count; i++)
			{
				listTweet[i].Init();
				if (listTweet[i].isMedia)
					listmedia.Add(listTweet[i]);
			}
			if (OnUserMedia != null)
				Application.Current.Dispatcher.BeginInvoke(OnUserMedia, new object[] { listmedia });
		}

		public void DMList(List<ClientDirectMessage> listDM)
		{
			
		}

		public void HomeMore(List<ClientTweet> listTweet)
		{
			if (OnHomeMore != null)
				Application.Current.Dispatcher.BeginInvoke(OnHomeMore, new object[] { listTweet });
		}

		public void MentionMore(List<ClientTweet> listTweet)
		{
			if (OnMentionMore != null)
				Application.Current.Dispatcher.BeginInvoke(OnMentionMore, new object[] { listTweet });
		}

		public void FavoriteMore(List<ClientTweet> listTweet)
		{
			if (OnFavoriteMore != null)
				Application.Current.Dispatcher.BeginInvoke(OnFavoriteMore, new object[] { listTweet });
		}

		public void UserMore(List<ClientTweet> listTweet)
		{
			if (OnUserMore != null)
				Application.Current.Dispatcher.BeginInvoke(OnUserMore, new object[] { listTweet });
		}

		public void UserTweetMediaMore(List<ClientTweet> listTweet)
		{
			List<ClientTweet> listmedia = new List<ClientTweet>();
			for (int i = 0; i < listTweet.Count; i++)
			{
				listTweet[i].Init();
				if (listTweet[i].isMedia)
					listmedia.Add(listTweet[i]);
			}
			if (OnUserMediaMore != null)
				Application.Current.Dispatcher.BeginInvoke(OnUserMediaMore, new object[] { listmedia });
		}

		public void SingleTweet(ClientTweet tweet, UIProperty property)
		{
			if (OnSingleTweet != null)
				Application.Current.Dispatcher.BeginInvoke(OnSingleTweet, new object[] { tweet,property });
		}
		#endregion
	}
}
