using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalsae.Data
{
	public class BlockList
	{
		public long next_cursor { get; set; } = -1;
		public long previous_cursor { get; set; } = -1;
		public HashSet<long> hashBlockUsers { get; set; } = new HashSet<long>();
	}

	public class FollowList
	{
		public Dictionary<long, ConcurrentDictionary<long, UserSemi>> dicFollow = new Dictionary<long, ConcurrentDictionary<long, UserSemi>>();//1차키: 계정 id, 2차키: usersemi의 id
	}

}
