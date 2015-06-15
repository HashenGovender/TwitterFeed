using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterFeed
{
    class Program
    {
        // Declaration of structures used to hold the user and tweet data
        private static SortedSet<string> users; // Using a SortedSet rather than a List for performance advantages when comparing items
        private static HashSet<Tuple<string, string>> userFollowers; // Using a HashSet rather than a List for performance advantages when checking for (CONTAINS) item
        private static List<Tuple<string, string>> tweets;

        private static string usersFileName;
        private static string tweetsFileName;

        static void Main(string[] args)
        {
            //Check if user and tweet filename arguments are present
            if (args.Length < 2)
            {
                Console.WriteLine("Please specify the path and filename of the user and tweet txt files as arguments 1 and 2 respectively");
                Console.WriteLine("USAGE: TwitterFeed.exe User_TXT_FileName Tweet_TXT_FileName");
                Console.WriteLine();
                Environment.Exit(0);
            }

            //Instantite local variables to store user and tweet filenames
            usersFileName = args[0];
            tweetsFileName = args[1];

            //Instantiate data storage structures for users, followers and tweets
            users = new SortedSet<string>();
            userFollowers = new HashSet<Tuple<string, string>>();
            tweets = new List<Tuple<string, string>>();

            //Read user data
            ReadUsers();

            //Read tweet data
            ReadTweets();

            //Output formatted twitter feed
            DisplayTwitterFeed();
        }


        //Read user file and copy user data into Set structures
        private static void ReadUsers()
        {
            try
            {
                int curLine = 0;
                string followsSeperator = " follows ";
                int followsSeperatorLength = followsSeperator.Length;

                //Read user file one line at a time
                foreach (string line in File.ReadLines(usersFileName))
                {
                    curLine++;

                    int indFollowsSeperator = line.IndexOf(followsSeperator);
                    if (indFollowsSeperator > 0)
                    {
                        string user = line.Substring(0, indFollowsSeperator);

                        users.Add(user);

                        string followers = line.Substring(indFollowsSeperator + followsSeperatorLength);
                        string[] followersSeperator = { ", " };

                        foreach (string follower in followers.Split(followersSeperator, StringSplitOptions.RemoveEmptyEntries))
                        {
                            users.Add(follower);
                            userFollowers.Add(new Tuple<string, string>(user, follower));
                        }
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Error parsing line {0} in user file. Could not find \"follows\" keyword", curLine));
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Could not read from user file: \"{0}\". Please ensure this file exists and is readable", usersFileName));
                Environment.Exit(0);
            }
        }
        

        //Read tweet file and copy tweets into List structure
        private static void ReadTweets()
        {
            try
            {
                int curLine = 0;
                string userTweetSeperator = "> ";
                int userTweetSeperatorLength = userTweetSeperator.Length;

                //Read tweet file one line at a time
                foreach (string line in File.ReadLines(tweetsFileName))
                {
                    curLine++;

                    int indUserTweetSeperator = line.IndexOf(userTweetSeperator);
                    if (indUserTweetSeperator > 0)
                    {
                        string user = line.Substring(0, indUserTweetSeperator);
                        string tweet = line.Substring(indUserTweetSeperator + userTweetSeperatorLength);

                        if (string.IsNullOrEmpty(tweet)) {
                            Console.WriteLine(string.Format("Error parsing line {0} in tweet file. Tweet is empty", curLine));
                            Environment.Exit(0);
                        }
                        else if (tweet.Length > 140)
                        {
                            Console.WriteLine(string.Format("Error parsing line {0} in tweet file. Tweet is greater than 140 characters", curLine));
                            Environment.Exit(0);
                        }

                        tweets.Add(new Tuple<string, string>(user, tweet));
                    }
                    else
                    {
                        Console.WriteLine(string.Format("Error parsing line {0} in tweet file. Could not find \"> \" seperator", curLine));
                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Could not read from tweet file: \"{0}\". Please ensure this file exists and is readable", tweetsFileName));
                Environment.Exit(0);
            }
        }


        //Read data structures and output twitter feed per user to the console
        private static void DisplayTwitterFeed()
        {
            try
            {
                //Iterate through the users
                foreach (string usr in users)
                {
                    //Output user name to the console
                    Console.WriteLine(usr);

                    //Iterate through the tweets
                    foreach (Tuple<string, string> twt in tweets)
                    {
                        //Determine whether to show tweet under this user's feed (if tweet is from the user or from a person they are following)
                        if (twt.Item1 == usr || userFollowers.Contains(new Tuple<string, string>(usr, twt.Item1)))
                        {
                            //Output tweet to the console formatted appropriately
                            Console.WriteLine(string.Format("\t@{0}: {1}", twt.Item1, twt.Item2));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured trying to display twitter feed");
                Environment.Exit(0);
            }
        }
    }
}
