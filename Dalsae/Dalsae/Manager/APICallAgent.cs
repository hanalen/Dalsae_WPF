using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Dalsae.API;
using Dalsae.Data;
using Dalsae.Web;
using System.Windows.Threading;
using static Dalsae.Manager.ResponseAgent;
using static Dalsae.TwitterWeb;

namespace Dalsae.Manager
{
	public class APICallAgent
	{
		private static APICallAgent _instence;
		public static APICallAgent apiInstence { get { if (_instence == null) _instence = new APICallAgent(); return _instence; } }
		//동일 패킷(리트윗, 관글, 대화 불러오기) 같은 건 한 번 받으면 더 호출 안 하게 막아보자
		//task객체로 해서 뭐 구분하면 될 거도 같고.......
		//웹에서 에러 구현을 IObservable로 하는 건?
		//에러 콜백을 웹에 등록 해두면 에러나면 그냥 그거만 호출 하게

		public delegate void DApiCall(eResponse response);
		public event DApiCall OnApiCall = null;

		private BlockingCollection<Task> listTask = new BlockingCollection<Task>();
		public void StopSend()
		{
			ct.Cancel();
			ct = new CancellationTokenSource();
			token = ct.Token;
		}

		#region 인증 관련
		public void GetOAuthKey()
		{
			PacketGetOAuth packet = new PacketGetOAuth();
			RequestOAuth(packet, responseInstence.OAuth);
		}

		public void GetAccessToken(string pin)
		{
			PacketGetAccessToken packet = new PacketGetAccessToken();
			packet.oauth_verifier = pin;
			RequestOAuth(packet, responseInstence.OAuth);
		}

		public void GetMyInfo()
		{
			PacketVerifyCredentials packet = new PacketVerifyCredentials();
			RequestPacket<User>(packet, responseInstence.MyInfo);
		}

		public void GetUserInfo_Chain(long id)
		{
			PacketUserShow packet = new PacketUserShow(id);
			RequestPacket<UserInfo>(packet, responseInstence.UserInfo_Chain);
		}

		public void GetUserInfo(string screenName="")
		{
			if (screenName == string.Empty)
			{
				PacketVerifyCredentials packet = new PacketVerifyCredentials();
				RequestPacket<UserInfo>(packet, responseInstence.UserInfo);
			}
			else
			{
				PacketUserShow packet = new PacketUserShow(screenName);
				RequestPacket<UserInfo>(packet, responseInstence.UserInfo);
			}
		}

		#endregion

		#region 팔로잉 블락 목록 가져옴
		public void GetFollwing(long id, long cursor = -1)
		{
			if (cursor == 0) return;
			PacketFollowingList packet = new PacketFollowingList();
			packet.user_id = id;
			packet.count = 200;
			packet.cursor = cursor;
			RequestPacket<ClientUsers>(packet, responseInstence.Followlist);
		}

		public void GetBlockids(long cursor = -1)
		{
			if (cursor == 0) return;
			PacketBlockIds packet = new PacketBlockIds();
			packet.cursor = cursor;
			RequestPacket<ClientBlockIds>(packet, responseInstence.BlockIds);
		}

		public void GetRetweetOffIds()
		{
			PacketGetRetweetOffIds packet = new PacketGetRetweetOffIds();
			RequestPacket<List<long>>(packet, responseInstence.RetweetOffIds);
		}

		public void RetweetOff(long id, bool isOff)
		{
			PacketUpdateFollowingData parameter = new PacketUpdateFollowingData();
			parameter.user_id = id;
			parameter.retweets = isOff;
			RequestPacket<ClientFollowingUpdate>(parameter, responseInstence.RetweetOff);
		}

		public void GetFollowingIDS(string screenName, long cursor=-1)
		{
			PacketFollowingIds packet = new PacketFollowingIds(screenName, cursor);
			RequestPacket<ClientBlockIds>(packet, responseInstence.FollowingIDS);
		}

		public void GetFollowerIDS_Chain(string screenName, long cursor=-1)
		{
			PacketFollowerIds packet = new PacketFollowerIds(screenName, cursor);
			RequestPacket<ClientBlockIds>(packet, responseInstence.FollowerIDS_Chain);
		}

		public void GetFollowerIDS(string screenName, long cursor = -1)
		{
			if (cursor == 0) return;
			PacketFollowerIds packet = new PacketFollowerIds(screenName, cursor);
			RequestPacket<ClientBlockIds>(packet, responseInstence.FollowerIDS);
		}

		public void Block(long userID)
		{
			PacketBlockCreate packet = new PacketBlockCreate(userID);
			RequestPacket<Data.UserInfo>(packet, responseInstence.BlockCreate, DispatcherPriority.Background);
		}
		#endregion


		
		#region 트윗 전송 등 전송 기능
		#region 트윗 전송
		public void SendMedia(ClientSendTweet tweet)
		{
			if (string.IsNullOrEmpty(tweet.multiPath))
			{
				BitmapImage image = tweet.GetNextImage();
				JpegBitmapEncoder encoder = new JpegBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(image));
				PacketMediaUpload packet = new PacketMediaUpload();
				encoder.Save(packet.mediaStream);

				packet.extension = encoder.CodecInfo.FileExtensions;
				packet.eresponse = eResponse.IMAGE;

				RequestPacket(packet, tweet, responseInstence.Multimedia);
			}
			else
			{
				PacketMediaUpload packet = new PacketMediaUpload();
				using (var stream = File.OpenRead(tweet.multiPath))
					stream.CopyTo(packet.mediaStream);

				packet.extension = Path.GetExtension(tweet.multiPath);
				packet.eresponse = eResponse.IMAGE;
				RequestPacket(packet, tweet, responseInstence.Multimedia);
			}
		}

		public void SendTweet(ClientSendTweet tweet)
		{
			RequestPacket<ClientTweet>(tweet.parameter, responseInstence.Tweet);
		}

		public void DeleteTweet(long id)
		{
			PacketTweetDelete parameter = new PacketTweetDelete(id);
			RequestPacket<ClientTweet>(parameter, responseInstence.Delete);
		}
		#endregion

		#region 관글 리트윗
		public void Favorite(long id, bool isFavorite)
		{
			if(isFavorite)
			{
				PacketFavorites_Create packet = new PacketFavorites_Create();
				packet.id = id;
				RequestPacket<ClientTweet>(packet, responseInstence.Favorite);
			}
			else
			{
				PacketFavorites_Destroy packet = new PacketFavorites_Destroy();
				packet.id = id;
				RequestPacket<ClientTweet>(packet, responseInstence.UnFavorite);
			}
		}

		public void Retweet(long id, bool isRetweet)
		{
			if(isRetweet)
			{
				PacketRetweet packet = new API.PacketRetweet(id);
				RequestPacket<ClientTweet>(packet, responseInstence.Retweet);
			}
			else
			{
				PacketUnRetweet packet = new PacketUnRetweet(id);
				RequestPacket<ClientTweet>(packet, responseInstence.UnRetweet);
			}
		}

		#endregion

		#endregion

		#region 트윗 목록 요청
		public void LoadTweetList(eTweetPanel panel, long sinceID=-1, string userid= "")
		{
			BasePacket packet = null;
			Action<List<ClientTweet>> callback = null;

			if(panel== eTweetPanel.eHome)
			{
				PacketHomeTimeLine pack = new PacketHomeTimeLine();
				pack.count = 200;
				if (sinceID != -1)
					pack.since_id = sinceID;
				packet = pack;
				callback = responseInstence.Home;
			}
			else if(panel== eTweetPanel.eMention)
			{
				PacketMentionTimeLine pack = new PacketMentionTimeLine();
				if (sinceID != -1)
					pack.since_id = sinceID;
				packet = pack;
				callback = responseInstence.Mention;
			}
			else if(panel== eTweetPanel.eFavorite)
			{
				PacketFavoritesList pack = new PacketFavoritesList();
				packet = pack;
				callback = responseInstence.FavoriteTweet;
			}
			else if(panel== eTweetPanel.eUser)
			{
				PacketUserTimeLine pack = new PacketUserTimeLine();
				pack.screen_name = userid;
				packet = pack;
				callback = responseInstence.UserTweet;
			}
			else if (panel == eTweetPanel.eUserMedia)
			{
				PacketUserTimeLine pack = new PacketUserTimeLine();
				pack.screen_name = userid;
				packet = pack;
				pack.count = 200.ToString();
				callback = responseInstence.UserTweetMedia;
			}
			if (packet != null && callback != null)
				RequestPacket(packet, callback);
		}

		public void LoadTweetListMore(eTweetPanel panel, long cursor=-1, string userid="")
		{
			BasePacket packet = null;
			Action<List<ClientTweet>> callback = null;

			if (panel == eTweetPanel.eHome)
			{
				PacketHomeTimeLine pack = new PacketHomeTimeLine();
				pack.max_id = cursor;
				pack.count = 200;
				packet = pack;
				callback = responseInstence.HomeMore;
			}
			else if (panel == eTweetPanel.eMention)
			{
				PacketMentionTimeLine pack = new PacketMentionTimeLine();
				pack.max_id = cursor;
				packet = pack;
				callback = responseInstence.MentionMore;
			}
			else if (panel == eTweetPanel.eFavorite)
			{
				PacketFavoritesList pack = new PacketFavoritesList();
				pack.max_id = cursor;
				packet = pack;
				callback = responseInstence.FavoriteMore;
			}
			else if (panel == eTweetPanel.eUser)
			{
				PacketUserTimeLine pack = new PacketUserTimeLine();
				pack.max_id = cursor;
				pack.screen_name = userid;
				packet = pack;
				callback = responseInstence.UserMore;
			}
			else if (panel == eTweetPanel.eUserMedia)
			{
				PacketUserTimeLine pack = new PacketUserTimeLine();
				pack.count = 200.ToString();
				pack.screen_name = userid;
				packet = pack;
				callback = responseInstence.UserTweetMedia;
			}
			if (packet != null && callback != null)
				RequestPacket(packet, callback);
		}

		public void LoadSingleTweet(ClientTweet tweet, string tweetID)
		{
			PacketSingleTweet packet = new API.PacketSingleTweet(tweetID);
			if (tweet.uiProperty.parentTweet == null)
				RequestSingleTweetPacket<ClientTweet>(packet, tweet.uiProperty, responseInstence.SingleTweet);
			else
				RequestSingleTweetPacket<ClientTweet>(packet, tweet.uiProperty.parentTweet, responseInstence.SingleTweet);
		}
		#endregion




		#region 패킷 송신

		private APICallAgent()
		{
			token = ct.Token;
		}
		CancellationTokenSource ct = new CancellationTokenSource();
		CancellationToken token;

		public void RequestPacket<TRes>(BasePacket parameter, Action<TRes> callback, DispatcherPriority priority = DispatcherPriority.Normal)
		{
			if (OnApiCall != null)
				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, OnApiCall, new object[] { parameter.eresponse });

			Task t = null;
			if (priority == DispatcherPriority.Send)
			{
				t = Task.Factory.StartNew(
					  new Action((() => TaskRequestPacket(parameter, callback))),
					  CancellationToken.None,
					  TaskCreationOptions.None,
					  PriorityScheduler.Highest);
			}
			else if (priority == DispatcherPriority.Background)
			{
				t = Task.Factory.StartNew(
				  new Action((() => TaskRequestPacket(parameter, callback))),
				  CancellationToken.None,
				  TaskCreationOptions.None,
				  PriorityScheduler.Lowest);
			}
			else if (priority == DispatcherPriority.Render)
			{
				t = Task.Factory.StartNew(
				  new Action((() => TaskRequestPacket(parameter, callback))),
				  CancellationToken.None,
				  TaskCreationOptions.None,
				  PriorityScheduler.AboveNormal);
			}
			else
			{
				t = Task.Factory.StartNew(
					  new Action((() => TaskRequestPacket(parameter, callback))),
					  CancellationToken.None,
					  TaskCreationOptions.None,
					  PriorityScheduler.BelowNormal);
			}
			//Task t = Task.Factory.StartNew(new Action((() => TaskRequestPacket<TRes>(parameter, callback))), token);
			t.ContinueWith(TaskComplete);
			listTask.TryAdd(t);
		}

		public void RequestSingleTweetPacket<TRes>(BasePacket packet, UIProperty property, Action<TRes,UIProperty> callback)
		{
			if (OnApiCall != null)
				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, OnApiCall, new object[] { packet.eresponse });

			Task t = Task.Factory.StartNew(new Action((() => TaskRequestPacket<TRes>(packet, property, callback))), token);
			t.ContinueWith(TaskComplete);
			listTask.TryAdd(t);
		}

	

		public void RequestPacket(PacketMediaUpload packet, ClientSendTweet tweet, Action<ClientSendTweet, ClientMultimedia> callback)
		{
			Task t = Task.Factory.StartNew(new Action((() => TaskRequestPacket(packet, tweet, callback))), token);
			t.ContinueWith(TaskComplete);
			listTask.TryAdd(t);
		}

		private void TaskComplete(Task task)
		{
			listTask.TryTake(out task);
		}

		private void TaskRequestPacket<TRes>(BasePacket param, Action<TRes> callback)
		{
			WebInstence.SyncRequest<TRes>(param, callback);
		}

		private void TaskRequestPacket<TRes>(BasePacket packet,UIProperty property, Action<TRes, UIProperty> callback)
		{
			WebInstence.SyncRequest<TRes>(packet, property, callback);
		}

		private void TaskRequestPacket(PacketMediaUpload packet, ClientSendTweet tweet, Action<ClientSendTweet, ClientMultimedia> callback)
		{
			WebInstence.SendMultimedia(packet, tweet, callback);
		}

		private void RequestOAuth(BasePacket packet, Action<ResOAuth> callback)
		{
			Task t = new Task(new Action(() => TaskRequestOAuth(packet, callback)));
			t.Start();
		}
		
		private void TaskRequestOAuth(BasePacket packet, Action<ResOAuth> callback)
		{
			WebInstence.SyncRequestOAuth(packet, callback);
		}
		#endregion
	}

	

	public class PriorityScheduler : TaskScheduler
	{
		public const string DefaultThreadName = "PriorityScheduler";

		public static readonly PriorityScheduler Highest = new PriorityScheduler(ThreadPriority.Highest, 0, "PriorityScheduler (Highest)");
		public static readonly PriorityScheduler AboveNormal = new PriorityScheduler(ThreadPriority.AboveNormal, 0, "PriorityScheduler (AboveNormal)");
		public static readonly PriorityScheduler BelowNormal = new PriorityScheduler(ThreadPriority.BelowNormal, 0, "PriorityScheduler (BelowNormal)");
		public static readonly PriorityScheduler Lowest = new PriorityScheduler(ThreadPriority.Lowest, 0, "PriorityScheduler (Lowest)");

		private readonly string m_threadName;
		private readonly int m_maximumConcurrencyLevel;
		private readonly BlockingCollection<Task> m_tasks = new BlockingCollection<Task>();
		private readonly Lazy<Thread>[] m_threads;

		public PriorityScheduler(ThreadPriority priority, int concurrencyLevel, string threadName)
		{
			this.m_maximumConcurrencyLevel = concurrencyLevel > 0 ? concurrencyLevel : Math.Max(1, Environment.ProcessorCount);

			this.m_threadName = threadName;

			this.m_threads = new Lazy<Thread>[this.m_maximumConcurrencyLevel];
			for (int i = 0; i < this.m_maximumConcurrencyLevel; ++i)
			{
				var ii = i;
				this.m_threads[i] = new Lazy<Thread>(
					() => new Thread(this.TaskWorker)
					{
						IsBackground = true,
						Name = $"{this.m_threadName} - {ii}",
						Priority = priority
					}
					);
			}
		}
		public PriorityScheduler(ThreadPriority priority)
			: this(priority, 0, DefaultThreadName)
		{
		}

		private void TaskWorker()
		{
			foreach (var task in this.m_tasks.GetConsumingEnumerable())
				base.TryExecuteTask(task);
		}

		public override int MaximumConcurrencyLevel
			=> this.m_maximumConcurrencyLevel;

		protected override IEnumerable<Task> GetScheduledTasks()
		{
			return this.m_tasks;
		}

		protected override void QueueTask(Task task)
		{
			this.m_tasks.Add(task);

			if (!this.m_threads[0].IsValueCreated)
				for (int i = 0; i < this.m_maximumConcurrencyLevel; i++)
					this.m_threads[i].Value.Start();
		}

		protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
		{
			return false;
		}

		protected override bool TryDequeue(Task task)
		{
			return false;
			//throw new NotImplementedException();
		}
	}
}
