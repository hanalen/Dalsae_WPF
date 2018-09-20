using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Dalsae.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dalsae.Web
{
	class UserStreaming
	{
		private static UserStreaming _instence;
		public static UserStreaming usInstence { get { if (_instence == null) _instence = new UserStreaming(); return _instence; } }
		private UserStreaming() { token = ct.Token; }
		private StreamReader streamRead;
		private Stream stream;
		public bool isConnectedStreaming { get; private set; } = false;

		private CancellationTokenSource ct = new CancellationTokenSource();
		private CancellationToken token;

		public delegate void DUserstreamChangedStatus(bool isConnected);
		public event DUserstreamChangedStatus OnChangedStatus = null;
		public delegate void DTweet(ClientTweet tweet);
		public event DTweet OnTweet = null;
		public delegate void DDelete(ClientStreamDelete delete);
		public event DDelete OnDelete = null;
		public delegate void DDM(StreamDirectMessage dm);
		public event DDM OnDM = null;
		public delegate void DEvent(StreamEvent streamEvent);
		public event DEvent OnEvent = null;

		public void ConnectUserStreaming()
		{
			PacketUserStream parameter = new PacketUserStream();
			Task t = Task.Factory.StartNew(new Action((() => SyncStreaming(parameter))), token);
			t.ContinueWith(TaskComplete);
		}

		private void TaskComplete(Task obj)
		{

		}

		public void DisconnectStreaming()
		{
			return;
			streamRead.Dispose();
			ct.Cancel();
			ct = new CancellationTokenSource();
			token = ct.Token;
		}

		private async void SyncStreaming(BasePacket parameter)
		{
			while (true)
			{
				HttpWebRequest req;
				if (parameter.method == "POST")
					req = (HttpWebRequest)WebRequest.Create(parameter.url);
				else//GET일 경우
					req = (HttpWebRequest)WebRequest.Create(parameter.MethodGetUrl());

				req.ContentType = "application/x-www-form-urlencoded;encoding=utf-8";
				req.Method = parameter.method;
				req.Headers.Add("Authorization", OAuth.GetInstence().GetHeader(parameter));
				try
				{
					using (WebResponse response = req.GetResponse())
					using (stream = response.GetResponseStream())
					using (streamRead = new StreamReader(stream))
					{
						string json;
						if (OnChangedStatus != null)
							Application.Current.Dispatcher.BeginInvoke(OnChangedStatus, new object[] { true });
						while ((json = streamRead.ReadLine()) != null)
						{
							if (string.IsNullOrWhiteSpace(json)) continue;
							ResponseJson(ref json);
						}
					}
				}
				catch (WebException e)
				{
					App.SendException(e);
					await Application.Current.Dispatcher.BeginInvoke(OnChangedStatus, new object[] { false });
					//OnChangedStatus?.Invoke(false);
				}
				catch (Exception e)
				{
					App.SendException(e);
					await Application.Current.Dispatcher.BeginInvoke(OnChangedStatus, new object[] { false });
					//OnChangedStatus?.Invoke(false);
				}
				finally
				{
					await Application.Current.Dispatcher.BeginInvoke(OnChangedStatus, new object[] { false });
					//OnChangedStatus?.Invoke(false);
					await Task.Delay(TimeSpan.FromSeconds(10));
				}
			}
		}

		private void ResponseJson(ref string json)
		{
			//try
			//{
			//	Friends friends = JsonConvert.DeserializeObject<Friends>(json);
			//	return;
			//}
			//catch (Exception e) { }
			try
			{
				ClientTweet tweet = JsonConvert.DeserializeObject<ClientTweet>(json);
				if (tweet?.created_at != null && tweet.dateTime != DateTime.MinValue)
				{
					if (OnTweet != null)
						Application.Current.Dispatcher.BeginInvoke(OnTweet, new object[] { tweet });
					//OnTweet?.Invoke(tweet);
					return;
				}
			}
			catch (Exception e) { }
			
			try
			{
				ClientStreamDelete delete = JsonConvert.DeserializeObject<ClientStreamDelete>(json);
				if (delete?.delete != null)
				{
					if (OnDelete != null)
						Application.Current.Dispatcher.BeginInvoke(OnDelete, new object[] { delete });
					//OnDelete?.Invoke(delete);
					return;
				}
			}
			catch (Exception e) { }

			try
			{
				StreamDirectMessage dm = JsonConvert.DeserializeObject<StreamDirectMessage>(json);
				if (dm?.direct_message != null)
				{
					if (OnDM != null)
						Application.Current.Dispatcher.BeginInvoke(OnDM, new object[] { dm });
					//OnDM?.Invoke(dm);
					return;
				}
			}
			catch (Exception e) { }

			try
			{
				StreamEvent streamEvent = JsonConvert.DeserializeObject<StreamEvent>(json);
				if (streamEvent?.Event != null)
				{
					if (OnEvent != null)
						Application.Current.Dispatcher.BeginInvoke(OnEvent, new object[] { streamEvent });
					//OnEvent?.Invoke(streamEvent);
					return;
				}
			}
			catch (Exception e) { }
		}
	}
}
