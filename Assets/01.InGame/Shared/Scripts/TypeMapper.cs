using System;
using UnityEngine;

public static class TypeMapper
{
     public static Petal.Type MapPetalType(TriggerSpawnPetal.TriggerType type)
     {
          switch (type)
          {
               case TriggerSpawnPetal.TriggerType.PetalYellow:
                    return Petal.Type.Yellow;
               case TriggerSpawnPetal.TriggerType.PetalOrange:
                    return Petal.Type.Orange;
               case TriggerSpawnPetal.TriggerType.PetalRed:
                    return Petal.Type.Red;
               case TriggerSpawnPetal.TriggerType.PetalPurple:
                    return Petal.Type.Purple;
               default:
                    return Petal.Type.Unknown;
          }
     }
        
}