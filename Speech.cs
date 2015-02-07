using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Speech.Synthesis;

namespace Harley
{
    class Speech
    {
        /// <summary>
        /// Speech synthesizer object
        /// </summary>
        private SpeechSynthesizer speechSynthesizer;

        /// <summary>
        /// Constructor
        /// </summary>
        public Speech()
        {
            speechSynthesizer = new SpeechSynthesizer();
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
    }
}
