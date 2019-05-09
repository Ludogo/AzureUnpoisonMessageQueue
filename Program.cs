using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;

namespace AzureStorageCopyMessageQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
            Console.WriteLine("Process terminated : press enter to leave");
            Console.ReadLine();
        }
        static async Task MainAsync(string[] args)
        {
            // Prompt the user for the connection string
            Console.WriteLine("Enter the connection string for the Azure storage account:");
            var connectionString = Console.ReadLine();

            // Prompt the user for the queue name
            Console.WriteLine("Enter the name of the Azure Queue to unpoison:");
            var queueName = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(queueName))
            {
                // setup the connection to table storage
                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
                CloudQueueClient queueClient = cloudStorageAccount.CreateCloudQueueClient();
                CloudQueue poisonQueue = queueClient.GetQueueReference(queueName + "-poison");
                CloudQueue regularQueue = queueClient.GetQueueReference(queueName);

                CloudQueueMessage retrievedMessage = await poisonQueue.GetMessageAsync();

                while (retrievedMessage != null)
                {
                    // delete the message from the poison queue
                    await poisonQueue.DeleteMessageAsync(retrievedMessage);
                    // queue up a new message on the original queue
                    await regularQueue.AddMessageAsync(retrievedMessage);
                    Console.WriteLine("Moved over message from poison queue: " + retrievedMessage.AsString);

                    // Get the next message for processing
                    retrievedMessage = await poisonQueue.GetMessageAsync();
                }
            }
            else
            {
                Console.WriteLine("Unable to proceed without the connection string and queue name.");
            }
        }
    }

} 