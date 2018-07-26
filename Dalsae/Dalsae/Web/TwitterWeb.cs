using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static Dalsae.FileManager;
using static Dalsae.DalsaeManager;
using static Dalsae.TwitterWeb;
using static Dalsae.DataManager;
using System.Threading;
using Dalsae.API;
using Dalsae.Web;
using System.Windows;

namespace Dalsae
{
	class TwitterWeb
	{
		private static TwitterWeb instence;
		public static TwitterWeb WebInstence { get { return GetWeb(); } }
		private static TwitterWeb GetWeb()
		{
			if (instence == null)
				instence = new TwitterWeb();
			return instence;
		}
		public delegate void DResponseError(BasePacket packet, string json);
		public event DResponseError OnResponseError = null;

		public delegate void DOAuthError();
		public event DOAuthError OnOAuthError = null;

		public void Test(string json)
		{
		}
		//public void DisconnectingUserStreaming()
		//{
		//	userStream.Disconnecting();
		//	userStream = new UserStream();
		//	DalsaeInstence.ConnectedStreaming(false);
		//}
		//public bool isConnectedUserStreaming() { return userStream.isConnectedStreaming; }

		//public void ConnectUserStream(object obj)
		//{
		//	if (obj == null) return;
		//	userStream.ConnectStreaming(obj as BasePacket);
		//}


		public void SyncRequest<TRes>(BasePacket packet, UIProperty property, Action<TRes, UIProperty> callback)
		{
			HttpWebRequest req;
			if (packet.method == "POST")
				req = (HttpWebRequest)WebRequest.Create(packet.url);
			else//GET일경우
				req = (HttpWebRequest)WebRequest.Create(packet.MethodGetUrl());
			TwitterRequest request = new TwitterRequest(req, packet);

			req.ContentType = "application/x-www-form-urlencoded;encoding=utf-8";
			req.Method = packet.method;
			req.Headers.Add("Authorization", OAuth.GetInstence().GetHeader(packet));



			if (packet.dicParams.Count > 0 && packet.method == "POST")//POST일 때에만 Stream사용
			{
				//-----------------------------------------------------------------------------------
				//------------------------------------Send------------------------------------------
				//-----------------------------------------------------------------------------------
				try//send!
				{
					Send(packet, req);
				}
				catch (WebException e)
				{
					using (Stream stream2 = e.Response?.GetResponseStream())
					{
						StreamReader srReadData = new StreamReader(stream2, Encoding.Default);
						string log = srReadData.ReadToEnd();
						if (OnResponseError != null)
							Application.Current.Dispatcher.BeginInvoke(OnResponseError, new object[] { log });
					}
				}
				catch (Exception e)
				{
					OnResponseError?.Invoke(packet, null);
				}
			}

			try//Response!!!
			{
				string json = Recv(packet, req);
				TRes ret = default(TRes);
				if (json?.Length > 0)
					ret = JsonConvert.DeserializeObject<TRes>(json);
				if (callback != null)
					Application.Current.Dispatcher.BeginInvoke(callback, new object[] { ret, property });
			}
			catch (WebException e)
			{
				if (e.Message.IndexOf("408") > -1)//timeout시 재 전송
				{
					Manager.APICallAgent.apiInstence.RequestSingleTweetPacket<TRes>(packet, property, callback);
				}
				else if (e.Response == null)
				{
					App.SendException(e);
				}
				else
				{
					//ClientAPIError error = null;
					using (Stream stream = e.Response?.GetResponseStream())
					using (StreamReader srReadData = new StreamReader(stream, Encoding.Default))
					{
						string log = srReadData.ReadToEnd();
						if (string.IsNullOrEmpty(log) == false)
							if (OnResponseError != null)
								Application.Current.Dispatcher.BeginInvoke(OnResponseError, new object[] { packet, log });
					}

				}
			}
			catch (Exception e)
			{
				OnResponseError?.Invoke(packet, null);
				App.SendException(e);
			}
		}

		public void SyncRequest<T>(BasePacket packet, Action<T> callback)
		{
			HttpWebRequest req;
			if (packet.method == "POST")
				req = (HttpWebRequest)WebRequest.Create(packet.url);
			else//GET일경우
				req = (HttpWebRequest)WebRequest.Create(packet.MethodGetUrl());
			TwitterRequest request = new TwitterRequest(req, packet);

			req.ContentType = "application/x-www-form-urlencoded;encoding=utf-8";
			req.Method = packet.method;
			req.Headers.Add("Authorization", OAuth.GetInstence().GetHeader(packet));



			if (packet.dicParams.Count > 0 && packet.method == "POST")//POST일 때에만 Stream사용
			{
				//-----------------------------------------------------------------------------------
				//------------------------------------Send------------------------------------------
				//-----------------------------------------------------------------------------------
				try//send!
				{
					Send(packet, req);
				}
				catch (WebException e)
				{
					using (Stream stream2 = e.Response?.GetResponseStream())
					{
						StreamReader srReadData = new StreamReader(stream2, Encoding.Default);
						string log = srReadData.ReadToEnd();
						if (OnResponseError != null)
							Application.Current.Dispatcher.BeginInvoke(OnResponseError, new object[] { log });
					}
				}
				catch (Exception e)
				{
					OnResponseError?.Invoke(packet, null);
				}
			}

			try//Response!!!
			{
				string json = Recv(packet, req);

				T ret = JsonConvert.DeserializeObject<T>(json);
				callback?.Invoke(ret);
			}
			catch (WebException e)
			{
				if (e.Message.IndexOf("408") > -1)//timeout시 재 전송
				{
					Manager.APICallAgent.apiInstence.RequestPacket<T>(packet, callback);
				}
				else if (e.Response == null)
				{
					App.SendException(e);
				}
				else
				{
					//ClientAPIError error = null;
					using (Stream stream = e.Response?.GetResponseStream())
					using (StreamReader srReadData = new StreamReader(stream, Encoding.Default))
					{
						string log = srReadData.ReadToEnd();
						if (string.IsNullOrEmpty(log) == false)
							if (OnResponseError != null)
								Application.Current.Dispatcher.BeginInvoke(OnResponseError, new object[] { packet, log });
					}
					
				}
			}
			catch (Exception e)
			{
				OnResponseError?.Invoke(packet, null);
				App.SendException(e);
			}

		}

		private string Recv(BasePacket packet, HttpWebRequest req)
		{
			using (WebResponse response = req.GetResponse())
			using (Stream stream = response.GetResponseStream())
			using (StreamReader streamRead = new StreamReader(stream))
				return streamRead.ReadToEnd();
		}

		private void Send(BasePacket packet, HttpWebRequest req)
		{
			Stream stream = req.GetRequestStream();
			StringBuilder sb = new StringBuilder();

			foreach (string item in packet.dicParams.Keys)
			{
				if (packet.dicParams[item] != "")
				{
					sb.Append(item);
					sb.Append("=");
					OAuth.GetInstence().CalcParamUri(sb, packet.dicParams[item]);
					sb.Append("&");
				}
			}
			string sendData = sb.ToString();
			byte[] bytes = Encoding.UTF8.GetBytes(sendData);

			stream.Write(bytes, 0, sendData.Length);
			stream.Close();
		}

		public void SendMultimedia(PacketMediaUpload packet, ClientSendTweet tweet, Action<ClientSendTweet, ClientMultimedia> callback)
		{
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(packet.url);
			TwitterRequest request = new TwitterRequest(req, packet);

			var boundary = Guid.NewGuid().ToString().Replace("-", string.Empty);

			req.ContentType = "multipart/form-data;encoding=utf-8;boundary=" + boundary;
			req.Method = packet.method;
			req.Headers.Add("Authorization", OAuth.GetInstence().GetHeader(packet));

			boundary = "--" + boundary;

			try//send!
			{
				using (Stream stream = req.GetRequestStream())
				using (var writer = new StreamWriter(stream, Encoding.UTF8))
				{
					writer.NewLine = "\r\n";
					writer.WriteLine(boundary);
					writer.WriteLine("Content-Type: application/octet-stream");
					writer.WriteLine($"Content-Disposition: form-data; name=\"media\"; filename=\"img{packet.extension}\"");
					writer.WriteLine();
					writer.Flush();

					packet.mediaStream.Position = 0;
					packet.mediaStream.CopyTo(stream);

					writer.WriteLine();
					writer.WriteLine(boundary + "--");
					writer.Flush();
				}
			}
			catch (WebException e)
			{
				using (Stream stream = e.Response?.GetResponseStream())
				{
					if (stream == null) return;
					using (StreamReader srReadData = new StreamReader(stream, Encoding.Default))
					{
						string log = srReadData.ReadToEnd();
						if (OnResponseError != null)
							Application.Current.Dispatcher.BeginInvoke(OnResponseError, new object[] { packet, log });
					}
				}
			}
			catch(Exception e)
			{
				packet.Dispose();
				App.SendException(e);
				return;
			}

			try//Response!!!
			{
				using (WebResponse response = req.GetResponse())
				{
					using (Stream stream = response.GetResponseStream())
					{
						if (stream == null) return;
						using (StreamReader streamRead = new StreamReader(stream))
						{
							string json = streamRead.ReadToEnd();
							ClientMultimedia media = JsonConvert.DeserializeObject<ClientMultimedia>(json);
							callback?.Invoke(tweet, media);
						}
					}
				}
			}
			catch (WebException e)
			{
				using (Stream stream = e.Response?.GetResponseStream())
				{
					if (stream == null) return;
					using (StreamReader srReadData = new StreamReader(stream, Encoding.Default))
					{
						string log = srReadData.ReadToEnd();
						if (OnResponseError != null)
							Application.Current.Dispatcher.BeginInvoke(OnResponseError, new object[] { log });
					}
				}
			}
			catch(Exception e)
			{
				App.SendException(e);
			}
			finally
			{
				packet.Dispose();
			}
		}



		public void SyncRequestOAuth(BasePacket packet, Action<ResOAuth> callback)
		{
			if (packet == null) return;
			try
			{
				HttpWebRequest req = (HttpWebRequest)WebRequest.Create(packet.url);
				req.ContentType = "application/x-www-form-urlencoded;encoding=utf-8";
				req.Method = packet.method;
				req.Headers.Add("Authorization", OAuth.GetInstence().GetHeader(packet));

				using (WebResponse response = req.GetResponse())
				using (Stream stream = response.GetResponseStream())
				using (StreamReader streamRead = new StreamReader(stream))
				{
					ResOAuth oauth = new ResOAuth();
					string responseString = streamRead.ReadToEnd();
					oauth.tokenStr = Regex.Match(responseString, @"oauth_token=([^&]+)").Groups[1].Value;//json으로 오는 게 아니라 이렇게 해야함
					oauth.secretStr = Regex.Match(responseString, @"oauth_token_secret=([^&]+)").Groups[1].Value;
					bool isCallBack = false;
					bool.TryParse(Regex.Match(responseString, @"oauth_callback_confirmed=([^&]+)").Groups[1].Value, out isCallBack);
					oauth.isCallBack = isCallBack;
					callback?.Invoke(oauth);
				}
			}
			catch (WebException e)
			{
				if (e.Message.IndexOf("401") > -1)
				{
					if (OnOAuthError != null)
						Application.Current.Dispatcher.BeginInvoke(OnOAuthError);
				}
			}
			catch (Exception e)
			{

			}
		}



		#region 오래된 코드


		//각종 API요청용 함수
		public void RequestTwitter(BasePacket parameter)
		{
			if (parameter == null) return;

			HttpWebRequest req;
			if (parameter.method == "POST")
				req = (HttpWebRequest)WebRequest.Create(parameter.url);
			else//GET일 경우
				req = (HttpWebRequest)WebRequest.Create(parameter.MethodGetUrl());

			req.ContentType= "application/x-www-form-urlencoded;encoding=utf-8";
			req.Method = parameter.method;
			req.Headers.Add("Authorization", OAuth.GetInstence().GetHeader(parameter));

			try
			{
				if (parameter.dicParams.Count > 0 && parameter.method=="POST")//POST일 때에만 Stream사용
				{
					TwitterRequest twitterRequest = new TwitterRequest(req, parameter);
					req.BeginGetRequestStream(new AsyncCallback(AsyncRequest), twitterRequest);
				}
				else
				{
					TwitterRequest twitterRequest = new TwitterRequest(req, parameter);
					req.BeginGetResponse(new AsyncCallback(AsyncResponse), twitterRequest);
				}
			}
			catch(WebException e)
			{
				using (Stream stream = e.Response?.GetResponseStream())
				{
					if (stream == null) return;
					StreamReader srReadData = new StreamReader(stream, Encoding.Default);
					string log = srReadData.ReadToEnd();
					if (OnResponseError != null)
						Application.Current.Dispatcher.BeginInvoke(OnResponseError, new object[] { log });
				}
			}
			catch (TimeoutException time)
			{
				//DalsaeInstence.ResponseTimeoutException(parameter);
			}
			catch(Exception e)
			{

			}
		}

		private void AsyncRequest(IAsyncResult ar)
		{
			TwitterRequest req = (TwitterRequest)ar.AsyncState;
			try
			{
				using (Stream stream = req.request.EndGetRequestStream(ar))
				{
					StringBuilder sb = new StringBuilder();

					foreach (string item in req.parameter.dicParams.Keys)
					{
						if (req.parameter.dicParams[item] != "")
						{
							sb.Append(item);
							sb.Append("=");
							OAuth.GetInstence().CalcParamUri(sb, req.parameter.dicParams[item]);
							sb.Append("&");
							//sb.Append(Uri.EscapeDataString(req.parameter.dicParams[item]));
						}
					}
					string sendData = sb.ToString();
					byte[] bytes = Encoding.UTF8.GetBytes(sendData);

					// Write to the request stream.
					stream.Write(bytes, 0, sendData.Length);
					//stream.Close();
				}
			}
			catch(WebException we)
			{

			}
			catch(TimeoutException time)
			{
				//DalsaeInstence.ResponseTimeoutException(req.parameter);
			}
			req.request.BeginGetResponse(new AsyncCallback(AsyncResponse), req);
		}

		private void AsyncResponse(IAsyncResult ar)
		{
			TwitterRequest req = (TwitterRequest)ar.AsyncState;
			//HttpWebRequest request = (HttpWebRequest)ar.AsyncState;

			try
			{
				HttpWebResponse response = (HttpWebResponse)req.request.EndGetResponse(ar);
				Stream stream = response.GetResponseStream();
				StreamReader streamRead = new StreamReader(stream);
				string responseString = streamRead.ReadToEnd();

				stream.Close();
				streamRead.Close();
				response.Close();

				//DalsaeInstence.ResponseJson(responseString, req.parameter.eresponse, req.parameter.isMore);
			}
			catch (WebException e)
			{
				using (Stream stream = e.Response?.GetResponseStream())
				{
					if (stream == null) return;
					StreamReader srReadData = new StreamReader(stream, Encoding.Default);
					string log = srReadData.ReadToEnd();
					if (OnResponseError != null)
						Application.Current.Dispatcher.BeginInvoke(OnResponseError, new object[] { log });
				}
			}
			catch(Exception e)
			{

			}
		}

		
		//이미지 업로드 시 요청하는 함수, 동기 전송
		//obj: 이미지 bytes를 담고있는 파라메터
		public string SendMultimedia2(PacketMediaUpload parameter)
		{
			string ret = string.Empty;
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(parameter.url);
			TwitterRequest request = new TwitterRequest(req, parameter);
			
            var boundary = Guid.NewGuid().ToString().Replace("-", string.Empty);

            req.ContentType = "multipart/form-data;encoding=utf-8;boundary=" + boundary;
			req.Method = parameter.method;
			req.Headers.Add("Authorization", OAuth.GetInstence().GetHeader(parameter));
            
            boundary = "--" + boundary;

			//-----------------------------------------------------------------------------------
			//------------------------------------Send------------------------------------------
			//-----------------------------------------------------------------------------------
			try//send!
			{
                using (Stream stream = req.GetRequestStream())
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.NewLine = "\r\n";
                    writer.WriteLine(boundary);
                    writer.WriteLine("Content-Type: application/octet-stream");
                    writer.WriteLine($"Content-Disposition: form-data; name=\"media\"; filename=\"img{parameter.extension}\"");
                    writer.WriteLine();
                    writer.Flush();

                    parameter.mediaStream.Position = 0;
                    parameter.mediaStream.CopyTo(stream);

                    writer.WriteLine();
                    writer.WriteLine(boundary + "--");
                    writer.Flush();
                }
			}
			catch(WebException e)
			{
				using (Stream stream = e.Response?.GetResponseStream())
				{
					if (stream == null) return string.Empty;
					using (StreamReader srReadData = new StreamReader(stream, Encoding.Default))
					{
						string log = srReadData.ReadToEnd();
						if (OnResponseError != null)
							Application.Current.Dispatcher.BeginInvoke(OnResponseError, new object[] { log });
					}
				}
			}
			//-----------------------------------------------------------------------------------
			//-------------------------------Response------------------------------------------
			//-----------------------------------------------------------------------------------

			try//Response!!!
			{
				using (WebResponse response = req.GetResponse())
				{
					using (Stream stream = response.GetResponseStream())
					{
						using (StreamReader streamRead = new StreamReader(stream))
							ret = streamRead.ReadToEnd();
					}
				}
			}
			catch (WebException e)
			{
				using (Stream stream = e.Response?.GetResponseStream())
				{
					if (stream == null) return string.Empty;
					using (StreamReader srReadData = new StreamReader(stream, Encoding.Default))
					{
						string log = srReadData.ReadToEnd();
						if (OnResponseError != null)
							Application.Current.Dispatcher.BeginInvoke(OnResponseError, new object[] { log });
					}
				}
			}

			return ret;
		}


		public string SyncRequest(BasePacket parameter)
		{
			string ret = string.Empty;
			HttpWebRequest req;
			if (parameter.method == "POST")
				req = (HttpWebRequest)WebRequest.Create(parameter.url);
			else//GET일경우
				req = (HttpWebRequest)WebRequest.Create(parameter.MethodGetUrl());
			TwitterRequest request = new TwitterRequest(req, parameter);

			req.ContentType = "application/x-www-form-urlencoded;encoding=utf-8";
			req.Method = parameter.method;
			req.Headers.Add("Authorization", OAuth.GetInstence().GetHeader(parameter));

			

			if (parameter.dicParams.Count > 0 && parameter.method == "POST")//POST일 때에만 Stream사용
			{
				//-----------------------------------------------------------------------------------
				//------------------------------------Send------------------------------------------
				//-----------------------------------------------------------------------------------
				try//send!
				{
					Stream stream = req.GetRequestStream();
					StringBuilder sb = new StringBuilder();

					foreach (string item in parameter.dicParams.Keys)
					{
						if (parameter.dicParams[item] != "")
						{
							sb.Append(item);
							sb.Append("=");
							OAuth.GetInstence().CalcParamUri(sb, parameter.dicParams[item]);
							sb.Append("&");
						}
					}
					string sendData = sb.ToString();
					byte[] bytes = Encoding.UTF8.GetBytes(sendData);

					stream.Write(bytes, 0, sendData.Length);
					stream.Close();
				}
				catch (WebException e)
				{
					using (Stream stream2 = e.Response?.GetResponseStream())
					{
						if (stream2 == null) return string.Empty;
						StreamReader srReadData = new StreamReader(stream2, Encoding.Default);
						string log = srReadData.ReadToEnd();
						if (OnResponseError != null)
							Application.Current.Dispatcher.BeginInvoke(OnResponseError, new object[] { log });
					}
				}
				catch (Exception e)
				{
					App.SendException(e);
				}
			}
			
			//-----------------------------------------------------------------------------------
			//-------------------------------Response------------------------------------------
			//-----------------------------------------------------------------------------------

			try//Response!!!
			{
				using (WebResponse response = req.GetResponse())
					using (Stream stream = response.GetResponseStream())
						using (StreamReader streamRead = new StreamReader(stream))
							ret = streamRead.ReadToEnd();
			}
			catch (WebException e)
			{
				//using (Stream stream = e.Response.GetResponseStream())
				//{
				//	StreamReader srReadData = new StreamReader(stream, Encoding.Default);
				//	string log = srReadData.ReadToEnd();
				//	DalsaeInstence.ResponseError(log);
				//}
			}
			catch(Exception e)
			{
				App.SendException(e);
			}

			return ret;
		}


		////OAuth, AccessToken 발급용 외부 호출 함수
		////parameter: BaseParameter를 상속받은 oauth, token용 parameter
		//public void RequestOAuth(BasePacket parameter)
		//{
		//	if (parameter == null) return;

		//	HttpWebRequest req = (HttpWebRequest)WebRequest.Create(parameter.url);
		//	req.ContentType = "application/x-www-form-urlencoded;encoding=utf-8";
		//	req.Method = parameter.method;
		//	req.Headers.Add("Authorization", OAuth.GetInstence().GetHeader(parameter));

		//	req.BeginGetResponse(new AsyncCallback(AsyncResponseOAuth), req);
		//}

		////비동기, OAuth, AccessToken발급용 함수
		////ar: HttpWebRequest
		//private void AsyncResponseOAuth(IAsyncResult ar)
		//{
		//	try
		//	{
		//		HttpWebRequest request = (HttpWebRequest)ar.AsyncState;

		//		HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
		//		Stream stream = response.GetResponseStream();
		//		StreamReader streamRead = new StreamReader(stream);
		//		string responseString = streamRead.ReadToEnd();

		//		string tokenStr = Regex.Match(responseString, @"oauth_token=([^&]+)").Groups[1].Value;//json으로 오는 게 아니라 이렇게 해야함
		//		string secretStr = Regex.Match(responseString, @"oauth_token_secret=([^&]+)").Groups[1].Value;
		//		bool isCallBack = false;
		//		bool.TryParse(Regex.Match(responseString, @"oauth_callback_confirmed=([^&]+)").Groups[1].Value, out isCallBack);
		//		//AccessToken 발급 시 user_id, screen_name, x_auth_expires(?) 옴. 현재는 사용x


		//		DalsaeInstence.UpdateToken(tokenStr, secretStr, !isCallBack);
		//		//TokenAndKey.SetUserToken(tokenStr);
		//		//TokenAndKey.SetUserTokenSecret(secretStr);

		//		if (isCallBack)//pin발급 시
		//			System.Diagnostics.Process.Start("https://api.twitter.com/oauth/authorize?oauth_token=" + tokenStr);

		//		stream.Close();
		//		streamRead.Close();
		//		response.Close();
		//	}
		//	catch(WebException e)
		//	{
		//		if (e.Message.IndexOf("401") > -1)
		//		{
		//			DalsaeInstence.InputErrorPin();
		//		}
		//	}
		//}
	}
	#endregion

	#region 유저스트림
	
	#endregion
	//비동기용 클래스
	class TwitterRequest
	{
		public TwitterRequest(HttpWebRequest req, BasePacket parameter)
		{
			this.parameter = parameter;
			this.request = req;
		}
		public BasePacket parameter { get; private set; }
		public HttpWebRequest request { get; private set; }
	}
}
