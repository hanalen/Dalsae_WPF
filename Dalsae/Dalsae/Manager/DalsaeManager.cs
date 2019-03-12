using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Dalsae.FileManager;
using static Dalsae.DalsaeManager;
using static Dalsae.TwitterWeb;
using static Dalsae.DataManager;
using static Dalsae.TweetManager;
using static Dalsae.Manager.ResponseAgent;
using static Dalsae.Manager.APICallAgent;
using static Dalsae.Web.UserStreaming;
using System.Windows;
using System.IO;
using Dalsae.API;
using Dalsae.Data;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using Dalsae.Web;
using System.Text.RegularExpressions;
using System.Net;
using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using System.Diagnostics;

namespace Dalsae
{
	public partial class DalsaeManager
	{
		private static DalsaeManager instence;
		private MainWindow window;
		private SoundPlayer notiSound;

		public static DalsaeManager DalsaeInstence { get { return GetInstence(); } }
		private Regex UrlMatch = new Regex(@"[(http|ftp|https):\/\/]*[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?");
		private static DalsaeManager GetInstence()
		{
			if (instence == null)
			{
				instence = new DalsaeManager();
				//instence.Init();
			}
			return instence;
		}



		public void Init()
		{
			SetResponseEvent();
			ThreadPool.SetMaxThreads(50, 20);//thread pool 세팅
			ServicePointManager.DefaultConnectionLimit = 48;
			ServicePointManager.MaxServicePoints = 48;
		}


		private void SetResponseEvent()
		{
			Manager.AccountAgent.accountInstence.OnChangeAccount += OnChangeAccount;
			Manager.AccountAgent.accountInstence.OnNoAccount += OnNoAccount;
			WebInstence.OnOAuthError += OAuthError;
			WebInstence.OnResponseError += ResponseError;

			usInstence.OnDelete += ResponseUSDelete;
			usInstence.OnTweet += ResponseUSTweet;
			usInstence.OnEvent += ResponseUSEvent;

			responseInstence.OnOAuth += ResponseOAuth;
			responseInstence.OnUser += ResponseMyInfo;

			responseInstence.OnFollowList += ResponseFollowList;
			responseInstence.OnFollowerIDS += ResponseSingleTweetFollowerIDS;
			responseInstence.OnBlockIds += ResponseBlockIds;
			responseInstence.OnRetweetOff += ResponseRetweetOff;

			responseInstence.OnHome += ResponseHome;
			responseInstence.OnMention += ResponseMention;
			responseInstence.OnFavoriteList += ResponseFavorite;
			responseInstence.OnUserTweet+= ResponseUserTweet;
			responseInstence.OnUserMedia+= RespnseUserMedia;

			responseInstence.OnHomeMore += ResponseHomeMore;
			responseInstence.OnMentionMore += ResponseMentionMore;
			responseInstence.OnFavoriteMore += ResponseFavoriteMore;
			responseInstence.OnUserMore += ResponseUserMore;
			responseInstence.OnUserMediaMore += ResponseUserMediaMore;

			responseInstence.OnRetweet += ResponseRetweet;
			responseInstence.OnUnRetweet += ResponseUnRetweet;
			responseInstence.OnFavorite += ResponseFavorite;
			responseInstence.OnUnFavorite += ResponseUnFavorite;
			responseInstence.OnDelete += ResponseDelete;

			responseInstence.OnSingleTweet += ResponseSingleTweet;

			responseInstence.OnMedia += ResponseMultimeida;
			TweetInstence.OnQt += ResponseSingleTweet;
		}

		private void ResponseSingleTweetFollowerIDS(ClientBlockIds ids)
		{
			apiInstence.GetFollowerIDS(DataInstence.userInfo.user.screen_name, ids.next_cursor);
		}

		private void OnNoAccount()
		{
			ClearClient();
			StartOAuthCertification();
		}

		private void OnChangeAccount(UserKey userkey)
		{
			DataInstence.UpdateToken(new Web.ResOAuth() { tokenStr = userkey.Token, secretStr = userkey.TokenSecret, isCallBack = false });
			LoadMyInfo();
		}

		public async void ResponseError(BasePacket packet, string json)
		{
			if (json == null) return;
			ClientAPIError error = JsonConvert.DeserializeObject<ClientAPIError>(json);
			if (error == null) return;
			if (error?.errors?.Count == 0) return;
			///리밋 걸렸을 때
			if (error.errors[0].code == 88)
			{
				if (packet.eresponse == eResponse.BLOCK_IDS)
				{
					await Task.Delay(TimeSpan.FromMinutes(1));
					int? num = Task.CurrentId;
					PacketBlockIds bpacket = packet as PacketBlockIds;
					long cursor = -1;
					long.TryParse(bpacket.cursor.ToString(), out cursor);
					apiInstence.GetBlockids(cursor);
				}
				else if (packet.eresponse == eResponse.FOLLOWING_LIST)
				{
					await Task.Delay(TimeSpan.FromMinutes(1));
					int? num = Task.CurrentId;
					PacketFollowingList fpacket = packet as PacketFollowingList;
					long cursor = -1;
					long.TryParse(fpacket.cursor.ToString(), out cursor);
					apiInstence.GetFollwing(DataInstence.userInfo.user.id, cursor);
				}
			}
			else if (error.errors[0].code == 408)
			{
				
			}
			if (error == null)
			{
				ShowMessageBox("원인 불명", "오류");
			}
			else if (error.errors == null)
			{
				ShowMessageBox(json, "오류");
			}
			else
			{
				if (error.errors.Count > 0)
				{
					string message = string.Empty;
					switch (error.errors[0].code)
					{
						case 32:
							ShowMessageBox("인증 오류. \r\n로그인 데이터를 초기화 합니다.\r\n다시 로그인 해 주세요.", "오류");
							Manager.AccountAgent.accountInstence.ClearAccountData();
							//FileInstence.ClearAccountData();
							ClearClient();
							StartOAuthCertification();
							break;
						case 34:
							message = "해당 유저는 없습니다.";
							break;
						case 44:
							message = "잘못 된 요청";
							break;
						case 64:
							ShowMessageBox("계정이 일시 정지 되었습니다.", "오류");
							break;
						case 87:
							ShowMessageBox("달새는 해당 동작을 할 수 없습니다.", "오류");
							break;
						case 88:
							message = "불러오기 제한, 몇 분 뒤 시도해주세요.";
							break;
						case 89:
							ShowMessageBox("잘못되거나 만료 된 토큰. 지속될 경우 Data폴더에 Switter를 지운 후 시도해주세요", "오류");
							break;
						case 130:
						case 131:
							message = "트위터 내부 오류";
							break;
						case 135:
							ShowMessageBox("인증할 수 없습니다.", "오류");
							break;
						case 136:
							ShowMessageBox("저런, 당신을 차단한 사람입니다.", "오류");
							break;
						case 139:
							message = "이미 관심글에 등록 된 트윗입니다.";
							break;
						case 144:
							message = "삭제된 트윗입니다.";
							break;
						case 150:
							message = "상대방에게 쪽지를 보낼 수 없습니다.";
							break;
						case 151:
							message = "메시지를 보내는 중 에러가 발생했습니다";
							break;
						case 179:
							message = "대화 트윗을 쓴 유저가 잠금 계정입니다.";
							break;
						case 185:
							ShowMessageBox("트윗 제한. 트잉여님 트윗 적당히 써주세요.", "오류");
							break;
						case 187:
							message = "중복 트윗입니다. 같은 내용을 적지 말아주세요 :(";
							break;
						case 327:
							message = "이미 리트윗 한 트윗입니다.";
							break;
						case 323:
							ShowMessageBox("GIF와 이미지를 동시에 업로드 할 수 없습니다.", "오류");
							break;
						case 324:
							ShowMessageBox("이미지 용량이 5mb를 넘어 업로드 할 수 없습니다.", "오류");
							break;
						case 386:
							message = "트윗이 280자를 넘어 전송할 수 없습니다.";
							break;
						default:
							message = json;
							break;
					}
					if (string.IsNullOrEmpty(message)) return;
					window.AddMessage(message);
				}
			}
		}

		#region 인증관련 함수들
		public void StartOAuthCertification()
		{
			apiInstence.GetOAuthKey();
			ShowInputPinForm();
		}

		public void ResponseOAuth(Web.ResOAuth oauth)
		{
			if (oauth.isCallBack)
			{
				DataInstence.UpdateToken(oauth);
				System.Diagnostics.Process.Start($"https://api.twitter.com/oauth/authorize?oauth_token={oauth.tokenStr}");
			}
			else
			{
				DataInstence.UpdateToken(oauth);
				Manager.AccountAgent.accountInstence.UpdateToken(DataInstence.userInfo);
				//FileInstence.UpdateToken(DataInstence.userInfo);
				LoadMyInfo();
			}
		}

		public void InputPin(string pin)
		{
			apiInstence.GetAccessToken(pin);
		}

		public void OAuthError()
		{
			ShowMessageBox("PIN 입력 오류", "오류");
			ShowInputPinForm();
		}

		private void ShowInputPinForm()
		{
			Dalsae.WindowAndControl.PinWindow pin = new WindowAndControl.PinWindow();
			pin.ShowInTaskbar = false;
			pin.Owner = Application.Current.MainWindow;
			pin.ShowDialog();
		}
		#endregion

		#region 로그인 유저 정보 가져오기
		private void LoadMyInfo()
		{
			apiInstence.GetMyInfo();
		}

		private void ResponseMyInfo(User user)
		{
			User.CopyUser(DataInstence.userInfo.user, user);
			Manager.AccountAgent.accountInstence.UpdateToken(DataInstence.userInfo);
			StartApiCalls();
		}
		#endregion

		#region 프로그램 시작 함수
		public void SetMainWindow(MainWindow window)
		{
			this.window = window;
		}

		public void LoadedMainWindow()
		{
			Manager.RefreshAgent.refreshAgent.SetWindowTick(window.RefreshTick);
			//인터넷 연결 확인
			if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == false)
				MessageBox.Show("인터넷 연결이 되어있지 않습니다.\r\n인터넷 연결을 확인 해주세요", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
			else
				StartDalsae();
		}

		private void StartDalsae()
		{
			//if (!string.IsNullOrEmpty(DataInstence.userInfo.Token) && !string.IsNullOrEmpty(DataInstence.userInfo.TokenSecret))//키가 없을 경우
			//	LoadMyInfo();
			//	StartOAuthCertification();
			//else

			if (DataInstence.option.isPlayNoti)
				ChangeSoundNoti(DataInstence.option.notiSound);

			//ThreadPool.QueueUserWorkItem(CheckNewVersion);
		}


		private void StartApiCalls()
		{
			Task.Factory.StartNew(CheckNewVersion);
			LoadTweet(eTweetPanel.eHome);
			LoadTweet(eTweetPanel.eMention);

            if (DataInstence.option.isUseStreaming && Process.GetProcesses().FirstOrDefault(x => x.ProcessName == "StreamingRespirator") != null)
            {
                //Process proStream = Process.GetProcesses().FirstOrDefault(x => x.ProcessName == "StreamingRespirator");
                //if (DataInstence.option.isAutoRunStreaming && File.Exists(DataInstence.option.streamFilePath) && proStream == null)
                //{
                //	Process p = Process.Start(DataInstence.option.streamFilePath);
                //}

                usInstence.ConnectUserStreaming();
            }
            else
            {
                Manager.RefreshAgent.refreshAgent.Reset();
            }
			GetRetweetOffIds();
			LoadFollowerIDS();
			if (DataInstence.option.isLoadBlock)//프로그램 시작 시 차단 가져오는 거
				LoadBlockIds();
			if (DataInstence.option.isLoadFollwing)//프로그램 시작 시 팔로 가져오는 거
				LoadFollowList();

			//LoadTweet(ePanel.eFavorite);
			//LoadTweet(ePanel.eDm);
		}

		#endregion

		#region 팔로잉 차단 목록 가져오기
		private void LoadFollowList()
		{
			apiInstence.GetFollwing(DataInstence.userInfo.user.id);
		}

		private void LoadFollowerIDS()
		{
			apiInstence.GetFollowerIDS(DataInstence.userInfo.user.screen_name);
		}

		private void LoadBlockIds()
		{
			apiInstence.GetBlockids();
		}

		private void ResponseFollowList(ClientUsers users)
		{
			apiInstence.GetFollwing(DataInstence.userInfo.user.id, users.next_cursor);
		}

		private void ResponseBlockIds(ClientBlockIds blockids)
		{
			apiInstence.GetBlockids(blockids.next_cursor);
		}

		private void GetRetweetOffIds()
		{
			apiInstence.GetRetweetOffIds();
		}
		#endregion

		#region 트윗 목록 요청
		public void LoadTweet(eTweetPanel panel, string userid = "")
		{
			apiInstence.LoadTweetList(panel, -1, userid);
		}

		public void LoadTweet(eTweetPanel panel, long sinceid, string userid = "")
		{
			apiInstence.LoadTweetList(panel, sinceid, userid);
		}

		public void LoadTweetMore(eTweetPanel panel, long cursor=-1, string userid="")
		{
			apiInstence.LoadTweetListMore(panel, cursor, userid);
		}

		private void ResponseHome(List<ClientTweet> listTweet)
		{
			for (int i = listTweet.Count - 1; i > -1; i--)
			{
				TweetInstence.AddTweet(eTweetPanel.eHome, listTweet[i]);
				ClientTweet tweet = listTweet[i];
				DalsaeUserInfo userInfo = DataInstence.userInfo;
				if (tweet?.user?.id == userInfo.user.id)
				{
					if (tweet.user.screen_name != userInfo.user.screen_name)
						userInfo.user.screen_name = tweet.user.screen_name;
					if (tweet.user.profile_image_url != userInfo.user.profile_image_url)
						userInfo.user.profile_image_url = tweet.user.profile_image_url;
				}
			}
			//TweetInstence.AddTweet(eTweetPanel.eHome, listTweet, false);
		}

		private void RespnseUserMedia(List<ClientTweet> listTweet)
		{
			TweetInstence.AddTweet(eTweetPanel.eUserMedia, listTweet, false);
		}

		private void ResponseMention(List<ClientTweet> listTweet)
		{
			for (int i = listTweet.Count - 1; i > -1; i--)
				TweetInstence.AddTweet(eTweetPanel.eMention, listTweet[i]);
			//TweetInstence.AddTweet(eTweetPanel.eMention, listTweet, false);
		}

		private void ResponseUserTweet(List<ClientTweet> listTweet)
		{
			for (int i = listTweet.Count - 1; i > -1; i--)
				TweetInstence.AddTweet(eTweetPanel.eUser, listTweet[i]);
		}

		private void ResponseFavorite(List<ClientTweet> listTweet)
		{
			for (int i = listTweet.Count - 1; i > -1; i--)
				TweetInstence.AddTweet(eTweetPanel.eFavorite, listTweet[i]);
		}

		private void ResponseHomeMore(List<ClientTweet> listTweet)
		{
			for (int i = 0; i < listTweet.Count; i++)
				TweetInstence.AddTweetMore(eTweetPanel.eHome, listTweet[i]);
			//TweetInstence.AddTweet(eTweetPanel.eHome, listTweet, true);
		}

		private void ResponseMentionMore(List<ClientTweet> listTweet)
		{
			for (int i = 0; i < listTweet.Count; i++)
				TweetInstence.AddTweetMore(eTweetPanel.eMention, listTweet[i]);
			//TweetInstence.AddTweet(eTweetPanel.eMention, listTweet, true);
		}

		private void ResponseFavoriteMore(List<ClientTweet> listTweet)
		{
			for (int i = 0; i < listTweet.Count; i++)
				TweetInstence.AddTweetMore(eTweetPanel.eFavorite, listTweet[i]);
			//TweetInstence.AddTweet(eTweetPanel.eFavorite, listTweet, true);
		}

		private void ResponseUserMore(List<ClientTweet> listTweet)
		{
			for (int i = 0; i < listTweet.Count; i++)
				TweetInstence.AddTweetMore(eTweetPanel.eUser, listTweet[i]);
			//TweetInstence.AddTweet(eTweetPanel.eUser, listTweet, true);
		}

		private void ResponseUserMediaMore(List<ClientTweet> listTweet)
		{
			List<ClientTweet> listMedia = new List<ClientTweet>();
			for(int i=0;i<listTweet.Count;i++)
			{
				if (listTweet[i].isMedia)
					listMedia.Add(listTweet[i]);
			}
			TweetInstence.AddTweet(eTweetPanel.eUserMedia, listMedia, true);
		}

		public void LoadSingleTweet(ClientTweet tweet)
		{
			if (tweet.isQTRetweet)
				apiInstence.LoadSingleTweet(tweet, tweet.originalTweet.quoted_status_id_str);
			if (string.IsNullOrEmpty(tweet.originalTweet.in_reply_to_status_id_str) == false)
				apiInstence.LoadSingleTweet(tweet, tweet.originalTweet.in_reply_to_status_id_str);
		}

		private void ResponseSingleTweet(ClientTweet tweet, UIProperty property)
		{
			if (tweet == null)
			{
				tweet = new ClientTweet("트윗을 표시할 수 없습니다.");
				return;
			}
			tweet.Init();
			if (DataInstence.isShowTweet(tweet, eTweetPanel.eHome) == false)
			{
				tweet.Init();
				tweet = new ClientTweet("트윗이 뮤트되었습니다.");
			}

			property.AddSingleTweet(tweet, tweet.uiProperty.isQtTweet ? tweet.uiProperty : tweet.uiProperty.parentTweet, false);
		}

		private void LoadDeahwaThread(object obj)
		{
			ClientTweet tweet = obj as ClientTweet;
			if (tweet == null) return;
			PacketSingleTweet parameter = new PacketSingleTweet(tweet.originalTweet.in_reply_to_status_id_str);
			string json = WebInstence.SyncRequest(parameter);
			ClientTweet dhTweet = JsonConvert.DeserializeObject<ClientTweet>(json);
			if (dhTweet == null)
			{
				DalsaeInstence.ShowMessageBox("이전 대화 유저가 잠금 계정 혹은 차단된 상태입니다.", "오류");
			}
			else
			{
				dhTweet.Init();
				UIProperty.DeleAddSingleTweet dele = new UIProperty.DeleAddSingleTweet(tweet.uiProperty.AddSingleTweet);
				Application.Current.Dispatcher.BeginInvoke
						(dele, new object[] { dhTweet, tweet.uiProperty.isQtTweet ? tweet.uiProperty : tweet.uiProperty.parentTweet, false });
			}
		}

		#endregion

		#region 프로그램 종료 함수

		#endregion

		#region 프로그램 구동 중 변경 함수

		#endregion

		#region 계정 추가 삭제 및 변경
		public void ClearClient()
		{
			apiInstence.StopSend();
			if (DataInstence.option.isUseStreaming == false)
				Manager.RefreshAgent.refreshAgent.Reset();
			usInstence.DisconnectStreaming();
			DataInstence.ClearToken();
			DeleClear dele = new DeleClear(TweetInstence.Clear);
			Application.Current.Dispatcher.BeginInvoke(dele);

			dele = new DeleClear(window.ClearWindow);
			Application.Current.Dispatcher.BeginInvoke(dele);

			GC.Collect();
		}

		public void AddAccount()
		{
			ClearClient();
			StartOAuthCertification();
		}

		public void ChangeAccount(string screen_name)
		{
			if (Manager.AccountAgent.accountInstence.ChangeAccountByName(screen_name))
			{
				ct?.Cancel();
				ClearClient();
				//LoadMyInfo();
			}
		}

		public void DeleteNowAccount()
		{
			ClearClient();
			//FileInstence.DeleteSelectAccount();
		}

		#endregion

		#region 트윗 전송 등

		public void SendTweet(ClientSendTweet tweet)
		{
			tweet.Reset();
			if (tweet.listBitmap?.Length > 0 || string.IsNullOrEmpty(tweet.multiPath) == false)
				SendMediaTweet(tweet);
			else
				apiInstence.SendTweet(tweet);
		}

		private void SendMediaTweet(ClientSendTweet tweet)
		{
			apiInstence.SendMedia(tweet);
		}

		private void ResponseMultimeida(ClientSendTweet tweet, ClientMultimedia media)
		{
			if(tweet.ResponseMedia(media))
			{
				apiInstence.SendTweet(tweet);
			}
			else
			{
				SendMediaTweet(tweet);
			}
		}

		public void Retweet(long id, bool isRetweet)
		{
			apiInstence.Retweet(id, isRetweet);
		}
		#region 플텍계 리트윗 관련
		private CancellationTokenSource ct = new CancellationTokenSource();
		private CancellationToken token;
		private BlockingCollection<Task> listTask = new BlockingCollection<Task>();
		private void TaskComplete(Task task)
		{
			listTask.TryTake(out task);
		}
		public void RetweetProtect(ClientTweet tweet)
		{
			if (tweet == null) return;
			ClientSendTweet sendPacket = new Web.ClientSendTweet();

			PacketUpdate parameter = new PacketUpdate(true);
			parameter.status = Generate.ReplaceTextExpend(tweet.originalTweet);
			parameter.status = parameter.status.Insert(0, "RT @**: ");
			sendPacket.SetTweet(parameter);
			if (tweet.isMedia)
			{
				Task t = Task.Factory.StartNew(new Action((() => LoadProtectTweetMedia(tweet, sendPacket))), token);
				t.ContinueWith(TaskComplete);
				listTask.TryAdd(t);
			}
			else
				SendTweet(sendPacket);
		}

		private void LoadProtectTweetMedia(ClientTweet tweet, ClientSendTweet sendTweet)
		{
			List<ClientMedia> listMedia = new List<Dalsae.ClientMedia>();
			for (int i = 0; i < tweet.mediaEntities.media.Count; i++)
				if (tweet.mediaEntities.media[i].type == "photo")
					listMedia.Add(tweet.mediaEntities.media[i]);

			List<BitmapImage> listBitmap = new List<BitmapImage>();
			foreach (ClientMedia item in listMedia)
			{
				sendTweet.parameter.status = sendTweet.parameter.status.Replace(item.expanded_url, "");
				try
				{
					WebRequest request = WebRequest.Create(item.media_url_https);
					using (WebResponse response = request.GetResponse())
					using (Stream stream = response.GetResponseStream())
					using (MemoryStream ms = new MemoryStream())
					{
						stream.CopyTo(ms);
						BitmapImage bitmap = new BitmapImage();
						bitmap.BeginInit();
						bitmap.StreamSource = new MemoryStream(ms.ToArray());
						listBitmap.Add(bitmap);
						bitmap.EndInit();
					}
				}
				catch (Exception e) { }
			}
			sendTweet.SetTweet(sendTweet.parameter, listBitmap.ToArray());
			SendTweet(sendTweet);
		}
		#endregion

		private void ResponseRetweet(ClientTweet tweet)
		{
			tweet.Init();
			TweetInstence.Retweet(tweet, true);
		}

		private void ResponseUnRetweet(ClientTweet tweet)
		{
			tweet.Init();
			TweetInstence.Retweet(tweet, false);
		}

		public void TweetDelete(long id)
		{
			apiInstence.DeleteTweet(id);
		}

		private void ResponseDelete(ClientTweet tweet)
		{
			TweetInstence.DeleteTweet(tweet);
		}

		public void RetweetOff(long id)
		{
			apiInstence.RetweetOff(id, DataInstence.hashRetweetOff.Contains(id));
		}

		public void ResponseRetweetOff(ClientFollowingUpdate relation)
		{
			if (DataInstence.UpdateRetweetOff(relation.relationship.target.id))
				ShowMessageBox($"{relation.relationship.target.screen_name}의 리트윗을 표시하지 않습니다.", "알림", MessageBoxImage.Information);
			else
				ShowMessageBox($"{relation.relationship.target.screen_name}의 리트윗을 표시합니다.", "알림", MessageBoxImage.Information);
		}


		public void Favorite(long id, bool isFavorite)
		{
			apiInstence.Favorite(id, isFavorite);
		}

		private void ResponseFavorite(ClientTweet tweet)
		{
			tweet.Init();
			TweetInstence.FavoriteTweet(tweet, tweet.favorited);
		}

		private void ResponseUnFavorite(ClientTweet tweet)
		{
			tweet.Init();
			TweetInstence.FavoriteTweet(tweet, tweet.favorited);
		}
		#endregion

		#region 유저스트리밍 리스폰스
		private void ResponseUSTweet(ClientTweet tweet)
		{
			tweet.Init();
			if (tweet.isRetweet && DataInstence.CheckIsMe(tweet.user.id))
				tweet.originalTweet.retweeted = true;
			TweetInstence.AddTweet(eTweetPanel.eHome, tweet);
			//DeleAddTweet dele = new DeleAddTweet(TweetInstence.AddTweet);
			//Application.Current.Dispatcher.BeginInvoke(dele, new object[] { eTweetPanel.eHome, tweet });
		}

		private void ResponseUSDelete(ClientStreamDelete delete)
		{
			//TweetInstence.DeleteTweet(delete);
			//DDeleteTweet dele = new DDeleteTweet(TweetInstence.DeleteTweet);
			//Application.Current.Dispatcher.BeginInvoke(dele, new object[] { delete });
		}

		private void ResponseUSEvent(StreamEvent streamEvent)
		{
			switch (streamEvent.Event)
			{
				case "unblock":
					DataInstence.UpdateBlockIds(streamEvent.target.id, false);
					break;
				case "block":
					DataInstence.UpdateBlockIds(streamEvent.target.id, true);
					break;
				case "follow":
					DataInstence.UpdateFollow(streamEvent.target, true);
					break;
				case "unfollow":
					DataInstence.UpdateFollow(streamEvent.target, false);
					break;
				case "delete":
					break;
			}
		}
		#endregion


		////핀 입력 시 핀값을 PinForm에서 받아온 후 AccessToken을 발급받는다
		////pin: pin값
		//public void UpdatePin(string pin)
		//{
		//	DalsaeInstence.InputPin(pin);
		//}



		////버전 체크하고 처음 api땡기는 거
		//private void StartDalsae()
		//{
		//	if (string.IsNullOrEmpty(DataInstence.userInfo.Token) ||
		//		string.IsNullOrEmpty(DataInstence.userInfo.TokenSecret))//키가 없을 경우
		//	{
		//		StartOAuthCertification();
		//	}
		//	else
		//	{
		//		LoadMyInfo();
		//	}
		//	if (DataInstence.option.isPlayNoti)
		//		ChangeSoundNoti(DataInstence.option.notiSound);

		//	ThreadPool.QueueUserWorkItem(CheckNewVersion);
		//}

		//----------------------------------------------------------------------------------------------------
		//---------------------------------------내부 기능-----------------------------------------------------
		//----------------------------------------------------------------------------------------------------



		//----------------------------------------------------------------------------------------------------
		//---------------------------------------UI 기능-----------------------------------------------------
		//----------------------------------------------------------------------------------------------------
		public void FocusPanel()
		{
			MainWindow.DeleFocusPanel dele = new MainWindow.DeleFocusPanel(window.FocusPanel);
			window.Dispatcher.BeginInvoke(dele);
		}

		public void OnMentionNoti()
		{
			window.NotiMention();
			PlaySoundNoti();
		}

		public void OnDmNoti()
		{
			window.NotiDM();
			PlaySoundNoti();
		}

		public void ChangeSmallUI(bool isSmall)
		{
			MainWindow.DeleChangeTopUI dele = new MainWindow.DeleChangeTopUI(window.SetTopUI);
			Application.Current.Dispatcher.BeginInvoke(dele, new object[] { isSmall });
		}
		
		public void ConnectedStreaming(bool isCon)
		{
			//TODO
			//스트리밍 현황
		}

		public void PlaySoundNoti()
		{
			if (notiSound != null && DataInstence.option.isPlayNoti)
				notiSound.Play();
		}
		public void ChangeSoundNoti(string path)
		{
			if (string.IsNullOrEmpty(path)) return;
			string soundPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\" +
										soundFolderPath + "\\" + path;
			if (File.Exists(soundPath) == false) return;
			notiSound = new SoundPlayer(soundPath);
			notiSound.Load();
		}

		public void ShowMessageBox(string message, string title, Action<MessageBoxResult> callback = null)
		{
			MainWindow.DeleShowMessageBox dele = new MainWindow.DeleShowMessageBox(window.ShowMessage);
			window.Dispatcher.BeginInvoke(dele, new object[] { message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning, callback, null });
		}

		/// <summary>
		/// 트윗 재전송 확인용 콜백 메시지 박스
		/// </summary>
		public void ShowMessageBox(string message, string title, Action<MessageBoxResult, BasePacket> callback = null)
		{
			MainWindow.DeleShowMessageBox dele = new MainWindow.DeleShowMessageBox(window.ShowMessage);
			window.Dispatcher.BeginInvoke(dele, new object[] { message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning, callback, null });
		}

		public void ShowMessageBox(string message, string title)
		{
			MainWindow.DeleShowMessageBox dele = new MainWindow.DeleShowMessageBox(window.ShowMessage);
			window.Dispatcher.BeginInvoke(dele, new object[] { message, title, MessageBoxButton.OK, MessageBoxImage.Warning, null, null});
		}

		public void ShowMessageBox(string message, string title, MessageBoxImage icon)
		{
			MainWindow.DeleShowMessageBox dele = new MainWindow.DeleShowMessageBox(window.ShowMessage);
			window.Dispatcher.BeginInvoke(dele, new object[] { message, title, MessageBoxButton.OK, icon, null, null});
		}

		public MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton button)
		{
			return MessageBox.Show(window, message, title, button, MessageBoxImage.None);
		}

		public void Reply(ClientTweet tweet)
		{
			MainWindow.DeleReplyAndQTRetweet dele = new MainWindow.DeleReplyAndQTRetweet(window.Reply);
			Application.Current.Dispatcher.BeginInvoke(dele, new object[] { tweet });
		}

		public void ReplyAll(ClientTweet tweet)
		{
			MainWindow.DeleReplyAndQTRetweet dele = new MainWindow.DeleReplyAndQTRetweet(window.ReplyAll);
			Application.Current.Dispatcher.BeginInvoke(dele, new object[] { tweet });
		}

		public void QTRetweet(ClientTweet tweet)
		{
			MainWindow.DeleReplyAndQTRetweet dele = new MainWindow.DeleReplyAndQTRetweet(window.QTRetweet);
			Application.Current.Dispatcher.BeginInvoke(dele, new object[] { tweet });
		}

		public void ProgramClosing()
		{
			Properties.Settings.Default.Save();
			//FileInstence.UpdateOption(DataInstence.option);//마지막 위치 때문에 항상 변경해야함
			//if (DataInstence.isChangeUserInfo)
			//	FileInstence.UpdateToken(DataInstence.userInfo);
			//FileInstence.UpdateHotkey(DataInstence.hotKey);
			//if (DataInstence.isChangeFollow && DataInstence.option.isLoadFollwing == false)
			//	FileInstence.UpdateFollowList(DataInstence.dicFollwing);
			if (DataInstence.isChangeBlock && DataInstence.option.isLoadBlock == false)
				FileInstence.UpdateBlockList(DataInstence.blockList);
		}
	}//class end
}
