using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ExcelReader_NS
{
    [Serializable]
    public class ExcelReader
    {
        public List<PlayerEntry> GetListFromExcel(string PathToFile)
        {
            List<PlayerEntry> PlayerList = new List<PlayerEntry>();


            using StreamReader File = new StreamReader(PathToFile);
            
            while (GetNextLine(File, out string Line))
            {
                string[] Values = Line.Split(";");
                PlayerEntry New = new PlayerEntry(Values[0], Values[1]);
                PlayerList.Add(New);
            }
            

            return PlayerList;
        }

        public bool GetNextLine(StreamReader Stream, out string NewLine)
        {
            string Line = Stream.ReadLine();
            if (Line == null || Line == string.Empty)
            {
                NewLine = null;
                return false;
            }
            else
            {
                NewLine = Line;
                return true;
            }

        }
    }

    [Serializable]
    public class PlayerEntry
    {
        public string Name;
        public string StartNumber;

        public PlayerEntry(string name, string startnumber)
        {
            Name = name;
            StartNumber = startnumber;
        }
    }

}
