using System;
using UnityEngine;

public static class TypeMapper
{
     public static Petal.Type MapPetalType(TriggerItem.TriggerType type)
     {
          switch (type)
          {
               case TriggerItem.TriggerType.PetalYellow:
                    return Petal.Type.Yellow;
               case TriggerItem.TriggerType.PetalOrange:
                    return Petal.Type.Orange;
               case TriggerItem.TriggerType.PetalRed:
                    return Petal.Type.Red;
               case TriggerItem.TriggerType.PetalPurple:
                    return Petal.Type.Purple;
               default:
                    return Petal.Type.Unknown;
          }
     }
        
}