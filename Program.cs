using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace ConsoleProgram
{
    //Tweet model containing 3 properties used to identify a tweet
    public class Tweet
    { 
        public string Id { get; set; }
        public string Stamp { get; set; }
        public string Text { get; set; }
    }

    //Main class used to execute search from "BAD API". Intention is to return all tweets from 2016-2017.
    public class assessment_RestAPI
    {
        //TODO: Declare Class Variables
        static HttpClient client = new HttpClient();

        private static string URL = "https://badapi.iqvia.io/swagger/";
        private static string endTime = "2017-12-31T23:59:59";
        private static string urlParameters = "/api/v1/Tweets?startDate=2016-01-01T00%3A00%3A01.271Z&endDate=2018-12-31T23%3A59%3A49.271Z";
        private static bool withinDateRange = true;

        private static Dictionary<string, Tweet> StoredTweets = new Dictionary<string, Tweet>();

        // TODO: Methods

        /* Desc: Used to setup initial connection and calls data retrieval method.
         * 
         * Args:  
         */        
        private static void setupClient()
        {
            //init url path and content type
            client.BaseAddress = new Uri(URL);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            GrabAndStoreTweets();
        }

        /* Desc: Used to gather response body content and extract into tweets.
         *       Stores tweets in dictionary.       
         * 
         * Args:  
         */
        private static void GrabAndStoreTweets()
        {
            string newStartDate = "";
            string year;

            do
            {
                HttpResponseMessage response = client.GetAsync(urlParameters).Result;           // grab response body

                if (response.IsSuccessStatusCode)                                                //Code 200:valid response, 400:invalid dates, 600:bad data
                {
                    var resultContent = response.Content.ReadAsStringAsync().Result;             //grab content
                    var listOfTweets = JsonConvert.DeserializeObject<List<Tweet>>(resultContent);//convert data to list of tweets

                    //check for new tweets, avoid duplicate attempts
                    foreach (var tweet in listOfTweets)
                    {
                        //extract year before converting to local time due to UTC offset. Avoid 2018 tweets.
                        year = tweet.Stamp.Substring(0, 4);

                        //exit if tweets are past 2017
                        if ((DateTime.Compare(Convert.ToDateTime(tweet.Stamp), Convert.ToDateTime(endTime)) > 0) || year.Contains("2018"))
                        {
                            withinDateRange = false;
                            Console.WriteLine("\n\n--------------------------------\n");
                            Console.WriteLine("Total Tweets Collected: " + StoredTweets.Count);
                            Console.WriteLine("Program Finished.");

                            break;
                        }
                        else
                        {
                            //dont store duplicate tweets
                            if (!StoredTweets.ContainsKey(tweet.Id))
                            {
                                StoredTweets.Add(tweet.Id, tweet);

                                //grab last tweet's timestamp to use as starting point for next round of tweets
                                newStartDate = tweet.Stamp;

                                //display added Tweet's Message + Timestamp to console.
                                Console.WriteLine("Tweet Message: " + tweet.Text + " \nTweet Date: " + tweet.Stamp + "\n");
                            }
                        }
                    }
                    //update URL string for new date collection
                    urlParameters = "/api/v1/Tweets?startDate=" + Convert.ToDateTime(newStartDate) + "&endDate=2018-12-31T23%3A59%3A49.271Z";
                }
                else
                {
                    Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
                }

            } while (withinDateRange);
        }
    
        static void Main(string[] args)
        {
            setupClient();
        }
    }
}