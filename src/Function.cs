using System.Text.Json;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace demo;

public class Function
{
    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    private readonly IAmazonSimpleNotificationService _amazonSimpleNotificationService;
    private readonly string _topicArn;

    public Function()
    {
        _amazonSimpleNotificationService = new AmazonSimpleNotificationServiceClient();
        _topicArn = "arn:aws:sns:ap-south-1:050451398880:demo-UploadsNotificationTopic";
    }


    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="evnt">The event for the Lambda function handler to process.</param>
    /// <param name="context">The ILambdaContext that provides methods for logging and describing the Lambda environment.</param>
    /// <returns></returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach(var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        context.Logger.LogInformation($"Processed message {message.Body}");

        var metadata = JsonSerializer.Deserialize<ImageMetadata>(message.Body);

        var plainText = $"Image uploaded.\n\n" +
                        $"Name: {metadata.OriginalName}\n"; //+
                        //$"Saved As: {metadata.Name}\n" +
                        //$"Size: {metadata.Size} bytes\n" +
                        //$"Extension: {metadata.Extension}\n" +
                        //$"Download: {metadata.DownloadUrl}";

        await _amazonSimpleNotificationService.PublishAsync(new PublishRequest
        {
            TopicArn = _topicArn,
            Message = plainText
        });

        // TODO: Do interesting work based on the new message
        await Task.CompletedTask;
    }
}

public class ImageMetadata
{
    public string Name { get; set; }
    public string OriginalName { get; set; }
    public long Size { get; set; }
    public string Extension { get; set; }
    public string DownloadUrl { get; set; }
}