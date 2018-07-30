using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EduroamApp
{
	public class GenerateEapConfig
	{
		public int Status { get; set; }
		public Datum Data { get; set; }
		public string Tou { get; set; }

		public class Datum
		{
			public string Profile { get; set; }
			public string Device { get; set; }
			public string Link { get; set; }
			public string Mime { get; set; }
		}
	}
}
