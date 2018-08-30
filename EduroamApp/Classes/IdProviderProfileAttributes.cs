using System.Collections.Generic;

namespace EduroamApp
{
	/// <summary>
	/// Stores information found in json for an IdentityProvider's profile attributes.
	/// </summary>
	public class IdProviderProfileAttributes
	{
		// Properties
		public int Status { get; set; }
		public Datum Data { get; set; }
		public string Tou { get; set; }

		/// <summary>
		/// Contains options.
		/// </summary>
		public class Options
		{
			public int Sign { get; set; }
			public string DeviceId { get; set; }
			public string Mime { get; set; }
			public string Args { get; set; }
			public int? Hidden { get; set; }
			public int? Redirect { get; set; }
			public string Message { get; set; }
			public int? NoCache { get; set; }
		}

		/// <summary>
		/// Contains device information.
		/// </summary>
		public class Device
		{
			public string Id { get; set; }
			public string Display { get; set; }
			public int Status { get; set; }
			public string Redirect { get; set; }
			public int EapCustomtext { get; set; }
			public int DeviceCustomtext { get; set; }
			public object Message { get; set; }
			public Options Options { get; set; }
		}

		/// <summary>
		/// Contains data.
		/// </summary>
		public class Datum
		{
			public string LocalEmail { get; set; }
			public string LocalPhone { get; set; }
			public string LocalUrl { get; set; }
			public List<Device> Devices { get; set; }
		}
	}
}
