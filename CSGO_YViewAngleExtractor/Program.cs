using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DemoInfo;

namespace CSGO_YViewAngleExtractor
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Select a demo");
            string path = "";
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    path = dialog.FileName;
                }
            }
            using (var fs = File.OpenRead(path))
            {
                using (var parser = new DemoParser(fs))
                {
                    //Prepare
                    parser.ParseHeader();
                    string outputFileName = path + "_test.csv";
                    var outputStream = new StreamWriter(outputFileName);

                    //CSV header
                    outputStream.WriteLine(GenerateCSVHeader());

                    //Variables to check in events
                    bool hasMatchStarted = false;
                    Dictionary<Player, List<int>> Data = new Dictionary<Player, List<int>>();

                    //events
                    parser.MatchStarted += (sender, eventArgs) =>
                    {
                        hasMatchStarted = true;
                        foreach (var player in parser.PlayingParticipants)
                        {
                            Data[player] = new List<int>(new int[361]);
                        }
                    };

                    parser.TickDone += (sender, eventArgs) =>
                    {
                        if (!hasMatchStarted)
                            return;
                        foreach (var player in parser.PlayingParticipants)
                        {
                            int y = Convert.ToInt32(player.ViewDirectionY);
                            Data[player][y]++;
                        }
                    };

                    parser.ParseToEnd();

                    //Print Data
                    string dirstr;
                    foreach (var item in Data)
                    {
                        dirstr = "";
                        foreach (var y in item.Value)
                        {
                            dirstr += y + ",";
                        }
                        outputStream.WriteLine(dirstr);
                    }

                    //Close
                    outputStream.Close();
                }
            }
        }

        private static string GenerateCSVHeader()
        {
            string output = "0";
            for (int i = 1; i < 361; i++)
            {
                output += "," + i;
            }

            return output;
        }
    
    }
}
