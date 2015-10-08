using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using SteamKit2;


namespace SteamBot
{
    //static class Login
    //{
    public class Login : Form1
    {
        static SteamClient steamClient;

        static CallbackManager manager;

        static SteamUser steamUser;

        static bool isRunning;

        static string user, pass;

        static ToolStripLabel tlb;

        static string authCode, twoFactorAuth;

        // static string toptlb;

       // public static string steamgg(string code)
       // {
       //     return code;
       // }

        public static void Enter_(string us, string pa, ToolStripLabel tlb_)
        {
            user = us;
            pass = pa;
            tlb = tlb_;
            if ((user.Length < 2) || (pass.Length < 2))
            {
                tlb.Text = "No username and password specified!";
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

            // this callback is triggered when the steam servers wish for the client to store the sentry file
            //manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            isRunning = true;

            tlb.Text = "Connecting to Steam...";


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
                tlb.Text = "Unable to connect to Steam";
                isRunning = false;
                return;
            }

            tlb.Text = "Connected to Steam! Logging in";

            byte[] sentryHash = null;
            if (File.Exists("sentry.bin"))
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes("sentry.bin");
                sentryHash = CryptoHelper.SHAHash(sentryFile);
            }

            steamUser.LogOn(new SteamUser.LogOnDetails
            {
                Username = user,
                Password = pass,

                // in this sample, we pass in an additional authcode
                // this value will be null (which is the default) for our first logon attempt
                AuthCode = authCode,

                // if the account is using 2-factor auth, we'll provide the two factor code instead
                // this will also be null on our first logon attempt
                TwoFactorCode = twoFactorAuth,

                // our subsequent logons use the hash of the sentry file as proof of ownership of the file
                // this will also be null for our first (no authcode) and second (authcode only) logon attempts
                SentryFileHash = sentryHash,
            });
        }

        static void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            tlb.Text = "Disconnected from Steam, reconnecting...";

            Thread.Sleep(TimeSpan.FromSeconds(5));

            isRunning = false;

            steamClient.Connect();
        }

        static void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
           
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;

            if (isSteamGuard || is2FA)
            {
                sgenter sg = new sgenter();
                tlb.Text = "This account is SteamGuard protected!";
                sg.Show();
                if (is2FA)
                {
                   // sg.button1.OnClientClick()

                    tlb.Text = "Please enter your 2 factor auth code from your authenticator app: ";
                    twoFactorAuth = sg.textBox1.Text;
                }
                else
                {
                    tlb.Text = "Please enter the auth code sent to the email";
                    authCode = sg.textBox1.Text;
                    return;
                }

            }
        
                if (callback.Result != EResult.OK)
                {
                    //if (callback.Result == EResult.AccountLogonDenied)
                    // {
                    // if we recieve AccountLogonDenied or one of it's flavors (AccountLogonDeniedNoMailSent, etc)
                    // then the account we're logging into is SteamGuard protected
                    // see sample 5 for how SteamGuard can be handled

                    //   tlb.Text ="Unable to logon to Steam: This account is SteamGuard protected.";

                    //  isRunning = false;
                    // return;
                    // }

                    tlb.Text = "Unable to logon to Steam";

                    isRunning = false;
                    return;
                }

                tlb.Text = "Successfully logged on!";

                // at this point, we'd be able to perform actions on Steam

                // for this sample we'll just log off
                steamUser.LogOff();
            }
       // }

        static void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            tlb.Text = "Logged off of Steam";
        }

        static void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            tlb.Text = "Updating sentryfile...";

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"

            int fileSize;
            byte[] sentryHash;
            using (var fs = File.Open("sentry.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using (var sha = new SHA1CryptoServiceProvider())
                {
                    sentryHash = sha.ComputeHash(fs);
                }
            }

            // inform the steam servers that we're accepting this sentry file
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            tlb.Text ="Done!";
        }
    }
    }
//}