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
        private UIElements UI;

        /// <summary>
        /// Takes a given Excel file, and converts it into a list containing each player's data
        /// </summary>
        /// <param name="PathToFile">The full file path to the Excel sheet (must be .CSV)</param>
        /// <returns>The list of <see cref="PlayerEntry"/> containing all info on each player, and their menu <see cref="Entry"/></returns>
        public List<PlayerEntry> GetListFromExcel(string PathToFile)
        {
            UI = GameObject.Find("UIHandler").GetComponent<UIElements>();
            List<PlayerEntry> PlayerList = new List<PlayerEntry>();
            using StreamReader File = new StreamReader(PathToFile);

            while (GetNextLine(File, out string Line))
            {
                string[] Values = Line.Split(";");
                if (UI.CreateEntry(Values[0], Values[1], out PlayerEntry New))
                {
                    PlayerList.Add(New);
                }
            }

            return PlayerList;
        }

        private bool GetNextLine(StreamReader Stream, out string NewLine)
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

    /// <summary>
    /// Player data (contains a menu entry)
    /// </summary>
    [Serializable]
    public class PlayerEntry
    {
        /// <summary>
        /// The Unity user ID, assigned when this player connects
        /// </summary>
        public ulong ID;

        /// <summary>
        /// The name of the player
        /// </summary>
        public string Name;

        /// <summary>
        /// The start number of the player
        /// </summary>
        public string StartNumber;
        public int StrikeCount;

        /// <summary>
        /// Assigned to true when this player connects
        /// </summary>
        public bool Joined = false;

        /// <summary>
        /// The menu entry of this player
        /// </summary>
        public Entry MenuEntry;

        public PlayerEntry(Entry entry)
        {
            MenuEntry = entry;
        }

        /// <summary>
        /// Update the display text of the inner <see cref="Entry"/>
        /// </summary>
        public void SetMenuText()
        {
            MenuEntry.SetText(Name, StartNumber);
        }
    }

}
