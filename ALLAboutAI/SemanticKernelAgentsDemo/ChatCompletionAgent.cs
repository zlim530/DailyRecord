using Azure.Identity;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

namespace SemanticKernelAgentsDemo
{
    public class ChatCompletionAgentDemo
    {
        static async Task Main(string[] args)
        {
            // Load configuration from environment variables or user secrets.
            Settings settings = new();

            Console.WriteLine("Initialize plugins...");
            GitHubSettings githubSettings = settings.GetSettings<GitHubSettings>();
            GitHubPlugin githubPlugin = new(githubSettings);

            Console.WriteLine("Creating kernel...");
            IKernelBuilder builder = Kernel.CreateBuilder();

            builder.AddAzureOpenAIChatCompletion(
                settings.AzureOpenAI.ChatModelDeployment,
                settings.AzureOpenAI.Endpoint,
                new AzureCliCredential());

            builder.Plugins.AddFromObject(githubPlugin);

            Kernel kernel = builder.Build();

            Console.WriteLine("Defining agent...");
            ChatCompletionAgent agent =
                new()
                {
                    Name = "SampleAssistantAgent",
                    Instructions =
                            """
                        You are an agent designed to query and retrieve information from a single GitHub repository in a read-only manner.
                        You are also able to access the profile of the active user.

                        Use the current date and time to provide up-to-date details or time-sensitive responses.

                        The repository you are querying is a public repository with the following name: {{$repository}}

                        The current date and time is: {{$now}}. 
                        """,
                    Kernel = kernel,
                    Arguments =
                        new KernelArguments(new AzureOpenAIPromptExecutionSettings() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
                        {
                        { "repository", "microsoft/semantic-kernel" }
                        }
                };

            Console.WriteLine("Ready!");

            ChatHistoryAgentThread agentThread = new();
            bool isComplete = false;
            do
            {
                Console.WriteLine();
                Console.Write("> ");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }
                if (input.Trim().Equals("EXIT", StringComparison.OrdinalIgnoreCase))
                {
                    isComplete = true;
                    break;
                }

                var message = new ChatMessageContent(AuthorRole.User, input);

                Console.WriteLine();

                DateTime now = DateTime.Now;
                KernelArguments arguments =
                    new()
                    {
                    { "now", $"{now.ToShortDateString()} {now.ToShortTimeString()}" }
                    };
                await foreach (ChatMessageContent response in agent.InvokeAsync(message, agentThread, options: new() { KernelArguments = arguments }))
                {
                    // Display response.
                    Console.WriteLine($"{response.Content}");
                }

            } while (!isComplete);

        }
    }
}
