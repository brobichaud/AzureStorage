using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.RetryPolicies;

namespace ConsoleApplication1
{
	class Program
	{
		static void Main(string[] args)
		{
			CloudQueueClient client = GetQueueClient("DefaultEndpointsProtocol=https;AccountName=debugmadrasplatform;AccountKey=9qMWjQ/AWgv3w1v5sj8A244if/DKfJaByClVrpZVaFND7nPKwBrMUEDXzMWo+9IsQgF/GFXUIbMCFVSoKhT9xA==");
			client.DefaultRequestOptions.RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(2), 3);
			var t = client.DefaultRequestOptions.ServerTimeout;
			var queue = client.GetQueueReference("notify");

			var msg = new CloudQueueMessage("This message was added sync with no ttl");
			queue.AddMessage(msg);
			msg = new CloudQueueMessage("This message was added sync with max ttl");
			queue.AddMessage(msg, CloudQueueMessage.MaxTimeToLive);
			msg = new CloudQueueMessage("This message was added async with no ttl");
			Task.WaitAll(AddAsync(queue, msg));
			Console.WriteLine("Complete");
		}

		static async Task AddAsync(CloudQueue queue, CloudQueueMessage msg)
		{
			try
			{
				await queue.AddMessageAsync(msg);
			}
			catch (Exception e)
			{
				Console.WriteLine("Add failure with message in Queue: {0}, Error:\n{1}\nMessage Contents:\n{2}", "notify", e, msg.AsString);
			}
		}

		private static CloudQueueClient GetQueueClient(string connection)
		{
			var account = CloudStorageAccount.Parse(connection);

			// disable nagle per Microsoft recommendation for table/queue storage
			ServicePointManager.FindServicePoint(account.QueueEndpoint).UseNagleAlgorithm = false;

			return account.CreateCloudQueueClient();
		}
	}
}
