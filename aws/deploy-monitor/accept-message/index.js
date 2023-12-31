var AWS = require('aws-sdk');

exports.handler = function (event, context) {
  let QUEUE_URL = process.env.sqsqueue;
  let sqs = new AWS.SQS({ region: process.env.sqsregion });

  var params = {
    MessageBody: event.body,
    QueueUrl: QUEUE_URL,
    MessageAttributes: {}
  };

  sqs.sendMessage(params, function (err, data) {
    if (err) {
      console.log('error:', "Fail Send Message" + err);
      context.done('error', "ERROR Put SQS");
    } else {
      console.log('data:', data.MessageId);
      context.done(null, ''); 
    }
  });
}