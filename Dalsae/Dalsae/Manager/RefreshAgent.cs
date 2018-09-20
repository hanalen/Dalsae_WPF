using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Threading;
using Dalsae.Data;

namespace Dalsae.Manager
{
	public class RefreshAgent
	{
		private static RefreshAgent _instence;
		public static RefreshAgent refreshAgent { get { if (_instence == null) _instence = new RefreshAgent(); return _instence; } }

		private DateTime prevUpdateTime = DateTime.Now;

		private DispatcherTimer timerRefresh = new DispatcherTimer();
		private DispatcherTimer timerWindow = new DispatcherTimer();
		
		private RefreshAgent()
		{
			timerRefresh.IsEnabled = false;
			timerWindow.IsEnabled = false;
			timerRefresh.Interval = TimeSpan.FromSeconds(60);
			timerWindow.Interval = TimeSpan.FromSeconds(1);
			
		}

		private void Timer_Tick(object sender, EventArgs e)
		{
			prevUpdateTime = DateTime.Now;
			APICallAgent.apiInstence.LoadTweetList(eTweetPanel.eHome, TweetManager.TweetInstence.GetTopTweet(eTweetPanel.eHome));
			APICallAgent.apiInstence.LoadTweetList(eTweetPanel.eMention, TweetManager.TweetInstence.GetTopTweet(eTweetPanel.eMention));
		}

		public void SetWindowTick(EventHandler refreshTick)
		{
			timerRefresh.Tick += Timer_Tick;
			timerWindow.Tick += refreshTick;
			timerWindow.IsEnabled = true;
			timerRefresh.IsEnabled = true;
		}

		public void Refresh(eTweetPanel panel, long sinceID = -1)
		{
			APICallAgent.apiInstence.LoadTweetList(panel, sinceID);
		}

		public void Reset()
		{

		}

		
	}
}
