using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Dalsae.API;

namespace Dalsae.Web
{
	public class ClientSendTweet
	{
		public PacketUpdate parameter { get; private set; }
		public BitmapImage[] listBitmap { get; private set; }
		public List<ClientMultimedia> listMedia { get; private set; } = new List<ClientMultimedia>();
		public string multiPath { get; private set; }
		private int index = -1;
		public void SetTweet(PacketUpdate parameter, BitmapImage[] listBitmap=null)
		{
			this.parameter = parameter;
			this.listBitmap = listBitmap;
			if (listBitmap != null)
				if (listBitmap.Length > 0)
					parameter.media_ids = string.Empty;//이거 안 하면 nullException뜸
		}

		public void Reset()
		{
			index = -1;
		}

		public BitmapImage GetNextImage()
		{
			index++;
			if (index >= listBitmap.Length)
				return null;
			else
				return listBitmap[index];
		}


		public void SetTweet(PacketUpdate parameter, string multiPath)
		{
			this.parameter = parameter;
			this.multiPath = multiPath;
			if (string.IsNullOrEmpty(multiPath) == false)
				parameter.media_ids = string.Empty;//이거 안 하면 nullException뜸
		}

		public bool ResponseMedia(ClientMultimedia media)
		{
			listMedia.Add(media);
			if (listMedia.Count == listBitmap?.Length)
			{
				for (int i = 0; i < listMedia.Count; i++)
				{
					parameter.media_ids += listMedia[i].media_id;
					parameter.media_ids += ",";
				}
				return true;
			}
			else if (string.IsNullOrEmpty(multiPath) == false)
			{
				parameter.media_ids += media.media_id;
				parameter.media_ids += ",";
				return true;
			}
			return false;
		}
	}

}
