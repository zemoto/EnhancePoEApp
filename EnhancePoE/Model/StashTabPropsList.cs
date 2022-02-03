using System.Collections.Generic;

namespace EnhancePoE.Model
{
   // property names from api
   public class StashTabProps
   {
      public string N { get; set; }
      public int I { get; set; }
   }
   public class StashTabPropsList
   {
      public List<StashTabProps> Tabs { get; set; }
   }
}
