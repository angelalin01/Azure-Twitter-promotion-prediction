using System;
using System.Text;
using System.Linq;
using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace testapp
{
    // Class object to store result to .csv file
    // Below are all defined attributes of an Azure promotion tweet 
    public class TwitterResult
    {
        [Display(Name = "ContainsURL")]
        public bool ContainsURL { get; set; }
        [Display(Name = "MicrosoftURL")]
        public bool IsMicrosoftURL { get; set; }
        [Display(Name = "ContainsAzureWord")]
        public bool ContainsAzure { get; set; }
        [Display(Name = "ContainsExclamation")]
        public bool ContainsExclamation { get; set; }
        [Display(Name = "ContainsColon")]
        public bool ContainsColon { get; set; }
        [Display(Name = "ContainsQuestionMark")]
        public bool ContainsQuestionMark { get; set; }
        [Display(Name = "ContainsKeyword")]
        public bool ContainsKeyword { get; set; }
        [Display(Name = "MentionsCometitors")]
        public bool MentionsCompetitors { get; set; }
        [Display(Name = "ContainsBenefit")]
        public bool ContainsBenefit {get; set;}
        [Display(Name = "Intent")]
        public string Intent { get; set; }
        [Display(Name = "IntentScore")]
        public float IntentScore { get; set; }
        
    }

    // Class object to hold the return values (intent and score) of LUIS API call
    // Intent is either "promotion" or "none"
    // Score is level of confidence/probability, indicates strength of prediction
    public class LUISResult
    {
        public string Intent { get; set; }
        public float Score { get; set; }
    }

    class Program
    {
        // Main entry for the program
        static void Main(string[] args)
        {
            ConvertCsv2Txt(10);
            ProcessAllTwitters().Wait();
        }

        private static int totalTwitterCount = 91503;

// Ensures a unique random sample from the total set of raw Twitter data is taken each time for analysis
        private static void ConvertCsv2Txt(int lines)

{


// Tracking the line numbers of the tweets (in any text or excel file) that have already been sampled/analysis

List<int> existingLineNums = new List<int>();

if (File.Exists("ExistingLineNums.txt"))

{

using (StreamReader reader =new StreamReader("ExistingLineNums.txt"))

{

string line;

while ((line =reader.ReadLine()) !=null)

{

int result;

if (int.TryParse(line,out result))

{

existingLineNums.Add(result);

}}}}

// Get the line numbers of random tweets

List<int> randomLineNums = new List<int>();

for (int i =0; i < lines; i++)

{

int num = new Random().Next(1,totalTwitterCount);//random selection based on tweet line number; totalTwitterCount is total number of tweets available 

if (!randomLineNums.Contains(num) && !existingLineNums.Contains(num))

{

randomLineNums.Add(num);//will only analyze tweet if its line number has not been recorded before (aka new unique tweet)

}

}



// Convert the file only if the line number equals to the random line numbers determined from prev method
// Line number should equal to number produced by prev method 

using (StreamReader reader = new StreamReader("twitter_output_influencers.csv"))

{
//random tweets printed in this text file
using (StreamWriter writer = new StreamWriter("TwitterRawData2.txt"))

{

string line;

int lineNum = 1;

int outputLineNum = 1;

line = reader.ReadLine();

if (line !=null)

{

while ((line =reader.ReadLine()) !=null && outputLineNum <=lines)

{

if (randomLineNums.Contains(lineNum))

{

var csv = line.Split(',').ToList();

//cleaning up data that contains these characters, which are not recognized by LUIS
writer.WriteLine(csv.Last().Replace("&nbsp;_",//occurs in some URLs
"").Replace("&nbsp;",
"").Replace("\"",
"").Replace("#",
"").Replace("&gt;",
""));//continue to add to list as more are discovered 

outputLineNum++;

}

lineNum++;}}}}



// Write the random line num back to the file

using (StreamWriter writer = new StreamWriter("ExistingLineNums.txt", true))

{

foreach (var n in randomLineNums)

{

writer.WriteLine(n);

}}}

        // Process all the twitters' raw data to conduct attribute analysis
        private static async Task ProcessAllTwitters()
        {
            List<TwitterResult> results = new List<TwitterResult>();
            foreach (string twitterContent in ReadTweetsFromFile("TwitterRawData3.txt"))
            {
                results.Add(await ProcessTwitter(twitterContent));
            }
            CsvWriter writer = new CsvWriter();//documents all attribute data in new CSV file
            writer.Write(results, "TwitterOutput.csv", true);
        }

        // Regular expression pattern for detecting URLs
        private static Regex urlRegex =
        new Regex(@"((http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Keyword list to detect if Azure-related words are mentioned in the twitter content
        // can continue to add to list or change
        private static List<string> azureKeywords = new List<string>()
        {
            "azure",
            "hdinsight",
            "microsoft cloud",
            "#azure",
            "parallel file systems",
            "SDK",
            "U-SQL"
        };

        // Convert the Azure keyword list into Regex pattern
        private static Regex azureRegex = new Regex(
            azureKeywords.Select(a => @"\b" + a + @"\b").Aggregate((a1, a2) => a1 + "|" + a2), RegexOptions.IgnoreCase);

        // Keyword list to detect if the URL belongs to Microsoft
        private static List<string> microsoftUrlKeywords = new List<string>()
        {
            "microsoft",
            "azure"
        };

        // Promotion related keyword list
        // can continue to add or change
        private static List<string> promotionKeywords = new List<string>()
        {
            "promotion",
            "available",
            "now",
            "preview",
            "announcing",
            "introducing",
            "releasing",
            "launch",
            "start",
            "development",
            "how",
            "improve",
            "have you seen",
            "how to",
            "using Azure",
            "with Azure",
            "on Azure",
            "tips",
            "in Azure",
            "new",
            "get started",
            "should",
            "join",
            "great",
            "good",
            "amazing",
            "love",
            "to Azure",
            "empowering",
            "use",
            "able",
            "w/ Azure",
            "with NVIDIA GRID",

        };

        // Convert the promotion keyword list into Regex pattern
        private static Regex promotionRegex = new Regex(
            promotionKeywords.Select(p => @"\b" + p + @"\b").Aggregate((p1, p2) => p1 + "|" + p2), RegexOptions.IgnoreCase);

        // Competitor keyword list
        // can continue to add or change
        private static List<string> competitorKeywords = new List<string>()
        {
            "aws",
            "amazon",
            "ec2",
            "google cloud",
            "gcp",
            "#aws",
            "#gcp",
            "skype",
            "facebook",
            "oracle",
            "google"
        };

        // Convert the competitor keyword list into Regex pattern
        private static Regex competitorRegex = new Regex(
            competitorKeywords.Select(c => @"\b" + c + @"\b").Aggregate((c1, c2) => c1 + "|" + c2), RegexOptions.IgnoreCase);

        // keyword list for the benefitKeywords
        // can change or add to it 
        private static List<string> benefitKeywords = new List<string>()
        {
            "manage",
            "customize",
            "maintain",
            "utilize",
            "capability",
            "organize",
            "create",
            "support",
            "backup",
            "recovery",
            "integrate",
            "build",
            "scale",
            "support",
            "secure",
            "enhance",
            "develop",
            "improve",
            "detection",
            "severless",
            "service",
            "deployment",
            "helps",
            "expanded",
            "increase",
            "solution",
            "impressive",
            "scalable",
            "distributed",
            "assess",
            "scaling",
            "high",
            "full",
            "reliable",
            "easy",
            "quick",
            "easier",
            "quicker"
        };

        // Regex for benefit keywords 
        private static Regex benefitRegex = new Regex(
            benefitKeywords.Select(x => @"\b" + x + @"\b").Aggregate((x1, x2) => x1 + "|" + x2), RegexOptions.IgnoreCase);
        // Read twitter raw data from the TwitterRawData.txt line by line.
        private static IEnumerable<string> ReadTweetsFromFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    return File.ReadLines(fileName);
                }
            }
            catch(Exception e) 
            {
                Console.WriteLine(e.Message);
            }
            return null;
        }

        // Convert the short URL to the original URL
        private static string GetSourceUri(string uri)
        {
            string redirectUri = string.Empty;
            try
            {
                HttpWebRequest request= (HttpWebRequest)HttpWebRequest.Create(uri);
                request.Referer = uri;
                request.Timeout = 4000;
                request.AllowAutoRedirect = false;
                using (WebResponse response = request.GetResponse())
                {
                    redirectUri = response.Headers["Location"];
                }
            }
            catch (Exception e)
            {Console.WriteLine(e.Message);}
            return redirectUri;
        }

        // Read webpage title based on the URL
        private static string GetTitleFromUri(string uri)
        {
            try
            {
                WebClient x = new WebClient();
                string source = x.DownloadString(uri);
                string title = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
                return title;
            }
            catch
            {
                return string.Empty;
            }
        }

        // URL format for the LUIS service
        private static string LUISEndpoint = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/{0}?subscription-key={1}&timezoneOffset=0&verbose=true&q=\"{2}\"";
        // Calls the LUIS service
        private static async Task<LUISResult> CallLUISAsync(string content,
        // Your app appID
        string appID = "af554cfd-af68-4c5d-b1b0-154e5e1e668d",
        // Your subscription key
        string subscriptionKey = "5d0d0e0f9572469ca415f52d332fa1f7")
        {
            string uri = string.Format(LUISEndpoint, appID, subscriptionKey, content);
            string json = null;
            HttpClient client = new HttpClient();
            LUISResult result = new LUISResult();
            client.BaseAddress = new Uri(uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage message = await client.GetAsync(uri);
            if (message.IsSuccessStatusCode)
            {
                json = await message.Content.ReadAsStringAsync();
                JsonTextReader reader = new JsonTextReader(new StringReader(json));
                while (reader.Read())
                {
                    // Gets the top scoring intent
                    if (reader.Path == "topScoringIntent.intent" && reader.TokenType.ToString() == "String")
                    {
                        result.Intent = reader.Value.ToString();
                    }
                    // Gets the top scoring intent score
                    else if (reader.Path == "topScoringIntent.score" && reader.TokenType.ToString() == "Float")
                    {
                        result.Score = float.Parse(reader.Value.ToString());
                    }
                }
            }

            return result;
        }

        // Analysis of tweet for all other attributes 
        private static async Task<TwitterResult> ProcessTwitter(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return null;
            }
            var result = new TwitterResult();
            var matchURL = urlRegex.Match(content);
            if (matchURL.Success)//determine if tweet contains a URL 
            {
                result.ContainsURL = true;

                Uri uri = null;
                Uri.TryCreate(matchURL.Value, UriKind.Absolute, out uri);
                Uri sourceUri = null;
                if (uri != null)
                {
                    if(uri.Host.Equals("t.co"))//shortened URL version in twitter
                    {
                        sourceUri = new Uri(GetSourceUri(uri.AbsoluteUri));
                    }
                    else
                    {
                        sourceUri = uri;
                    }

                    if (microsoftUrlKeywords.Any(m => sourceUri.Host.ToLower().Contains(m)))
                    {
                        result.IsMicrosoftURL = true;
                    }//consider scenario when URL is azure-specific but doesn't contain azure keywords
                    else
                    {
                        string title = GetTitleFromUri(sourceUri.AbsoluteUri);//looks if webpage title contains "Microsoft" or "Azure" and not a competitor
                        if (microsoftUrlKeywords.Any(m => title.ToLower().Contains(m)) && !competitorRegex.Match(title).Success)
                        {
                            result.IsMicrosoftURL = true;
                        }
                    }

                }
            }

            // Determines presence of other attributes 
            if (azureRegex.Match(content).Success)
            {
                result.ContainsAzure = true;
            }

            if (promotionRegex.Match(content).Success)
            {
                result.ContainsKeyword = true;
            }

            if (competitorRegex.Match(content).Success)
            {
                result.MentionsCompetitors = true;
            }

            if (benefitRegex.Match(content).Success)
            {
                result.ContainsBenefit = true;
            }

            if (content.Contains("?"))
            {
                result.ContainsQuestionMark = true;
            }

            if (content.Contains(":"))
            {
                result.ContainsColon = true;
            }

            if (content.Contains("!"))
            {
                result.ContainsExclamation = true;
            }

            LUISResult lresult = await CallLUISAsync(content);
            result.Intent = lresult.Intent;
            result.IntentScore = lresult.Score;

            return result;
        }
    }
}