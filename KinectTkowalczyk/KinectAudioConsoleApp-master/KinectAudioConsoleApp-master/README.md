KinectAudioConsoleApp
=====================

This simple application shows how to properly use [Kinect for Windows SDK](http://www.microsoft.com/en-us/kinectforwindows/ "Kinect for Windows SDK") with the Speech Recognition engine from MSFT.

Before use code from this example you have to be sure that you have installed [Microsoft Speech Platform](http://www.microsoft.com/en-us/download/details.aspx?id=27226 "Microsoft Speech Platform").

**How does it work?**

First off all we have to add following usings:

`using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;`

Declare important objects:

`KinectSensor _sensor;
            SpeechRecognitionEngine _sre;`

Find sensor connected to your computer using `LINQ`:

`_sensor = (from sensorToCheck in KinectSensor.KinectSensors
                       where sensorToCheck.Status == KinectStatus.Connected
                       select sensorToCheck).FirstOrDefault();`

Assign audio source:

`var audioSource = _sensor.AudioSource;`

and use it as follow:

`using (var source = audioSource.Start())`

Set important properties and call `CreateSpeechRecognizer()` method where speech grammar is defined:

`audioSource.EchoCancellationMode = EchoCancellationMode.None;`
`audioSource.AutomaticGainControlEnabled = false;`

`_sre = CreateSpeechRecognizer();`

In the `CreateSpeechRecognizer()` method we call `GetKinectRecognizer()` where we have to define which language we are going to use in our application:

`RecognizerInfo ri = GetKinectRecognizer();`

Now we are able to build and define grammar:

`var grammar = new Choices();`
            `grammar.Add("red");
            grammar.Add("green");
            grammar.Add("blue");`

            var gb = new GrammarBuilder { Culture = ri.Culture };
            gb.Append(grammar);

            var g = new Grammar(gb);`

After this we can load this grammar to speech recognizer and define some event methods for recognized or rejeced words:

`sre.LoadGrammar(g);
            sre.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);
            sre.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(sre_SpeechHypothesized);
            sre.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(sre_SpeechRecognitionRejected);`

Now we can test this simple example, to do this just say one of three colors defined above.

**More examples**

Feel free to visit my homepage [Tomasz Kowalczyk](http://tomek.kownet.info/ "Tomasz Kowalczyk") to see more complex examples.