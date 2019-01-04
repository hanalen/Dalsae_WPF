using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dalsae.Data;
using SharpRaven;
using Newtonsoft.Json;
using static Dalsae.Manager.ResponseAgent;

namespace Dalsae
{
	class DataManager
	{
		private static DataManager instence;
		public DalsaeUserInfo userInfo { get; private set; } = new DalsaeUserInfo();
		public ConcurrentDictionary<long, UserSemi> dicFollwing { get; set; } = new ConcurrentDictionary<long, UserSemi>();
		public HashSet<long> hashRetweetOff { get; private set; } = new HashSet<long>();
		public HashSet<long> hashFollowerIDS { get; private set; } = new HashSet<long>();
		public HotKeys hotKey { get; private set; }
		public Option option { get; set; }
		public BlockList blockList { get; set; } = new BlockList();
		public Skin skin { get; private set; }


		public bool isChangeOption { get; private set; } = false;
		public bool isChangeFollow { get; private set; } = false;
		public bool isChangeBlock { get; private set; } = false;
		public bool isChangeHotKey { get; private set; } = false;
		//public bool isChangeUserInfo { get; private set; } = false;

		public static DataManager DataInstence { get { return GetInstence(); } }
		private static DataManager GetInstence()
		{
			if (instence == null)
			{
				instence = new DataManager();
				instence.userInfo = new DalsaeUserInfo();
			}

			return instence;
		}

		#region 프로그램 시작 부분
		private void SetEventResponse()
		{
			responseInstence.OnBlockIds += ResponseBlockIds;
			responseInstence.OnFollowList += ResponseFollowList;
			responseInstence.OnTweet += ResponseTweet;
			responseInstence.OnRetweetOffIds += ResponseRetweetOffIds;
			responseInstence.OnFollowerIDS += ResponseFollowerIDS;
			//Web.UserStreaming.usInstence.OnTweet += ResponseTweet;
		}

		public void Init()
		{
			SetEventResponse();
		}
		#endregion

		public void SetOption(Option option)
		{
			option.CheckNullOption();
			this.option = option;
		}

		#region 팔로잉 차단 목록
		public void ResponseFollowList(ClientUsers users)
		{
			if (users == null) return;
			if (users.users == null) return;
			for (int i = 0; i < users.users.Length; i++)
			{
				if (dicFollwing.ContainsKey(users.users[i].id)) continue;
				User user = users.users[i];
				UserSemi usersemi = new UserSemi(user.name, user.screen_name, user.id, user.profile_image_url);
				dicFollwing.TryAdd(user.id, usersemi);
			}
		}

		private void ResponseFollowerIDS(ClientBlockIds ids)
		{
			hashFollowerIDS.UnionWith(ids.ids);
		}

		public void ResponseBlockIds(ClientBlockIds blockIds)
		{
			if (blockIds == null) return;
			if (blockIds.ids == null) return;

			blockList.hashBlockUsers.UnionWith(blockIds.ids);
		}

		private void ResponseRetweetOffIds(List<long> listUsers)
		{
			hashRetweetOff.UnionWith(listUsers);
		}
		#endregion

		private void ResponseTweet(ClientTweet tweet)
		{
			if (tweet?.user?.id == userInfo.user.id)
			{
				if (tweet.user.screen_name != userInfo.user.screen_name)
					userInfo.user.screen_name = tweet.user.screen_name;
				if (tweet.user.profile_image_url != userInfo.user.profile_image_url)
					userInfo.user.profile_image_url = tweet.user.profile_image_url;
			}
		}

		public bool CheckIsMe(string screenName)
		{
			if (string.Equals(userInfo.user.screen_name, screenName, StringComparison.OrdinalIgnoreCase))
				return true;
			else
				return false;
		}


		public bool CheckIsMe(long id)
		{
			if (userInfo.user.id == id)
				return true;
			else
				return false;
		}

		public void SetVideoWindowLocation(double left, double top, double width, double height)
		{
			Properties.Settings.Default.ptVideoX = left;
			Properties.Settings.Default.ptVideoY = top;
			Properties.Settings.Default.ptVideoWidth = width;
			Properties.Settings.Default.ptVideoHight = height;
		}

		public void SetImageWidowLocation(double left, double top, double width, double height)
		{
			Properties.Settings.Default.ptImgX = left;
			Properties.Settings.Default.ptImgY = top;
			Properties.Settings.Default.ptImgWidth = width;
			Properties.Settings.Default.ptImgHeight = height;
		}

		public void SetMainWindowLocation(double left, double top, double width, double height)
		{
			Properties.Settings.Default.ptMainX = left;
			Properties.Settings.Default.ptMainY = top;
			Properties.Settings.Default.ptMainWidth = width;
			Properties.Settings.Default.ptMainHeight = height;
		}

		public bool isShowTweet(ClientTweet tweet, eTweetPanel panel)
		{
			bool ret = true;

			if (tweet.originalTweet == null)
			{
				App.SendException("isshowtweet org tweet null", "");
				return false;
			}
			if (tweet.originalTweet.user == null)
			{
				App.SendException("isshowtweet org tweet user null", "");
				return false;
			}
			try
			{

				if (isBlockUser(tweet.originalTweet.user.id))
					ret = false;
				else if (tweet.isQTRetweet && tweet.quoted_status != null)//인용트윗
				{
					if (tweet.quoted_status.user != null)
						ret = !isBlockUser(tweet.quoted_status.user.id);//인용트윗의 유저 정보로 block체크하고 flag 뒤집어서 ret
				}
				else if (tweet.isQTRetweet && tweet.quoted_status == null)
					ret = false;
				else
					ret = !isBlockUser(tweet.entities.user_mentions);

				if (ret == false)//차단 된 유저일 경우 그냥 위에서 return
					return ret;

				if (panel == eTweetPanel.eMention)
				{
					if (option.isMuteMention)//멘션함 뮤트일경우 체크체크
					{
						if (option.MatchMuteWord(tweet.originalTweet.text))
							ret = false;
						else if (option.MatchMuteClient(tweet.originalTweet.source))
							ret = false;
						else if (option.MatchMuteUser(tweet.originalTweet.user.screen_name))
							ret = false;
						else if (option.MatchMuteTweet(tweet.originalTweet.id))
							ret = false;
						else if (option.MatchMuteTweet(tweet.in_reply_to_status_id_str))
							ret = false;
						else if (option.MatchMuteTweet(tweet.originalTweet.in_reply_to_status_id_str))
							ret = false;
					}
				}
				else if (panel == eTweetPanel.eDm)
				{
					ret = true;
				}
				else if (panel == eTweetPanel.eUser)
				{
					if (option.MatchMuteWord(tweet.originalTweet.text))
						ret = false;
					else if (option.MatchMuteClient(tweet.originalTweet.source))
						ret = false;
					else if (option.MatchMuteUser(tweet.originalTweet.user.screen_name))
						ret = false;
					else if (option.MatchMuteUser(tweet.user.screen_name))
						ret = false;
				}
				else
				{
					if (option.MatchMuteWord(tweet.originalTweet.text))
						ret = false;
					else if (option.MatchMuteClient(tweet.originalTweet.source))
						ret = false;
					else if (option.MatchMuteUser(tweet.originalTweet.user.screen_name))
						ret = false;
					else if (option.MatchMuteUser(tweet.user.screen_name))
						ret = false;
					else if (isRetweetOffUser(tweet.user.id) && tweet.retweeted)
						ret = false;
					else if (option.MatchMuteTweet(tweet.originalTweet.id))
						ret = false;
					else if (option.MatchMuteTweet(tweet.originalTweet.in_reply_to_status_id_str))
						ret = false;
					else if (option.MatchMuteTweet(tweet.in_reply_to_status_id_str))
						ret = false;
				}
			}
			catch(Exception e)
			{
				ret = false;
				App.SendException("isshow tweet", JsonConvert.SerializeObject(tweet));
			}
			return ret;
		}

		//public void UpdateMyScreenName(string screen_name)
		//{
		//	if (userInfo.user.screen_name != screen_name)
		//		userInfo.user.screen_name = screen_name;
		//}

		//public void UpdateMyProfilePicture(string url)
		//{
		//	if (userInfo.user.profile_image_url != url)
		//		userInfo.user.profile_image_url = url;
		//}


		public void UpdateBlockIds(long id, bool isAdd)
		{
			isChangeBlock = true;

			if (isAdd)
				blockList.hashBlockUsers.Add(id);
			else
				blockList.hashBlockUsers.Remove(id);
		}

		public bool UpdateRetweetOff(long id)
		{
			if (hashRetweetOff.Contains(id))
			{
				hashRetweetOff.Remove(id);
				return false;
			}
			else
			{
				hashRetweetOff.Add(id);
				return true;
			}
		}

		public bool isRetweetOffUser(long id)
		{
			bool ret = hashRetweetOff.Contains(id);

			return ret;
		}

		public bool isBlockUser(long id)
		{
			bool ret = blockList.hashBlockUsers.Contains(id);

			return ret;
		}

		public bool isBlockUser(List<ClientUserMentions> listId)
		{
			for (int i = 0; i < listId.Count; i++)
				if (blockList.hashBlockUsers.Contains(listId[i].id))
					return true;

			return false;
		}

		public void UpdateFollow(User user, bool isAdd)
		{
			isChangeFollow = true;
			if (isAdd)
			{
				UserSemi usersemi = new UserSemi(user.name, user.screen_name, user.id, user.profile_image_url);
				if (dicFollwing.ContainsKey(user.id) == false)
					dicFollwing.TryAdd(user.id, usersemi);
			}
			else
			{
				UserSemi semi = null;
				if (dicFollwing.ContainsKey(user.id))
					dicFollwing.TryRemove(user.id, out semi);
			}
		}
		
		public void UpdateHotkeys(HotKeys hotkeys)
		{
			this.hotKey = hotkeys;
			FileManager.FileInstence.UpdateHotkey(hotkeys);
		}

		public void UpdateSkin(Skin skin)
		{
			if (skin == null) return;
			if (this.skin == null) this.skin = new Skin();
			this.skin.blockOne = skin.blockOne;
			this.skin.blockTwo = skin.blockTwo;
			this.skin.bottomBar = skin.bottomBar;
			this.skin.defaultColor = skin.defaultColor;
			this.skin.leaveColor = skin.leaveColor;
			this.skin.mention = skin.mention;
			this.skin.mentionOne = skin.mentionOne;
			this.skin.mentionTwo = skin.mentionTwo;
			this.skin.menuColor = skin.menuColor;
			this.skin.retweet = skin.retweet;
			this.skin.select = skin.select;
			this.skin.topbar = skin.topbar;
			this.skin.tweet = skin.tweet;
		}

		public void UpdateToken(Web.ResOAuth oauth)
		{
			userInfo.UpdateToken(oauth);
			OAuth.GetInstence().OAuthToken = oauth.tokenStr;
		}

		//public void SetUserTokenSecret(string secret)
		//{
		//	userInfo.UpdateSecretToken(secret);
		//}

		//public void SetUserToken(string token)
		//{
		//	userInfo.UpdateToken(token);
		//	OAuth.GetInstence().OAuthToken = token;
		//}

		public void ClearToken()
		{
			userInfo.Clear();
			OAuth.GetInstence().Clear();
		}

		public void UpdateFollowingName(long id, string name)
		{
			if(dicFollwing.ContainsKey(id))
			{
				if (dicFollwing[id].name != name)
				{
					dicFollwing[id].UpdateName(name);
					isChangeFollow = true;
				}
			}
		}
	}


	//클라이언트 사용자 관련 값들을 들고있음
	public class DalsaeUserInfo
	{
		public string Token { get; private set; }
		public string TokenSecret { get; private set; }
		public User user { get; set; } = new User();
		public void Clear()
		{
			Token = string.Empty;
			TokenSecret = string.Empty;
			user.profile_image_url = "";
		}
		public void UpdateToken(Web.ResOAuth oauth)
		{
			this.Token = oauth.tokenStr;
			this.TokenSecret = oauth.secretStr;
		}

		//public void UpdateSecretToken(string secret)	{		this.TokenSecret = secret;	}
		//public void UpdateToken(string token)	{		this.Token = token;	}
		public void UpdateScreenName(string name)	{		user.screen_name = name;	}
		public void UpdateUser(User user)	{		this.user = user;	}
	}
}
