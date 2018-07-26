using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dalsae.API;
using static Dalsae.DataManager;

namespace Dalsae
{
	public enum eOAuth
	{
		OAUTH,

	}
	class OAuth
	{
		private static OAuth instence;

		private Dictionary<string, string> dicParameter = new Dictionary<string, string>();
		private static readonly object lockObject = new object();
		public static OAuth GetInstence()
		{
			if (instence == null) { instence = new Dalsae.OAuth(); }

			return instence;
		}

		//public string OAuthCallback//인증 후 이동할 페이지 주소, 필요없음
		//{
		//	get { return dicParameter["oauth_callback"]; }
		//	set { dicParameter["oauth_callback"] = value; }
		//}

		public string OAuthConsumerKey//포함
		{
			get { return dicParameter["oauth_consumer_key"]; }
			set { dicParameter["oauth_consumer_key"] = value; }
		}

		public string OAuthConsumerSecret//zMhrROBYpxzSNgaurwZ1sJl5XRLPM2VtK8xBW0YMZP69L7uXP8
		{
			get { return dicParameter["oauth_consumer_secret"]; }
			set { dicParameter["oauth_consumer_secret"] = value; }
		}

		public string OAuthTimestamp//포함
		{
			get { return dicParameter["oauth_timestamp"]; }
			set { dicParameter["oauth_timestamp"] = value; }
		}

		public string OAuthNonce//포함
		{
			get { return dicParameter["oauth_nonce"]; }
			set { dicParameter["oauth_nonce"] = value; }
		}

		public string OAuthSignatureMethod//포함
		{
			get { return dicParameter["oauth_signature_method"]; }
			set { dicParameter["oauth_signature_method"] = value; }
		}

		public string OAuthSignature//hash낸 값...?
		{
			get { return dicParameter["oauth_signature"]; }
			set { dicParameter["oauth_signature"] = value; }
		}

		public string OAuthToken//포함
		{
			get { return dicParameter["oauth_token"]; }
			set { dicParameter["oauth_token"] = value; }
		}

		//public string OAuthTokenSecret//R95wV4KMOEHFvgrnGRfob01FAxFefOHbSgkQX4IJBRttY
		//{
		//	get { return dicParameter["oauth_token_secret"]; }
		//	set { dicParameter["oauth_token_secret"] = value; }
		//}

		public string OAuthVersion//포함
		{
			get { return dicParameter["oauth_version"]; }
			set { dicParameter["oauth_version"] = value; }
		}

		//public string OAuthVerifier
		//{
		//	get { return dicParameter["oauth_verifier"]; }
		//	set { dicParameter["oauth_verifier"] = value; }
		//}

		public OAuth()
		{
			//this.OAuthCallback = "oob";
			this.OAuthConsumerKey = Data.APIKeys.ConsumerKey;
			this.OAuthConsumerSecret = Data.APIKeys.ConsumerSecret;
			//this.OAuthTimestamp = Generate.GetTimestamp();
			//this.OAuthNonce = Generate.GetNonce();
			this.OAuthSignatureMethod = "HMAC-SHA1";//트위터에서 사용하는 메소드 이름
			this.OAuthSignature = "";
			//this.OAuthTokenSecret = Generate.TokenSecret;
			//this.OAuthToken = Generate.Token;
			this.OAuthVersion = "1.0";
			//this.OAuthVerifier = "";
		}

		public string GetHeader(BasePacket parameter)
		{
			lock(lockObject)
			{
				Refresh();

				this.OAuthSignature = CalcSignature(parameter);

				StringBuilder sb = new StringBuilder();
				sb.Append("OAuth ");
				foreach (string item in dicParameter.Keys)
				{
					if (!string.IsNullOrEmpty(dicParameter[item]) && !item.EndsWith("_secret")
						/*&& !item.EndsWith("_verifier") && !item.EndsWith("_callback")*/)
					{
						sb.Append(item);
						sb.Append("=\"");
						CalcParamUri(sb, dicParameter[item]);
						//sb.Append(Uri.EscapeDataString(dicParameter[item]));
						sb.Append("\", ");
					}
				}
				foreach (string item in parameter.dicParams.Keys)
				{
					if (!string.IsNullOrEmpty(parameter.dicParams[item]))
					{
						sb.Append(item);
						sb.Append("=\"");
						CalcParamUri(sb, parameter.dicParams[item]);
						//sb.Append(Uri.EscapeDataString(parameter.dicParams[item]));
						sb.Append("\", ");
					}
				}
				sb.Remove(sb.Length - 2, 2);

				return sb.ToString();
			}
		}

		private string CalcSignature(BasePacket parameter)
		{
			SortedDictionary<string, string> dicSorted = new SortedDictionary<string, string>();
			
			foreach(string item in dicParameter.Keys)
			{
				if (!string.IsNullOrEmpty(dicParameter[item]) && !item.EndsWith("_secret") && !item.EndsWith("_signature")
					/*&& !item.EndsWith("_verifier") && !item.EndsWith("_callback")*/)
					dicSorted[item] = dicParameter[item];
			}
			foreach(string item in parameter.dicParams.Keys)
			{
				if (!string.IsNullOrEmpty(parameter.dicParams[item]) && !item.EndsWith("_secret") && !item.EndsWith("_signature"))
					dicSorted[item] = parameter.dicParams[item];
			}

			StringBuilder sb = new StringBuilder();
			foreach(string item in dicSorted.Keys)
			{
				sb.Append(item);
				sb.Append("=");
				CalcParamUri(sb, dicSorted[item]);
				sb.Append("&");
			}
			sb.Remove(sb.Length - 1, 1);//마지막 & 지우기

			string baseStr = CalcBaseString(parameter.method, parameter.url, sb.ToString());
			string signKey = GetSignKey();//Generate.ConsumerSecret + "&" + Generate.TokenSecret;//이게 문제
			string ret = string.Empty;
			using (HMACSHA1 sha = new HMACSHA1(Encoding.ASCII.GetBytes(signKey)))
			{
				byte[] byteArray = Encoding.ASCII.GetBytes(baseStr);
				//MemoryStream stream = new MemoryStream(byteArray);//버그?
				byte[] hashvalue = sha.ComputeHash(byteArray);
				ret = Convert.ToBase64String(hashvalue);
				sha.Dispose();
			}
			return ret;
		}

		public void Clear()
		{
			lock(lockObject)
			{
				OAuthToken = string.Empty;
			}
		}

		public void CalcParamUri(StringBuilder sb, string text)
		{
			int limit = 100;

			if (text.Length > limit)//media등은 길어서 나눠서 해야됨
			{
				int loops = text.Length / limit;


				for (int i = 0; i <= loops; i++)
				{
					if (i < loops)
					{
						sb.Append(Uri.EscapeDataString(text.Substring(100 * i, limit)));
					}
					else
					{
						sb.Append(Uri.EscapeDataString(text.Substring(limit * i)));
					}
				}
			}
			else
			{
				sb.Append(Uri.EscapeDataString(text));
			}
		}

		private string GetSignKey()
		{
			return string.Format("{0}&{1}", Data.APIKeys.ConsumerSecret,
												DataInstence.userInfo.TokenSecret);
		}

		private string CalcBaseString(string method, string url, string paramStr)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(method);
			sb.Append("&");
			CalcParamUri(sb, url);
			sb.Append("&");
			CalcParamUri(sb, paramStr);

			return sb.ToString();
		}

		private void Refresh()
		{
			this.OAuthTimestamp = GetTimestamp();
			this.OAuthNonce = GetNonce();
		}

		private string GetTimestamp()
		{
			TimeSpan span = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToInt64(span.TotalSeconds).ToString();
			//DateTime GenerateTimeStampDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
			//return Convert.ToInt64((DateTime.UtcNow - GenerateTimeStampDateTime).TotalSeconds).ToString();
		}

		private string GetNonce()
		{
			return Convert.ToBase64String(new ASCIIEncoding().GetBytes(DateTime.Now.Ticks.ToString()));
		}
	}//class End

	
}
