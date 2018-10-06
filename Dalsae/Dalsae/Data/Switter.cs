using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalsae.Data
{
	public class Switter : BaseNoty
	{
		private UserKey _selectAccount;
		public UserKey selectAccount
		{
			get
			{
				return _selectAccount;
			}
			set
			{
				_selectAccount = value;
				OnPropertyChanged("selectAccount");
			}
		}
		public Dictionary<long, UserKey> dicUserKey = new Dictionary<long, UserKey>();
		//public ObservableCollection<UserKey> listUser = new ObservableCollection<UserKey>();
	}

	public class UserKey
	{
		public long id { get; set; }
		public string screen_name { get; set; }
		public string Token { get; set; }
		public string TokenSecret { get; set; }
	}

}
