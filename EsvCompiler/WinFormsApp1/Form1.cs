using TagLib;
using System.Net;
using System.Security.Policy;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private const string ESV_URL = "https://api.esv.org/v3/passage/text/";
        private const string AUTH_KEY = "Authorization";
        private const string AUTH_VALUE = "Token b14f22301e598431dabb4a6f682115a052422b5f";
        public static HttpClient _HttpClient = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };

        public Form1()
        {
            InitializeComponent();
            _HttpClient.DefaultRequestHeaders.Add(AUTH_KEY, AUTH_VALUE);
        }

        private async Task <string> GetPassageText(string verseReference)
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
                test(verses);
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
            }

            




            return "";
        }

        private async Task<string> GetPassageAudio(string verseReference)
        {
            string verses = string.Empty;
            string query = string.Empty;

            verseReference = "Matthew 27:32-66";


 
            try
            {
                var response = await _HttpClient.GetStringAsync(ESV_URL + "?q=ps1");
                //JObject passageObject = JObject.Parse(response);

                //verses = passageObject["passages"].ToString();

                //verses = verses.Replace("\\n", Environment.NewLine);
                Debug.WriteLine(response.ToString());
                //test(verses);
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


        private void test(string passage)
        {
            var tfile = TagLib.File.Create(@"E:\English Standard Version Bible\44-02 Matthew 27_32-66.mp3");
            string title = tfile.Tag.Title;
            TimeSpan duration = tfile.Properties.Duration;
            Console.WriteLine("Title: {0}, duration: {1}", title, duration);

            // change title in the file
            tfile.Tag.Lyrics = passage;
            tfile.Save();
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            
        }


        private void button1_Click(object sender, EventArgs e)
        {
            GetPassageText("");

        }
    }
}