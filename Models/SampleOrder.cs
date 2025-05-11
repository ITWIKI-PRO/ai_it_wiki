using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ai_it_wiki.Models
{
  public class SampleOrder
  {
    //[Newtonsoft.Json.JsonIgnore, System.Text.Json.Serialization.JsonIgnore]
    public int OrderID { get; set; }
    public DateTime OrderDate { get; set; }
    public string CustomerID { get; set; }
    public string CustomerName { get; set; }
    public string ShipCountry { get; set; }
    public string ShipCity { get; set; }
  }
}
