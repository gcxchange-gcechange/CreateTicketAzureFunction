using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UpdateUserHttp
{
  class LgObject
  {
    public string token_type { get; set; }
    public string scope { get; set; }
    public string expires_in { get; set; }
    public string ext_expires_in { get; set; }
    public string access_token { get; set; }
  }
}
