using System;
using System.Windows.Forms;
using SteamKit2;


namespace SteamBot
{
    //static class Login
    //{
       public  class Login : Form1
        {
            static SteamClient steamClient;
            static CallbackManager manager;

            static SteamUser steamUser;

            static bool isRunning;

            static string user, pass;


            public static void Enter_(string us, string pa)
            {
              //  user = this.user;
               // pass = this.pass;

                // save our logon details
              //  var user = getlogin;
              //  var pass = textBox2.Text;
                user = us;
                pass = pa;
                MessageBox.Show(user, pass);

                if ((user.Length < 2) || (pass.Length < 2))
                {
                    MessageBox.Show("Sample1: No username and password specified!");
                    return;
                }

                // create our steamclient instance
                steamClient = new SteamClient();
                // create the callback manager which will route callbacks to function calls
                manager = new CallbackManager(steamClient);

                // get the steamuser handler, which is used for logging on after successfully connecting
                steamUser = steamClient.GetHandler<SteamUser>();

                // register a few callbacks we're interested in
                // these are registered upon creation to a callback manager, which will then route the callbacks
                // to the functions specified
                manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
                manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

                manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
                manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

                isRunning = true;

                MessageBox.Show("Connecting to Steam...");

                // initiate the connection
                steamClient.Connect();

                // create our callback handling loop
                while (isRunning)
                {
                    // in order for the callbacks to get routed, they need to be handled by the manager
                    manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                }
            }

            static void OnConnected(SteamClient.ConnectedCallback callback)
            {
                if (callback.Result != EResult.OK)
                {
                    MessageBox.Show("Unable to connect to Steam");

                    isRunning = false;
                    return;
                }

                MessageBox.Show("Connected to Steam! Logging in", user);

                steamUser.LogOn(new SteamUser.LogOnDetails
                {
                    Username = user,
                    Password = pass,
                });
            }

            static void OnDisconnected(SteamClient.DisconnectedCallback callback)
            {
                MessageBox.Show("Disconnected from Steam");

                isRunning = false;
            }

            static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
            {
                if (callback.Result != EResult.OK)
                {
                    if (callback.Result == EResult.AccountLogonDenied)
                    {
                        // if we recieve AccountLogonDenied or one of it's flavors (AccountLogonDeniedNoMailSent, etc)
                        // then the account we're logging into is SteamGuard protected
                        // see sample 5 for how SteamGuard can be handled

                        MessageBox.Show("Unable to logon to Steam: This account is SteamGuard protected.");

                        isRunning = false;
                        return;
                    }

                    MessageBox.Show("Unable to logon to Steam");

                    isRunning = false;
                    return;
                }

                MessageBox.Show("Successfully logged on!");

                // at this point, we'd be able to perform actions on Steam

                // for this sample we'll just log off
                steamUser.LogOff();
            }

            static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
            {
                MessageBox.Show("Logged off of Steam");
            }
        }
    }
//}