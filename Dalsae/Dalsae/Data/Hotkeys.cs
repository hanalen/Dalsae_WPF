using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Dalsae.Data
{
	public class HotKey
	{
		public HotKey() { }
		public HotKey(HotKey hotkey)
		{
			isCtrl = hotkey.isCtrl;
			isAlt = hotkey.isAlt;
			isShift = hotkey.isShift;
			key = hotkey.key;
		}
		public bool isCtrl { get; set; } = false;
		public bool isAlt { get; set; } = false;
		public bool isShift { get; set; } = false;

		public Key key { get; set; }
	}
	public class HotKeys
	{
		public HotKeys()
		{
			SetDefaultKey();
		}

		public void SetDefaultKey()
		{
			dicHotKey.Clear();
			dicHotKey.Add(eHotKey.eKeyDeleteTweet, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.Delete });
			dicHotKey.Add(eHotKey.eKeyCopyTweet, new HotKey() { isAlt = false, isCtrl = true, isShift = false, key = Key.C});
			dicHotKey.Add(eHotKey.eKeyFavorite, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.F});
			dicHotKey.Add(eHotKey.eKeyGoEnd, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.End });
			dicHotKey.Add(eHotKey.eKeyGoHome, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.Home});
			dicHotKey.Add(eHotKey.eKeyHashTag, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.H});
			dicHotKey.Add(eHotKey.eKeyInput, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.U});
			dicHotKey.Add(eHotKey.eKeyLoad, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.Space});
			dicHotKey.Add(eHotKey.eKeyMenu, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.V});
			dicHotKey.Add(eHotKey.eKeyOpenImage, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.G });
			dicHotKey.Add(eHotKey.eKeyQRetweet, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.W});
			dicHotKey.Add(eHotKey.eKeyReply, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.R});
			dicHotKey.Add(eHotKey.eKeyReplyAll, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.A});
			dicHotKey.Add(eHotKey.eKeyRetweet, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.T});
			dicHotKey.Add(eHotKey.eKeySendDM, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.D });
			dicHotKey.Add(eHotKey.eKeyShowDM, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.D3 });
			dicHotKey.Add(eHotKey.eKeyShowFavorite, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.D4});
			dicHotKey.Add(eHotKey.eKeyShowMention, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.D2});
			dicHotKey.Add(eHotKey.eKeyShowOpendUrl, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.D5});
			dicHotKey.Add(eHotKey.eKeyShowTL, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.D1 });
			dicHotKey.Add(eHotKey.eKeySmallPreview, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.B });
			dicHotKey.Add(eHotKey.eKeyRefresh, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.F5 });
			dicHotKey.Add(eHotKey.eKeyClear, new HotKey() { isAlt = false, isCtrl = false, isShift = false, key = Key.Escape });
		}

		public enum eHotKey
		{
			eNone,
			eKeyReplyAll,
			eKeyReply,
			eKeyRetweet,
			eKeyFavorite,
			eKeyQRetweet,
			eKeyInput,
			eKeySendDM,
			eKeyHashTag,
			eKeyShowTL,
			eKeyShowMention,
			eKeyShowDM,
			eKeyShowFavorite,
			eKeyGoHome,
			eKeyGoEnd,
			eKeyDeleteTweet,
			eKeyLoad,
			eKeyShowOpendUrl,
			eKeyMenu,
			eKeyOpenImage,
			eKeyCopyTweet,
			eKeySmallPreview,
			eKeyRefresh,
			eKeyClear,
		}
		public Dictionary<eHotKey, HotKey> dicHotKey { get; set; } = new Dictionary<eHotKey, HotKey>();
		
		public eHotKey PressHotKey(bool isCtrl, bool isShift, bool isAlt, Key key)
		{
			eHotKey ret = eHotKey.eNone;
			foreach (KeyValuePair<eHotKey, HotKey> item in dicHotKey)
			{
				HotKey hotKey = item.Value;
				if (isCtrl == hotKey.isCtrl && isAlt == hotKey.isAlt && isShift == hotKey.isShift && key==hotKey.key)
				{
					ret = item.Key;
					break;
				}
			}

			return ret;
		}
	}
}
