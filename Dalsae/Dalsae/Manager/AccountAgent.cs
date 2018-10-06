using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Dalsae.Data;

namespace Dalsae.Manager
{
	public class AccountAgent
	{
		private static AccountAgent _instence;
		public static AccountAgent accountInstence { get { if (_instence == null) { _instence = new AccountAgent(); _instence.Init(); } return _instence; } }
		private Switter switter = null;

		public delegate void DChangeAccount(UserKey switter);
		public event DChangeAccount OnChangeAccount = null;
		public delegate void DNoAccount();
		public event DNoAccount OnNoAccount = null;

		public UserKey selectedAccount { get { return switter.selectAccount; } }
		public ObservableCollection<UserKey> obsListUser = new ObservableCollection<UserKey>();
		private bool isInit = false;
		public void Init()
		{
			if (isInit) return;

			switter=FileManager.FileInstence.LoadSwitter();
			if (switter == null)
				switter = new Data.Switter();

		
			isInit = true;
		}

		public void Start()
		{
			if (switter.dicUserKey.Count == 0 || switter.selectAccount == null)
				ChangeAccount(null);
			else if (string.IsNullOrEmpty(switter.selectAccount.Token) || string.IsNullOrEmpty(switter.selectAccount.TokenSecret))//키가 없을 경우
				ChangeAccount(null);
			else
				ChangeAccount(switter.selectAccount);

			foreach (UserKey item in switter.dicUserKey.Values)
				obsListUser.Add(item);
		}

		public List<string> GetUserNames()
		{
			List<string> list = new List<string>();

			foreach(UserKey item in switter.dicUserKey.Values)
			{
				list.Add(item.screen_name);
			}

			return list;
		}

		public void ClearAccountData()
		{
			switter = new Switter();
			obsListUser.Clear();
			SaveSwitter();
		}

		private void SaveSwitter()
		{
			FileManager.FileInstence.SaveSwitter(switter);
		}

		public void UpdateToken(DalsaeUserInfo userinfo)
		{
			User user = userinfo.user;
			if (switter.dicUserKey.ContainsKey(user.id) == false)
			{
				UserKey key = new UserKey();
				key.id = user.id;
				key.Token = userinfo.Token;
				key.TokenSecret = userinfo.TokenSecret;
				key.screen_name = user.screen_name;
				switter.dicUserKey.Add(user.id, key);
				switter.selectAccount = key;
			}
			else
			{
				switter.dicUserKey[user.id].screen_name = user.screen_name;
			}

			SaveSwitter();
		}

		public void AddAccount(UserKey userkey)
		{
			if (switter.dicUserKey.ContainsKey(userkey.id))
			{
				
				ChangeAccount(userkey);
			}
			else
			{
				switter.dicUserKey.Add(userkey.id, userkey);
				ChangeAccount(userkey);
			}
			SaveSwitter();
		}

		public void DeleteAccount()
		{
			UserKey deleteKey=switter.selectAccount;
			if (deleteKey == null) return;

			switter.dicUserKey.Remove(deleteKey.id);

			UserKey changeKey = null;

			foreach (UserKey item in switter.dicUserKey.Values)
				changeKey = item;

			ChangeAccount(changeKey);
	
			SaveSwitter();
		}

		public bool ChangeAccountByName(string screenName)
		{
			if (switter.selectAccount.screen_name == screenName)
				return false;
			UserKey changeKey = null;

			foreach (UserKey item in switter.dicUserKey.Values)
				if (item.screen_name == screenName)
					changeKey = item;

			if (changeKey != null)
				ChangeAccount(changeKey);
			else
				return false;

			SaveSwitter();
			return true;
		}

		private void ChangeAccount(UserKey userkey)
		{
			if (userkey == null)
			{
				switter.selectAccount = null;
				switter.dicUserKey.Clear();
				if (OnNoAccount != null)
					Application.Current.Dispatcher.BeginInvoke(OnNoAccount);
			}
			else
			{
				Dalsae.DataManager.DataInstence.UpdateToken(new Web.ResOAuth() { tokenStr = userkey.Token, secretStr = userkey.TokenSecret, isCallBack = false });
				if (OnChangeAccount != null)
				{
					switter.selectAccount = userkey;
					Application.Current.Dispatcher.BeginInvoke(OnChangeAccount, new object[] { userkey });
				}
			}
		}

		public void ChangeAccountInfo(UserKey userkey)
		{
			if (switter.dicUserKey.ContainsKey(userkey.id) == false) return;

			switter.dicUserKey[userkey.id] = userkey;

			SaveSwitter();
		}
	}
}
