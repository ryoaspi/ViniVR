using System.IO;
using UnityEngine;

namespace TheFoundation.Runtime
{
    public static class SavePaths
    {
        //Todo tool editor to edit/create desired paths ? 
        
        //Roots
        public static string Root => 
            Path.Combine(Application.persistentDataPath,"Saves");
        public static string PlayerRoot =>
            Path.Combine(Root,"Player");
        public static string WordsRoot =>
            Path.Combine(Root,"Words");
        public static string LangRoot =>
            Path.Combine(WordsRoot,"Lang");
        //Files
        public static string PlayerProgressionFile =>
            Path.Combine(PlayerRoot,"progression.json");
     
    }

}
