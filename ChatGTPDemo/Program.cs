using ChatGTPDemo;
using ChatGTPDemo.Models;
using Spectre.Console;
using Microsoft.CognitiveServices.Speech;

var chatGPTClient = new ChatGPTClient();
var chatActive = true;

// Load an image
var image = new CanvasImage("openai_logo.png");

// Set the max width of the image.
// If no max width is set, the image will take
// up as much space as there is available.
image.MaxWidth(20);

// Render the image to the console
//AnsiConsole.Write(image);

Console.WriteLine();

AnsiConsole.Write(
    new FigletText("Gleason Chat")
        .LeftJustified()
        .Color(Color.NavajoWhite1));

Console.WriteLine();

AnsiConsole.MarkupLine("[#ffffff]Powered by ChatGPT and Microsoft Cognitive Services[/]");
AnsiConsole.MarkupLine("[Grey]Experimenting with ChartGPT And Speech APIs in a C# console application[/]");

Console.WriteLine();

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("OpenAIAPIKey")))
{
    AnsiConsole.MarkupLine("[#af0000]The OpenAPI key is not specified.[/]");
    AnsiConsole.MarkupLine("[#af0000]Please add the key in your project in the[/] [red]environment variable[/] [#af0000]section.[/]");
    return;
}

SynthesisToSpeakerAsync("Welcome to Gleason Chat. Ask me anything!").Wait();

var rule = new Rule();
while (chatActive)
{
    //AnsiConsole.Write(rule);
    //var userMessage = AnsiConsole.Prompt(new TextPrompt<string>("[Grey]What is your [/][green]question[/][Grey]?[/]").AllowEmpty());
    //AnsiConsole.MarkupLine("[Grey]What is your [/][green]question[/][Grey]?[/]");
    //SynthesisToSpeakerAsync("What else can I help you with?").Wait();

    var userMessage = RecognitionWithMicrophoneAsync().Result;

    if (string.IsNullOrEmpty(userMessage))
    {
        //AnsiConsole.WriteLine("Unrecognized input or timed out");
        continue;
    }

    AnsiConsole.WriteLine(userMessage);

    if (userMessage.ToLowerInvariant() == "quit.")
    {
        chatActive = false;
        continue;
    }
    else
    {
        ChatResponse chatResponse = null;

        try
        {
            AnsiConsole.Status()
                       .Spinner(Spinner.Known.Balloon)
                       .Start("Thinking...", ctx =>
                       {
                           chatResponse = chatGPTClient.SendMessage(userMessage).Result;
                       });
        }
        catch { }

        if (chatResponse != null)
        {
            Console.WriteLine();

            
            foreach (var assistantMessage in chatResponse.Choices!.Select(c => c.Message))
            {
                if (assistantMessage.Content != null)
                {
                    AnsiConsole.WriteLine(assistantMessage.Content);

                    var chunks = assistantMessage.Content.Split("```");
                    if (chunks.Length == 3)
                    {
                        SynthesisToSpeakerAsync(chunks[0]).Wait();
                        SynthesisToSpeakerAsync(chunks[2]).Wait();
                    }
                    else
                        SynthesisToSpeakerAsync(assistantMessage.Content).Wait();
                }
            }

            //var table = new Table();
            //AnsiConsole.Live(table)
            //           .Start(ctx =>
            //           {
            //               table.AddColumn("[yellow]Answer[/]");
            //               ctx.Refresh();

            //               foreach (var assistantMessage in chatResponse.Choices!.Select(c => c.Message))
            //               {
            //                   table.AddRow(new Markup("[Grey]" + assistantMessage!.Content!.Trim().Replace("\n", "") + "[/]"));
            //                   SynthesisToSpeakerAsync(assistantMessage.Content);
            //                   ctx.Refresh();
            //               }
            //           });

            Console.WriteLine();
        }
        else
        {
            var errMessage = "I received an error when calling the ChatGPT API. Please try again.";
            AnsiConsole.MarkupLine($"[#af0000]{errMessage}[/]");
            SynthesisToSpeakerAsync(errMessage).Wait();
            //chatActive = false;
        }
    }
}

Console.WriteLine();

var endMessage = "It was very nice chatting with you. Goodbye";
rule = new Rule($"[Grey]{endMessage}[/]");
rule.RuleStyle("Grey dim");
AnsiConsole.Write(rule);
SynthesisToSpeakerAsync(endMessage).Wait();


static async Task SynthesisToSpeakerAsync(string text)
{
    string SubscriptionKey = "40b30b3901ba47d1af253c5f5413ca68";
    string ServiceRegion = "eastus";

    // Creates an instance of a speech config with specified subscription key and service region.
    // Replace with your own subscription key and service region (e.g., "westus").
    // The default language is "en-us".
    var config = SpeechConfig.FromSubscription(SubscriptionKey, ServiceRegion);
    config.SpeechSynthesisVoiceName = "en-US-JasonNeural";

    // Creates a speech synthesizer using the default speaker as audio output.
    using (var synthesizer = new SpeechSynthesizer(config))
    {
       // while (true)
        {
            // Receives a text from console input and synthesize it to speaker.
            //Console.WriteLine("Enter some text that you want to speak, or enter empty text to exit.");
            //Console.Write("> ");
            //string text = Console.ReadLine();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            using (var result = await synthesizer.SpeakTextAsync(text))
            {
                if (result.Reason == ResultReason.SynthesizingAudioCompleted)
                {
                    //Console.WriteLine($"Speech synthesized to speaker for text [{text}]");
                }
                else if (result.Reason == ResultReason.Canceled)
                {
                    //var cancellation = SpeechSynthesisCancellationDetails.FromResult(result);
                    //Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

                    //if (cancellation.Reason == CancellationReason.Error)
                    //{
                    //    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    //    Console.WriteLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                    //    Console.WriteLine($"CANCELED: Did you update the subscription info?");
                    //}
                }
            }
        }
    }
}

static async Task<string> RecognitionWithMicrophoneAsync()
{
    string SubscriptionKey = "40b30b3901ba47d1af253c5f5413ca68";
    string ServiceRegion = "eastus";
    string Result = string.Empty;

    // <recognitionWithMicrophone>
    // Creates an instance of a speech config with specified subscription key and service region.
    // Replace with your own subscription key and service region (e.g., "westus").
    // The default language is "en-us".
    //var config = SpeechConfig.FromSubscription(SubscriptionSettings.SubscriptionKey, SubscriptionSettings.ServiceRegion);
    var config = SpeechConfig.FromSubscription(SubscriptionKey, ServiceRegion);
    //config.OutputFormat = OutputFormat.Detailed;

    //config.SetProperty(PropertyId.Speech_SegmentationSilenceTimeoutMs, "2000");

    // Creates a speech recognizer using microphone as audio input.
    using (var recognizer = new SpeechRecognizer(config))
    {
        // Starts recognizing.
        //Console.WriteLine("Say something...");

        // Starts speech recognition, and returns after a single utterance is recognized. The end of a
        // single utterance is determined by listening for silence at the end or until a maximum of 15
        // seconds of audio is processed.  The task returns the recognition text as result.
        // Note: Since RecognizeOnceAsync() returns only a single utterance, it is suitable only for single
        // shot recognition like command or query.
        // For long-running multi-utterance recognition, use StartContinuousRecognitionAsync() instead.
        var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

        // Checks result.
        if (result.Reason == ResultReason.RecognizedSpeech)
        {
            //Console.WriteLine($"RECOGNIZED: Text={result.Text}");
            Result = result.Text;
        }
        else if (result.Reason == ResultReason.NoMatch)
        {
            //Console.WriteLine($"NOMATCH: Speech could not be recognized.");
        }
        else if (result.Reason == ResultReason.Canceled)
        {
            //var cancellation = CancellationDetails.FromResult(result);
            //Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

            //if (cancellation.Reason == CancellationReason.Error)
            //{
            //    Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
            //    Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
            //    Console.WriteLine($"CANCELED: Did you update the subscription info?");
            //}
        }
    }

    return Result;
}