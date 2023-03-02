using TagLib;
using System.Net;
using System.Security.Policy;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Data;
using Microsoft.Data.Sqlite;


//TODO: Create a file that will be used to track progress
//Track 130 missing OT
//Track 235 missing Pr
//Track 240 missing Ps
//Track 241 missing Ps
//Track 247 missing Pr
//Track 248 missing Pr
//Track 260 missing Pr
//Track 263 missing Ps  



namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private const string ESV_URL = "https://api.esv.org/v3/passage/text/";
        private const string AUTH_KEY = "Authorization";
        private const string AUTH_VALUE = "Token b14f22301e598431dabb4a6f682115a052422b5f";
        private const string SQLITE_DB_PATH = "E:\\gitHub\\ESV_Media_Compiler\\EsvCompiler\\WinFormsApp1\\lib\\dailybible.db";
        private const string AUDIO_SOURCE_FILES_PATH = "E:\\gitHub\\ESV_Media_Compiler\\EsvCompiler\\WinFormsApp1\\lib\\English Standard Version Bible";

        public static HttpClient _HttpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };

        public Form1()
        {
            InitializeComponent();
            _HttpClient.DefaultRequestHeaders.Add(AUTH_KEY, AUTH_VALUE);
        }

        private void Process()
        {
            DataTable dt = GetDatabase();
            PopulateSqliteDb(dt);
        }

        private DataTable GetDatabase()
        {
            DataTable table = new DataTable();

            table.Columns.Add(new DataColumn("Title", typeof(System.String))); //The verses
            table.Columns.Add(new DataColumn("DiscNumber", typeof(System.UInt16))); //1-365
            table.Columns.Add(new DataColumn("TrackNumber", typeof(System.UInt32))); //1-4
            table.Columns.Add(new DataColumn("Filename", typeof(System.String)));
    
            string[] fileEntries = Directory.GetFiles(AUDIO_SOURCE_FILES_PATH); 

            foreach (string fileEntry in fileEntries)
            {
                TrackInfo trackInfo = GetTagInfo(fileEntry);
                DataRow r = table.NewRow();
                r["Title"] = trackInfo.Title;
                r["DiscNumber"] = trackInfo.DiscNumber;
                r["TrackNumber"] = trackInfo.TrackNumber;
                r["Filename"] = trackInfo.Filename;

                table.Rows.Add(r);
            }

            return table;
        }

        private void PopulateSqliteDb(DataTable dataTable)
        {
            try
            {
                using (var connection = new SqliteConnection($"Data Source={SQLITE_DB_PATH}")) 
                {
                    connection.Open();

                    SqliteCommand cmd = connection.CreateCommand();

                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText =
                        @"DELETE    FROM dailybible";

                    cmd.ExecuteNonQuery();

                    cmd.CommandText =
@"INSERT    INTO dailybible (
    Title,
    DiscNumber,
    TrackNumber,
    Filename)
SELECT  $title,
        $discNumber,
        $trackNumber,
        $filename";

                    cmd.Parameters.AddWithValue("$title", "");
                    cmd.Parameters.AddWithValue("$discNumber", 0);
                    cmd.Parameters.AddWithValue("$trackNumber", 0);
                    cmd.Parameters.AddWithValue("$filename", "");
                    foreach (DataRow r in dataTable.Rows)
                    {
                        string filename = Path.GetFileName(r["Filename"].ToString());
                        cmd.Parameters["$title"].Value = r["Title"].ToString();
                        cmd.Parameters["$discNumber"].Value = r["DiscNumber"].ToString();
                        cmd.Parameters["$trackNumber"].Value = r["TrackNumber"].ToString();
                        cmd.Parameters["$filename"].Value = filename;

                        cmd.ExecuteNonQuery();
                    }

                }
            }
            catch (Exception)
            {

                throw;
            }

        }


        private TrackInfo GetTagInfo(string path)
        {
            var tfile = TagLib.File.Create(path);

            TrackInfo trackInfo = new TrackInfo();
            trackInfo.Title = tfile.Tag.Title;
            trackInfo.DiscNumber = tfile.Tag.Disc;
            trackInfo.TrackNumber = tfile.Tag.Track;
            trackInfo.Filename = path;
            return trackInfo;
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Process();
        }


        #region stubs
        private async Task<string> GetPassageText(string verseReference)
        {
            string verses = string.Empty;
            string query = string.Empty;

            verseReference = "Matthew 27:32-66";


            query = $"?q={verseReference}&include-passage-references=false&include-verse-numbers=true&include-footnotes=false&include-footnote-body=false&include-short-copyright=false&include-copyright=false&include-passage-horizontal-lines=false&include-heading-horizontal-lines=false&include-headings=true&include-selahs=true&indent-paragraphs=true&indent-poetry=true&indent-declares=0&indent-psalm-doxology=0";

            try
            {
                var response = await _HttpClient.GetStringAsync(ESV_URL + query);
                JObject passageObject = JObject.Parse(response);

                if (passageObject["passages"] != null)
                {
                    verses = passageObject["passages"].ToString();
                }

                verses = verses.TrimStart('[', '\r', '\n', ' ', ' ', '"').TrimEnd('"', '\r', '\n', ']');
                verses = verses.Replace("\\n", Environment.NewLine);
                Console.WriteLine(verses);
                //test(verses);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }






            return "";
        }

        private async Task<string> GetPassageAudio(string verseReference)
        {

            string query = string.Empty;

            verseReference = "Matthew 27:32-66";



            try
            {
                var response = await _HttpClient.GetStringAsync("https://api.esv.org/v3/passage/audio/" + "?q=ps1");
                byte[] asBytesAgain = Encoding.ASCII.GetBytes(response);
                System.IO.File.WriteAllBytes(@"E:\test.mp3", asBytesAgain);

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }






            return "";
        }


        private string GetVerseReferenceFromMp3(string path)
        {
            string verseReference = "";

            return verseReference;
        }

        #endregion
    }

    public class TrackInfo
    {
        public string Title { get; set; } = "";
        public uint DiscNumber { get; set; }
        public uint TrackNumber { get; set; }
        public string Filename { get; set; } = "";

    }
}