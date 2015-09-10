using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VelocityDb;

namespace VelocityDbSchema
{
  public class EntityNewsData : OptimizedPersistable 
	{
    Guid _id;
    string docId; // or should be int?
    DateTime publishDate;
    int start;
    int stop;
    string type;
    float confidence;
    string entityName;
    string personTitle;
    string personFirstName;
    string personMiddleName;
    string personLastName;
    string personSuffix;
    string geoName;
    string geoClass;
    string geoType;
    float geoWeight;
    float geoLat;
    float geoLon;
    public EntityNewsData(string[] record)
    {
      int i = 0;
      foreach (string f in record)
      {
        switch (i)
        {
          case 0:
            Guid.TryParse(record[i], out _id);
            break;
          case 1:
            docId = record[i];
            break;
          case 2:
            DateTime.TryParse(record[i], out publishDate);
            break;
          case 3:
            int.TryParse(record[i], out start);
            break;
          case 4:
            int.TryParse(record[i], out stop);
            break;
          case 5:
            type = record[i];
            break;
          case 6:
            float.TryParse(record[i], out confidence);
            break;
          case 7:
            entityName = record[i];
            break;
          case 8:
            personTitle = record[i];
            break;
          case 9:
            personFirstName = record[i];
            break;
          case 10:
            personMiddleName = record[i];
            break;
          case 11:
            personLastName = record[i];
            break;
          case 12:
            personSuffix = record[i];
            break;
          case 13:
            geoName = record[i];
            break;
          case 14:
            geoClass = record[i];
            break;
          case 15:
            geoType =  record[i];
            break;
          case 16:
            float.TryParse(record[i], out geoWeight);
            break;
          case 17:
            float.TryParse(record[i], out geoLat);
            break;
          case 18:
            float.TryParse(record[i], out geoLon);
            break;
         // default:
           // throw new UnexpectedException("Out of Range");
        }
        i++;
      }
    }
  }
}
