﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterFeed
{
    class Program
    {
        // Declaration of structures used to hold the user, follower and tweet data
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
                return;
            }

            //Instantite variables to store user and tweet filenames
            usersFileName = args[0];
            tweetsFileName = args[1];

            //Instantiate data storage structures for users, followers and tweets
            users = new SortedSet<string>();
            userFollowers = new HashSet<Tuple<string, string>>();
            tweets = new List<Tuple<string, string>>();

            //Read user data
            if (!ReadUsers())
                return;

            //Read tweet data
            if (!ReadTweets())
                return;

            //Output formatted twitter feed
            if (!DisplayTwitterFeed())
                return;
        }


        //Read user file and copy user data into Set structures
        private static bool ReadUsers()
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

                    //Try and locate the index of the "follows" keyword seperator
                    int indFollowsSeperator = line.IndexOf(followsSeperator);

                    //If seperator is found and there is at least one character before the seperator process the line
                    if (indFollowsSeperator > 0)
                    {
                        //Extract the user name from the line
                        string user = line.Substring(0, indFollowsSeperator);

                        //Add the user to the users sortedset, duplicates will be ignored
                        users.Add(user);

                        //Extract the followers from the rest of the line after the seperator
                        string followers = line.Substring(indFollowsSeperator + followsSeperatorLength);
                        string[] followersSeperator = { ", " };

                        //Split up the followers string using the followersSeperator and iterate over
                        foreach (string follower in followers.Split(followersSeperator, StringSplitOptions.RemoveEmptyEntries))
                        {
                            //Add follower as a user to the users sortedset, duplicates will be ignored
                            users.Add(follower);
                            //Add user-follower entry to the userFollowers HashSet, duplicates will be ignored
                            userFollowers.Add(new Tuple<string, string>(user, follower));
                        }
                    }
                    else
                    {
                        //If user name is blank (whitespace), display error
                        if (indFollowsSeperator == 0)
                        {
                            Console.WriteLine(string.Format("Error parsing line {0} in user file. User name is empty", curLine));
                        }
                        //If user seperator not found, display error
                        else
                        {
                            Console.WriteLine(string.Format("Error parsing line {0} in user file. Could not find \" follows \" keyword", curLine));
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Could not read from user file: \"{0}\". Please ensure this file exists and is readable", usersFileName));
                return false;
            }

            return true;
        }
        

        //Read tweet file and copy tweets into List structure
        private static bool ReadTweets()
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

                    //Try and locate the index of the "> " keyword seperator
                    int indUserTweetSeperator = line.IndexOf(userTweetSeperator);

                    //If seperator is found and there is at least one character before the seperator process the line
                    if (indUserTweetSeperator > 0)
                    {
                        //Extract the user name from the line
                        string user = line.Substring(0, indUserTweetSeperator);
                        //Extract the tweet text from the line
                        string tweet = line.Substring(indUserTweetSeperator + userTweetSeperatorLength);

                        //If tweet text is empty, display error
                        if (string.IsNullOrEmpty(tweet)) {
                            Console.WriteLine(string.Format("Error parsing line {0} in tweet file. Tweet is empty", curLine));
                            return false;
                        }
                        //If tweet text is greater than 140 chars display error
                        else if (tweet.Length > 140)
                        {
                            Console.WriteLine(string.Format("Error parsing line {0} in tweet file. Tweet is greater than 140 characters", curLine));
                            return false;
                        }

                        //add user-tweet entry to the tweets list
                        tweets.Add(new Tuple<string, string>(user, tweet));
                    }
                    else
                    {
                        //If user name is blank (whitespace), display error
                        if (indUserTweetSeperator == 0)
                        {
                            Console.WriteLine(string.Format("Error parsing line {0} in tweet file. User name is empty", curLine));
                        }
                        //If user seperator not found, display error
                        else
                        {
                            Console.WriteLine(string.Format("Error parsing line {0} in tweet file. Could not find \"> \" seperator", curLine));
                        }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Could not read from tweet file: \"{0}\". Please ensure this file exists and is readable", tweetsFileName));
                return false;
            }

            return true;
        }


        //Read data structures and output twitter feed per user to the console
        private static bool DisplayTwitterFeed()
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
                return false;
            }

            return true;
        }
    }
}
