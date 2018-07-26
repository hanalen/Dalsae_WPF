using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;

namespace Dalsae
{
	public partial class DalsaeManager
	{
		GitHubRelease patch = null;
		public void CheckNewVersion()
		{
			patch = LoadGitHubRelease();
			if (patch != null)
			{
				//어셈블리 정보 가져옴
				Assembly assembly = Assembly.GetExecutingAssembly();
				FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);

				float newVersion = 0.0f, prevVersion = 0.0f;
				//버전 파싱, 파싱 오류 시 그냥 실행 작업 진행
				if (float.TryParse(fvi.FileVersion, out prevVersion) && float.TryParse(patch.tag_name, out newVersion))
				{
					if (newVersion > prevVersion)
					{
						ShowMessageBox($"달새의 새로운 버전인 {patch.tag_name}v가 나왔습니다. 패치를 진행하시겠습니까?",
							"패치 알림", PatchProccess);
					}
				}
			}
		}

		//패치 여부를 묻고 패치 진행
		private void PatchProccess(MessageBoxResult mbr)
		{
			if (mbr == MessageBoxResult.Yes)
			{
				for (int i = 0; i < patch.Assets.Length; i++)
				{
					if (string.Equals(patch.Assets[i].name, "Dalsae_Patch.exe", StringComparison.OrdinalIgnoreCase))
					{
						if (DownloadPatchFile(patch.Assets[i]))
						{
							string path = $"{Directory.GetCurrentDirectory()}/{patch.Assets[i].name}";
							if (Process.Start(path) != null)
								Application.Current.Shutdown();
						}
					}
				}
			}
		}
		
		private bool DownloadPatchFile(Asset asset)
		{
			bool ret = false;
			try
			{
				WebClient myWebClient = new WebClient();
				myWebClient.DownloadFile(asset.browser_download_url, asset.name);
				ret = true;
			}
			catch (Exception e) { ret = false; }
			return ret;
		}

		private GitHubRelease LoadGitHubRelease()
		{
			//인터넷 연결 확인
			GitHubRelease ret = null;
			try
			{
				string json = string.Empty;
				HttpWebRequest req = WebRequest.Create("https://api.github.com/repos/hanalen-/Dalsae_WPF/releases/latest") as HttpWebRequest;
				ServicePointManager.Expect100Continue = true;
				ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
				req.Timeout = 5000;
				req.UserAgent = "Dalsae_WPF";
				using (WebResponse res = req.GetResponse())
				using (Stream stream = res.GetResponseStream())
				using (StreamReader streamRead = new StreamReader(stream))
					json = streamRead.ReadToEnd();
				if (json != string.Empty)
					ret = JsonConvert.DeserializeObject<GitHubRelease>(json);
			}
			catch (Exception e) { App.SendException(e); }
			catch { }
			return ret;
		}

		private class GitHubRelease//API전체
		{
			public string tag_name { get; set; }//버전
			public string name { get; set; }//패치 글 이름
			public string html_url { get; set; }//해당 패치 페이지
			public Asset[] Assets { get; set; }

		}

		private class Asset//업로드 파일 정보
		{
			public string name { get; set; }//파일 이름
			public string browser_download_url { get; set; }//파일 주소
		}
	}
}
