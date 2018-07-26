using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Dalsae_Patch
{
	class Program
	{
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.AssemblyResolve += (sender, bargs) =>
			{
				string dllName = new AssemblyName(bargs.Name).Name + ".dll";
				var assem = Assembly.GetExecutingAssembly();
				string resourceName = null;
				foreach (string str in assem.GetManifestResourceNames())
				{
					if (str.IndexOf(dllName) != -1)
					{
						resourceName = str;
						break;
					}
				}
				if (resourceName == null) return null;
				using (var stream = assem.GetManifestResourceStream(resourceName))
				{
					Byte[] assemblyData = new Byte[stream.Length];
					stream.Read(assemblyData, 0, assemblyData.Length);
					return Assembly.Load(assemblyData);
				}

			};

			Console.WriteLine("달새가 패치를 시작 합니다! >_<");
			//while (true)
			//{
			//	if (Process.GetProcessesByName("Dalsae") == null)
			//	{
			//		Console.WriteLine("달새 종료를 기다리고 있습니다.");
			//		Thread.Sleep(1000);
			//	}
			//	else
			//		break;
			//}

			//1.43v에서만 사용할 녀석...?
			//Process[] pc = Process.GetProcessesByName("Dalsae");
			//for (int i = 0; i < pc.Length; i++)
			//	pc[i].Kill();

			Patch patch = new Patch();
			patch.LoadPatch();
			Console.WriteLine($"달새 {patch.github.tag_name}v의 패치노트 웹페이지에 접속합니다. 패치노트를 읽어주세요.");
			Thread.Sleep(2000);
			System.Diagnostics.Process.Start(patch.github.html_url);
			patch.RunDalsae();
			Environment.Exit(0);
		}
	}

	public class Patch
	{
		public GitHubRelease github = null;
		public void LoadPatch()
		{
			Console.WriteLine("패치 정보를 불러옵니다.");
			HttpWebRequest req = WebRequest.Create("https://api.github.com/repos/hanalen-/Dalsae_WPF/releases/latest") as HttpWebRequest;
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
			req.Timeout = 5000;
			req.UserAgent = "Dalsae_WPF";
			string json = string.Empty;
			using (WebResponse res = req.GetResponse())
			using (Stream stream = res.GetResponseStream())
			using (StreamReader streamRead = new StreamReader(stream))
				json = streamRead.ReadToEnd();
			if (json == string.Empty) return;

			Console.WriteLine("패치 정보 불러오기 성공!");
			github = JsonConvert.DeserializeObject<GitHubRelease>(json);
			DownLoadSequence();
		}

		private void DownLoadSequence()
		{
			//파일 전부 다운로드 진행
			//파일은 exe, config, 스킨 설명파일로 최대 3개. 차후 변경 가능
			for (int i = 0; i < github.Assets.Length; i++)
			{
				if (string.Equals(github.Assets[i].name, "Dalsae_Patch.exe", StringComparison.OrdinalIgnoreCase)) continue;
				Console.WriteLine($"패치 파일 ({i} / {github.Assets.Length - 1}) 다운 진행 중");
				if (DownloadFile(github.Assets[i].name, github.Assets[i].browser_download_url) == false)
				{
					Console.WriteLine("다운로드 오류.\n달새를 재실행하여 재시도 해주세요.");
					return;
				}
			}

			Console.WriteLine("패치 파일 다운로드 성공!");
			//파일 다운에 전부 성공
			//파일 변경 작업 진행
			ChangeFile();
			Console.WriteLine("달새 패치 성공! 곧 달새가 재시작 됩니다.");
			Thread.Sleep(2000);
		}

		private bool DownloadFile(string fileName, string url)
		{
			bool ret = false;
			try
			{
				WebClient myWebClient = new WebClient();
				myWebClient.DownloadFile(url, $"{fileName}");
				ret = true;
			}
			catch { ret = false; }

			return ret;
		}

		private void ChangeFile()
		{
			for (int i = 0; i < github.Assets.Length; i++)
			{
				if (string.Equals(github.Assets[i].name, "Dalsae_Patch.exe", StringComparison.OrdinalIgnoreCase)) continue;//패치파일은 넘어가게
				File.Delete(github.Assets[i].name);
				File.Move($"{github.Assets[i].name}_", github.Assets[i].name);
			}
		}

		public void RunDalsae()
		{
			string path = $"{Directory.GetCurrentDirectory()}/Dalsae.exe";
			Process.Start(path);
		}

		public class GitHubRelease//API전체
		{
			public string tag_name { get; set; }//버전
			public string name { get; set; }//패치 글 이름
			public string html_url { get; set; }//해당 패치 페이지
			public Asset[] Assets { get; set; }
			public string body { get; set; }
		}

		public class Asset//업로드 파일 정보
		{
			public string name { get; set; }//파일 이름
			public string browser_download_url { get; set; }//파일 주소
		}
	}
}
