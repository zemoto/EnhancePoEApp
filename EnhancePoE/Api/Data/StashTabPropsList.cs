﻿using System.Collections.Generic;

namespace EnhancePoE.Api.Data;

// property names from api
public class StashTabProps
{
   public string n { get; set; }
   public int i { get; set; }
}

public class StashTabPropsList
{
   public List<StashTabProps> tabs { get; set; }
}
