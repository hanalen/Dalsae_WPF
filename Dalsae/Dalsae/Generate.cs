using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
//using DWORD = System.UInt32;

namespace Dalsae
{
	class Generate
	{
		public static T FindElementByName<T>(FrameworkElement element, string sChildName) where T : FrameworkElement
		{
			if (element == null) return null;
			T childElement = null;
			var nChildCount = VisualTreeHelper.GetChildrenCount(element);
			for (int i = 0; i < nChildCount; i++)
			{
				FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

				if (child == null)
					continue;

				if (child is T && child.Name.Equals(sChildName))
				{
					childElement = (T)child;
					break;
				}

				childElement = FindElementByName<T>(child, sChildName);

				if (childElement != null)
					break;
			}
			return childElement;
		}

		public static bool IsOnScreen(Window window)//모니터 밖인지 체크
		{
			Screen[] screens = Screen.AllScreens;
			foreach (Screen screen in screens)
			{
				Rectangle formRectangle = new Rectangle((int)window.Left, (int)window.Top,
														 (int)window.Width, (int)window.Height);

				if (screen.WorkingArea.IntersectsWith(formRectangle))
				{
					return true;
				}
			}

			return false;
		}


		public static T FindElementByName<T>(FrameworkElement element) where T : FrameworkElement
		{
			T childElement = null;
			var nChildCount = VisualTreeHelper.GetChildrenCount(element);
			for (int i = 0; i < nChildCount; i++)
			{
				FrameworkElement child = VisualTreeHelper.GetChild(element, i) as FrameworkElement;

				if (child == null)
					continue;

				if (child is T)
				{
					childElement = (T)child;
					break;
				}

				childElement = FindElementByName<T>(child);

				if (childElement != null)
					break;
			}
			return childElement;
		}

		public static string ReplaceTextExpend(ClientTweet tweet)
		{
			string ret = tweet.originalTweet.text;
			if (tweet.isUrl)
				for (int i = 0; i < tweet.listUrl.Count; i++)
					ret = ret.Replace(tweet.listUrl[i].display_url, tweet.listUrl[i].expanded_url);

			if (tweet.isMedia)
				foreach (ClientMedia item in tweet.dicPhoto.Values)
					ret = ret.Replace(item.display_url, item.expanded_url);

			return ret;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

		//Flash both the window caption and taskbar button.
		//This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags. 
		public const UInt32 FLASHW_ALL = 3;

		// Flash continuously until the window comes to the foreground. 
		public const UInt32 FLASHW_TIMERNOFG = 12;

		[StructLayout(LayoutKind.Sequential)]
		public struct FLASHWINFO
		{
			public UInt32 cbSize;
			public IntPtr hwnd;
			public UInt32 dwFlags;
			public UInt32 uCount;
			public UInt32 dwTimeout;
		}

		// Do the flashing - this does not involve a raincoat.
		public static bool FlashWindowEx(Window window)
		{
			IntPtr hWnd = new WindowInteropHelper(window).Handle;
			FLASHWINFO fInfo = new FLASHWINFO();

			fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
			fInfo.hwnd = hWnd;
			fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
			fInfo.uCount = UInt32.MaxValue;
			fInfo.dwTimeout = 0;

			return FlashWindowEx(ref fInfo);
		}
	}
}
