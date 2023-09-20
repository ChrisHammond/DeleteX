using System.Diagnostics;
using LinqToTwitter;
using LinqToTwitter.OAuth;
using System.Timers;
using Timer = System.Timers.Timer;

//need to install the following via command line
// dotnet add package LinqToTwitter

class DeleteX
{
    //create your application via developers.twitter.com
    //Under the oauth 2.0 section be sure to request read/write access
    //generate the auth tokens.
    private static string twitterApiKey = ""; // Here you need to put your Twitter Developer API Key
    private static string twitterApiKeySecret = ""; // Here you need to put your Twitter Developer API Key Secret
    private static string accessToken = ""; // Access Token 
    private static string accessTokenSecret = ""; // Access Token

    //No need to change these paths unless you put files somewhere else.
    private static readonly string deletedTweetsPath = Path.Combine(Directory.GetCurrentDirectory(), "deletedTweets.txt");
    private static readonly string tweetsPath = Path.Combine(Directory.GetCurrentDirectory(), "tweets.csv");

    private static string[] tweetIds;
    private static int currentIndex = 0;

    private static TwitterContext twitterCtx;

    static void Main()
    {
        if (!InitializeTwitterContextFromSavedCredentials())
        {
            Console.WriteLine("Could not authenticate with saved credentials. Did you forget to populate them? \n\n\nGo back and edit Program.cs to populate them.");
            return;            
        }

        // Load tweet IDs from the file
        tweetIds = File.ReadAllLines(tweetsPath);

        // Load deleted tweet list
        var deletedTweets = new HashSet<string>(File.ReadAllLines(deletedTweetsPath));

        // Remove already deleted tweets from our list, no need to attempt to delete them again
        tweetIds = tweetIds.Where(id => !deletedTweets.Contains(id)).ToArray();

        // Set up a timer to delete tweets every 15 seconds
        Timer timer = new Timer(5000);
        timer.Elapsed += async (sender, e) => await DeleteTweet(sender, e);
        timer.Start();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static bool InitializeTwitterContextFromSavedCredentials()
    {

        if (twitterApiKey == string.Empty || twitterApiKeySecret == string.Empty || accessToken == string.Empty || accessTokenSecret == string.Empty)
            return false;

        var authorizer = new SingleUserAuthorizer
        {
            CredentialStore = new SingleUserInMemoryCredentialStore
            {
                ConsumerKey = twitterApiKey, 
                ConsumerSecret = twitterApiKeySecret, 
                AccessToken = accessToken,
                AccessTokenSecret = accessTokenSecret
            }
        };

        twitterCtx = new TwitterContext(authorizer);
        return true;
    }

    static async Task DeleteTweet(object sender, ElapsedEventArgs e)
    {
        if (currentIndex >= tweetIds.Length)
        {
            Console.WriteLine("All tweets processed! Press any key to stop the program.");
            return;
        }

        var tweetId = tweetIds[currentIndex];

        if (long.TryParse(tweetId, out long res))
        {
                    // Call the Twitter API to delete the tweet
            bool success = await DeleteTweetViaAPI(tweetId);
            if (success)
            {
                // Log the deleted tweet ID
                File.AppendAllText(deletedTweetsPath, tweetId + Environment.NewLine);
                Console.WriteLine($"Deleted tweet with ID {tweetId}");
            }
            else
            {
                Console.WriteLine($"Failed to delete tweet with ID {tweetId}");
                //if you want to go ahead and log this one as deleted, uncomment the next line. Might make sense to throw this into a separate file
                //File.AppendAllText(deletedTweetsPath, tweetId + Environment.NewLine);
            }
        }
        else
        {
            Console.WriteLine($"Failed to delete tweet with ID {tweetId}");
        }


        currentIndex++;
    }

    static async Task<bool> DeleteTweetViaAPI(string tweetId)
    {
        try
        {
            if (twitterCtx != null)
            { 
                var tweet = await twitterCtx.DeleteTweetAsync(tweetId);

                if (tweet != null)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting tweet: {ex.Message}");
            return false;
        }
    }

}
