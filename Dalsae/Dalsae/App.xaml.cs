using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using static Dalsae.FileManager;
using static Dalsae.DalsaeManager;
using static Dalsae.TwitterWeb;
using static Dalsae.DataManager;
using static Dalsae.TweetManager;
using System.IO;
using SharpRaven;
using SharpRaven.Data;
using System.Windows.Threading;
using System.Net;

namespace Dalsae
{
	/// <summary>
	/// App.xaml에 대한 상호 작용 논리
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
			this.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
			this.Dispatcher.UnhandledExceptionFilter += Dispatcher_UnhandledExceptionFilter;
			System.AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			System.Threading.Tasks.TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
			if (File.Exists("Newtonsoft.Json.dll") == false)
			{
				MessageBox.Show("Newtonsoft.Json.dll 파일이 없습니다.\r\n공식 배포 페이지에서 다운로드라고 되어있는 항목에서\r\nDalsae_xxx.zip 파일을 받아 압축을 풀어주세요.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
				System.Diagnostics.Process.Start("https://github.com/hanalen-/Dalsae_WPF/releases");
				Application.Current.Shutdown();
				return;
			}
			DataInstence.Init();
			FileInstence.Init();//init
			Manager.AccountAgent.accountInstence.Init();
			DalsaeInstence.Init();//init
		}

		private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			e.SetObserved();
			SendException(e.Exception);
		}

		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			SendException((Exception)e.ExceptionObject);
		}

		private void Dispatcher_UnhandledExceptionFilter(object sender, DispatcherUnhandledExceptionFilterEventArgs e)
		{
			SendException(e.Exception);
		}

		private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			e.Handled = true;
			SendException(e.Exception);
		}

		public static void SendException(Exception e)
		{
			SendSentry(new SentryEvent(e));
		}

		public void SendException(string msg)
		{
			SendSentry(new SentryEvent(new SentryMessage(msg)));
		}

		public void SendException(SentryMessage msg)
		{
			SendSentry(new SentryEvent(msg));
		}

		public static void SendException(string msg, string json)
		{
			Exception ex = new Exception(msg);
			ex.Data.Add("data", json);
			SendSentry(new SentryEvent(ex));
		}

		public static void SendSentry(SentryEvent sentry)
		{
			if (DataInstence?.option?.isSendError == false) return;
			RavenClient ravenClient = new RavenClient("https://4d47f583a98748cfafb1ce90eb41844f:c9c950b43eb24cbb9a47b199e11cfa93@sentry.io/1191534");
			ravenClient.Capture(sentry);
		}
	}
}
