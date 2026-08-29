using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Library.Models;

public class UpdateResponseDto
{
    [JsonProperty("http://letswifi.app/update#v1")]
    public UpdateResponseRootDto? UpdateRoot { get; set; }
}

public class UpdateResponseRootDto
{
    public string? Os { get; set; }
    public string? Arch { get; set; }
    public string? MinimalSupportedVersion { get; set; }
    public string? CurrentVersion { get; set; }
    public string? DownloadUrl { get; set; }
}
