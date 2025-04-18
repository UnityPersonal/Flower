using System;
using UnityEngine;

public static class TypeMapper
{
     public static Petal.Type MapPetalType(TriggerType type)
     {
          switch (type)
          {
               case TriggerType.PetalYellow:
                    return Petal.Type.Yellow;
               case TriggerType.PetalOrange:
                    return Petal.Type.Orange;
               case TriggerType.PetalRed:
                    return Petal.Type.Red;
               case TriggerType.PetalPurple:
                    return Petal.Type.Purple;
               default:
                    return Petal.Type.Unknown;
          }
     }
        
}