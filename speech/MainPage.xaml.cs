using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Microsoft.CognitiveServices.Speech;
using LiteDB;

namespace speech
{
    public partial class MainPage : ContentPage
    {
        SpeechRecognizer recognizer;
        IMicrophoneService micService;
        bool isTranscribing = false;
        bool anserFlag = false;
        List<string> japaneseList;
        List<string> englishList;
        int counter = default;

        public MainPage()
        {
            InitializeComponent();
            micService = DependencyService.Resolve<IMicrophoneService>();
            using (var database = new LiteDatabase("sample.db"))
            {
                var col = database.GetCollection<Post>("words");
                var lis = col.FindAll().OrderBy(x => new Guid(x.English));
                japaneseList = lis.Select(x => x.Japanese).ToList();
                englishList = lis.Select(x => x.English).ToList();
            }
        }

        async void TranscribeButton(Object sender, EventArgs e)
        {
            bool isMicEnabled = await micService.GetPermissionAsync();

            // EARLY OUT: make sure mic is accessible
            if (!isMicEnabled)
            {
                UpdateTranscription("Please grant access to the microphone!");
                return;
            }

            // initialize speech recognizer 
            if (recognizer == null)
            {
                var config = SpeechConfig.FromSubscription(Constants.Key, Constants.Region);
                recognizer = new SpeechRecognizer(config);
                recognizer.Recognized += (obj, args) =>
                {
                    UpdateTranscription(args.Result.Text);
                };
            }

            // if already transcribing, stop speech recognizer
            if (isTranscribing)
            {
                try
                {
                    await recognizer.StopContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
                    UpdateTranscription(ex.Message);
                }
                isTranscribing = false;
            }

            // if not transcribing, start speech recognizer
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    InsertDateTimeRecord();
                });
                try
                {
                    await recognizer.StartContinuousRecognitionAsync();
                }
                catch (Exception ex)
                {
                    UpdateTranscription(ex.Message);
                }
                isTranscribing = true;
            }
            UpdateDisplayState();
        }

        void UpdateTranscription(string newText)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    transcribedText.Text += $"{newText}\n";
                }
            });
        }

        void InsertDateTimeRecord()
        {
            var msg = $"=================\n{DateTime.Now.ToString()}\n=================";
            UpdateTranscription(msg);
        }

        void UpdateDisplayState()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if(isTranscribing)
                {
                    transButton.Text = "Stop";
                    transButton.BorderColor = Color.Red;
                    transcribingIndicator.IsRunning = true;
                }
                else
                {
                    transButton.Text = "Transcribe";
                    transButton.BackgroundColor = Color.Green;
                    transcribingIndicator.IsRunning = false;
                }
            });
        }
    }

    public class Post
    {
        public int ID { get; set; }
        public string Japanese { get; set; }
        public string English { get; set; }
    }
}
