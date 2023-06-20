using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Newtonsoft.Json;
using Octopus.Client;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace process_message;

public class Function
{

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {

    }
    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        // Log
        LambdaLogger.Log("Begin message processing...");

        // Get environment variables
        string octopusServerUrl = Environment.GetEnvironmentVariable("OCTOPUS_SERVER_URL");
        string octopusApiKey = Environment.GetEnvironmentVariable("OCTOPUS_API_KEY");
        string slackWebhookUrl = Environment.GetEnvironmentVariable("SLACK_WEBHOOK_URL");

        // Log
        LambdaLogger.Log(string.Format("Retrieved environment variables, Octopus Server Url: {0}...", octopusServerUrl));

        // Deserialize message JSON
        LambdaLogger.Log("Parsing message...");
        OctopusMessage octopusMessage = JsonConvert.DeserializeObject<OctopusMessage>(message.Body);
        LambdaLogger.Log(string.Format("Successfully parsed message JSON with {0} type ({1})", octopusMessage.EventType, octopusMessage.Timestamp));

        // Create Octopus client object
        LambdaLogger.Log("Creating server endpoint object ...");
        var endpoint = new OctopusServerEndpoint(octopusServerUrl, octopusApiKey);
        LambdaLogger.Log("Creating repository object...");
        var repository = new OctopusRepository(endpoint);
        LambdaLogger.Log("Creating client object ...");
        var client = new OctopusClient(endpoint);

        // Create repository for space
        string spaceId = octopusMessage.Payload.Event.SpaceId;
        LambdaLogger.Log(string.Format("Creating repository object for space: {0}...", spaceId));
        var space = repository.Spaces.Get(spaceId);
        Octopus.Client.IOctopusSpaceRepository repositoryForSpace = client.ForSpace(space);

        // Process event
        var payload = octopusMessage.Payload;
        var @event = payload.Event;
        var eventCategory = @event.Category;

        switch (eventCategory)
        {
            case "DeploymentFailed":
                ProcessFailure(payload);
                break;
            default:
                ProcessGeneralMessage(payload);
                break;
        }

        LambdaLogger.Log(string.Format("Completed processing of message {0} type ({1})", octopusMessage.EventType, octopusMessage.Timestamp));

        await Task.CompletedTask;
    }

    private void ProcessGeneralMessage(Payload payload)
    {
        LambdaLogger.Log(string.Format("Processing event {0} through general route", payload.Event.Category));
    }

    private void ProcessFailure(Payload payload)
    {
        LambdaLogger.Log(string.Format("Processing event {0} through failure route", payload.Event.Category));
    }
}
