using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProcessConsoleApp
{
    // 事件定义  
    public static class ChatBotEvents
    {
        public const string StartProcess = nameof(StartProcess);
        public const string Exit = nameof(Exit);
        public const string AssistantResponseGenerated = nameof(AssistantResponseGenerated);
    }

    public static class CommonEvents
    {
        public const string UserInputReceived = nameof(UserInputReceived);
    }

    // 介绍步骤  
#pragma warning disable SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    public sealed class IntroStep : KernelProcessStep
    {
        [KernelFunction]
        public void PrintIntroduction()
        {
            Console.WriteLine("=== ChatBot Process Started ===");
            Console.WriteLine("Type 'exit' to quit the conversation.");
        }
    }
#pragma warning restore SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

    // 用户输入步骤  
#pragma warning disable SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    public sealed class ChatUserInputStep : KernelProcessStep
    {
        [KernelFunction]
        public async Task GetUserInputAsync(KernelProcessStepContext context)
        {
            Console.Write("\nYou: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            if (input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                // 发送退出事件 - 使用 context.EmitEventAsync  
                await context.EmitEventAsync(new() { Id = ChatBotEvents.Exit, Data = null });
            }
            else
            {
                // 发送用户输入事件 - 使用 context.EmitEventAsync  
                await context.EmitEventAsync(new() { Id = CommonEvents.UserInputReceived, Data = input });
            }
        }
    }
#pragma warning restore SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

    // 聊天响应步骤  
#pragma warning disable SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    public sealed class ChatBotResponseStep : KernelProcessStep<ChatBotState>
#pragma warning restore SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    {
        private readonly Kernel _kernel;
        internal ChatBotState? _state;

#pragma warning disable SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        public ChatBotResponseStep(Kernel kernel)
        {
            _kernel = kernel;
        }
#pragma warning restore SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

#pragma warning disable SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        public override ValueTask ActivateAsync(KernelProcessStepState<ChatBotState> state)
#pragma warning restore SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        {
#pragma warning disable SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            _state = state.State;
#pragma warning restore SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            return ValueTask.CompletedTask;
        }

        [KernelFunction]
#pragma warning disable SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        public async Task GenerateResponseAsync(KernelProcessStepContext context, string userMessage)
#pragma warning restore SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        {
            _state!.ChatMessages.AddUserMessage(userMessage);
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();
            var response = await chatService.GetChatMessageContentAsync(_state.ChatMessages);

            if (response != null)
            {
                _state.ChatMessages.AddAssistantMessage(response.Content);
                Console.WriteLine($"\nAssistant: {response.Content}");
            }

            // 发送响应生成事件 - 使用 context.EmitEventAsync  
#pragma warning disable SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            await context.EmitEventAsync(new()
            {
                Id = ChatBotEvents.AssistantResponseGenerated,
                Data = response?.Content
            });
#pragma warning restore SKEXP0080 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        }
    }

    // 状态定义  
    public sealed class ChatBotState
    {
        internal ChatHistory ChatMessages { get; } = new();
    }
}


