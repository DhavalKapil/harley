using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Speech.Synthesis;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

namespace Harley
{
    class Speech
    {
        /// <summary>
        /// The kinect sensor object
        /// </summary>
        private KinectSensor kinectSensor;

        /// <summary>
        /// Speech synthesizer object
        /// </summary>
        private SpeechSynthesizer speechSynthesizer;

        /// <summary>
        /// Speech recognizer engine
        /// </summary>
        private SpeechRecognitionEngine speechRecognizer;

        /// <summary>
        /// List of grammer that are recognizable
        /// </summary>
        private string[] grammarList = {"ready", "go"};

        /// <summary>
        /// Constructor
        /// </summary>
        public Speech(KinectSensor kinectSensor)
        {
            speechSynthesizer = new SpeechSynthesizer();

            this.kinectSensor = kinectSensor;

            speechRecognizer = CreateSpeechRecognizer();
        }

        /// <summary>
        /// Synchronous function to convert text to speech
        /// </summary>
        /// <param name="text"></param>
        public void Speak(String text)
        {
            speechSynthesizer.Speak(text);
        }

        /// <summary>
        /// Asynchronous function to convert text to speech
        /// </summary>
        /// <param name="text"></param>
        public void SpeakAsync(String text)
        {
            speechSynthesizer.SpeakAsync(text);
        }

        /// <summary>
        /// Function to stop existing narration
        /// </summary>
        public void StopSpeak()
        {
            if(speechSynthesizer!=null)
            {
                speechSynthesizer.Dispose();
            }
        }

        /// <summary>
        /// Function to return RecognizerInfo
        /// </summary>
        /// <returns></returns>
        private static RecognizerInfo GetKinectRecognizer()
        {
            Func<RecognizerInfo, bool> matchingFunc = r =>
            {
                string value;
                r.AdditionalInfo.TryGetValue("Kinect", out value);
                return "True".Equals(value, StringComparison.InvariantCultureIgnoreCase) && "en-US".Equals(r.Culture.Name, StringComparison.InvariantCultureIgnoreCase);
            };
            return SpeechRecognitionEngine.InstalledRecognizers().Where(matchingFunc).FirstOrDefault();
        }

        /// <summary>
        /// Function to start streaing audio
        /// </summary>
        public void Start()
        {
            //set sensor audio source to variable
            var audioSource = kinectSensor.AudioSource;

            //Set the beam angle mode - the direction the audio beam is pointing
            //we want it to be set to adaptive
            audioSource.BeamAngleMode = BeamAngleMode.Adaptive;

            //start the audiosource 
            var kinectStream = audioSource.Start();

            //configure incoming audio stream
            speechRecognizer.SetInputToAudioStream(
                kinectStream, new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));

            //make sure the recognizer does not stop after completing     
            speechRecognizer.RecognizeAsync(RecognizeMode.Multiple);

            //reduce background and ambient noise for better accuracy
            kinectSensor.AudioSource.EchoCancellationMode = EchoCancellationMode.None;
            kinectSensor.AudioSource.AutomaticGainControlEnabled = false;
        }

        /// <summary>
        /// Function to create SpeechRecognizer
        /// </summary>
        /// <returns></returns>
        private SpeechRecognitionEngine CreateSpeechRecognizer()
        {
            //set recognizer info
            RecognizerInfo ri = GetKinectRecognizer();
            //create instance of SRE
            SpeechRecognitionEngine sre;
            sre = new SpeechRecognitionEngine(ri.Id);

            //Now we need to add the words we want our program to recognise
            var grammar = new Choices();
            foreach (var gm in grammarList)
            {
                grammar.Add(gm);
            }

            //set culture - language, country/region
            var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(grammar);

            //set up the grammar builder
            var g = new Grammar(gb);
            sre.LoadGrammar(g);

            //Set events for recognizing, hypothesising and rejecting speech
            sre.SpeechRecognized += SreSpeechRecognized;
            sre.SpeechHypothesized += SreSpeechHypothesized;
            sre.SpeechRecognitionRejected += SreSpeechRecognitionRejected;
            return sre;
        }

        /// <summary>
        /// Function called when speech is rejected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SreSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            Trace.WriteLine("Rejected: " + e.Result);
        }

        /// <summary>
        /// Function called when speech is hypothesized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SreSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Trace.WriteLine("Hypothesized: " + e.Result.Text + " " + e.Result.Confidence);
        }

        /// <summary>
        /// Called when the speech is recognized
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SreSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //Very important! - change this value to adjust accuracy - the higher the value
            //the more accurate it will have to be, lower it if it is not recognizing you
            if (e.Result.Confidence < .4)
            {
                Trace.WriteLine("Rejected: " + e.Result);
            }

            //and finally, here we set what we want to happen when 
            //the SRE recognizes a word
            Trace.WriteLine(e.Result.Text);
        }
    }
}
