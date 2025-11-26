using System.Collections.Generic;
using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerDeck", menuName = "Scriptable Objects/PlayerDeck")]
public class PlayerDeck : ScriptableObject  //out game deck data gemerate, communication
{
   public List<string> playerdecklist = new List<string>();
   static public string playerdeckpath => Path.Combine(Application.persistentDataPath + "/playerdeckdata.json");
   
   [System.Serializable]
   public class SaveData
   {
      public List<string> saveplayerdecklist = new List<string>();
   }
   public void Save()   //must excute after addlist
   {
      var data = new SaveData { saveplayerdecklist = playerdecklist };
      var json = JsonUtility.ToJson(data, true);
      System.IO.File.WriteAllText(playerdeckpath, json);
   }
   public void Load()
   {
      if (System.IO.File.Exists(playerdeckpath))
      {
         var json = System.IO.File.ReadAllText(playerdeckpath);
         var data = JsonUtility.FromJson<SaveData>(json);
         playerdecklist = data.saveplayerdecklist ?? new List<string>();
      }
      else
      {
         Debug.Log("playerdeck file empty");
         playerdecklist = new List<string>();
      }
   }
   public void AddList(string addid)
    {
      playerdecklist.Add(addid);
    }
}
