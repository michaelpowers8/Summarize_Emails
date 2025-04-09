using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data.SqlClient;
using System.Linq;

/// <summary>
/// This program is used to create a basic actionable summary of emails using locally downloaded 
/// ChatGPT. Any API call made in this program is not connected to the internet, it is being sent
/// to a downloaded ChatGPT model from the library of Ollama models that can be found at https://ollama.com/search
/// These models allow us to use privileged client data in an internal way that still uses the 
/// advancements that chatgpt offers.
/// </summary>
class Program
{
    static async Task Main()
    {
        //String used to connect to clifford server and BOX database
        string connectionString = "Server=clifford;Database=BOX;Trusted_Connection=True;";

        //Query to order emails by priority where most actionable phrases go first
        string query = @"SELECT *
							FROM [BOX].[dbo].[Classified_Emails]
							ORDER BY 
								CASE 
									WHEN Actionable_Phrase_10 IS NOT NULL THEN 1
									WHEN Actionable_Phrase_9 IS NOT NULL THEN 2
									WHEN Actionable_Phrase_8 IS NOT NULL THEN 3
									WHEN Actionable_Phrase_7 IS NOT NULL THEN 4
									WHEN Actionable_Phrase_6 IS NOT NULL THEN 5
									WHEN Actionable_Phrase_5 IS NOT NULL THEN 6
									WHEN Actionable_Phrase_4 IS NOT NULL THEN 7
									WHEN Actionable_Phrase_3 IS NOT NULL THEN 8
									WHEN Actionable_Phrase_2 IS NOT NULL THEN 9
									WHEN Actionable_Phrase_1 IS NOT NULL THEN 10
									WHEN Classification='General' THEN 11
									WHEN Classification='Spam' THEN 12
									ELSE 13
								END;
						";
        string prompt = "";

        //Delimiter to show where prompt ends and ChatGPT response begins
        string delimiter = new string('-', 100);

        //Load local ChatGPT model and prepare to run API Calls.
        var client = new HttpClient();
        string requestJson = "{\"model\": \"llama3.1:latest\", \"prompt\": " + JsonConvert.SerializeObject(prompt) + "}";
        string fileName = "";

        //Connecting to SQL servers
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            //Declare and initialize query that will be called in SQL
            SqlCommand command = new SqlCommand(query, connection);

            try
            {
                //Connect to SQL and execute SQL query command
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                //Iterate through every row loaded from query
                while (reader.Read())
                {
                    DateTime starting_datetime = DateTime.Now;
                    //Create prompt that will be sent to ChatGPT
                    prompt = $@"I am going to give you the contents of an email.
Summarize the content of this email using MORE THAN {reader["Content"].ToString().Length / 15} characters and 
LESS THAN {reader["Content"].ToString().Length / 10} characters. Do not use predicate nominatives. Remain
factual. Just start the summary without any introduction. Here are the contents:" + "\n\n" + reader["Content"];
                    requestJson = "{\"model\": \"llama3.1:latest\", \"prompt\": " + JsonConvert.SerializeObject(prompt) + "}";

                    //Pull the file name from the FilePath table. This will be the file name of the txt file
                    //that the prompt and summary of this email will be saved to.
                    fileName = reader["FilePath"].ToString().Split('\\').Last().Split('.').First();

                    //Print file name and datetime to compare how long each email takes to summarize
                    Console.WriteLine($"{fileName}.eml: {DateTime.Now}");

                    //Making API call to local chatgpt
                    var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                    var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate")
                    {
                        Content = content
                    };
                    using (var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var gpt_reader = new StreamReader(stream))
                    {
                        string line;
                        var fullResponse = new StringBuilder();

                        //Iterate through local chatgpt response and build full response as one big string
                        while ((line = await gpt_reader.ReadLineAsync()) != null)
                        {
                            try
                            {
                                var json = JObject.Parse(line);
                                var token = json["response"]?.ToString();
                                if (!string.IsNullOrEmpty(token))
                                {
                                    fullResponse.Append(token);
                                }

                                if (json["done"]?.ToObject<bool>() == true)
                                {
                                    break;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error parsing line: " + ex.Message);
                                // Optionally continue or break
                            }
                        }
                        DateTime ending_datetime = DateTime.Now;
                        //Write full prompt and summary to filename text file
                        File.WriteAllText($"C:/Code/C#/LocalChatGPT/Email_Summaries/{fileName}.eml.txt", $"{starting_datetime}\n{prompt}\n\n{delimiter}\n\n{ending_datetime}\n{fullResponse.ToString()}");
                    }
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
