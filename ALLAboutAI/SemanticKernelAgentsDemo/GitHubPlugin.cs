using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SemanticKernelAgentsDemo
{
    internal class GitHubPlugin(GitHubSettings settings)
    {
        [KernelFunction]
        public async Task<GitHubModels.User> GetUserProfileAsync()
        {
            using HttpClient client = this.CreateClient();
            JsonDocument response = await MakeRequestAsync(client, "/user");
            return response.Deserialize<GitHubModels.User>() ?? throw new InvalidDataException($"Request failed: {nameof(GetUserProfileAsync)}");
        }

        [KernelFunction]
        public async Task<GitHubModels.Repo> GetRepositoryAsync(string organization, string repo)
        {
            using HttpClient client = this.CreateClient();
            JsonDocument response = await MakeRequestAsync(client, $"/repos/{organization}/{repo}");

            return response.Deserialize<GitHubModels.Repo>() ?? throw new InvalidDataException($"Request failed: {nameof(GetRepositoryAsync)}");
        }

        [KernelFunction]
        public async Task<GitHubModels.Issue[]> GetIssuesAsync(
            string organization,
            string repo,
            [Description("default count is 30")]
        int? maxResults = null,
            [Description("open, closed, or all")]
        string state = "",
            string label = "",
            string assignee = "")
        {
            using HttpClient client = this.CreateClient();

            string path = $"/repos/{organization}/{repo}/issues?";
            path = BuildQuery(path, "state", state);
            path = BuildQuery(path, "assignee", assignee);
            path = BuildQuery(path, "labels", label);
            path = BuildQuery(path, "per_page", maxResults?.ToString() ?? string.Empty);

            JsonDocument response = await MakeRequestAsync(client, path);

            return response.Deserialize<GitHubModels.Issue[]>() ?? throw new InvalidDataException($"Request failed: {nameof(GetIssuesAsync)}");
        }

        [KernelFunction]
        public async Task<GitHubModels.IssueDetail> GetIssueDetailAsync(string organization, string repo, int issueId)
        {
            using HttpClient client = this.CreateClient();

            string path = $"/repos/{organization}/{repo}/issues/{issueId}";

            JsonDocument response = await MakeRequestAsync(client, path);

            return response.Deserialize<GitHubModels.IssueDetail>() ?? throw new InvalidDataException($"Request failed: {nameof(GetIssueDetailAsync)}");
        }

        private HttpClient CreateClient()
        {
            HttpClient client = new()
            {
                BaseAddress = new Uri(settings.BaseUrl)
            };

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("User-Agent", "request");
            client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {settings.Token}");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

            return client;
        }

        private static string BuildQuery(string path, string key, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return $"{path}{key}={value}&";
            }

            return path;
        }

        private static async Task<JsonDocument> MakeRequestAsync(HttpClient client, string path)
        {
            Console.WriteLine($"REQUEST: {path}");
            Console.WriteLine();

            HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative));
            response.EnsureSuccessStatusCode();
            string content = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(content);
        }
    }

    /// <summary>
    /// Models for GitHub REST API GET responses:
    /// https://docs.github.com/en/rest
    /// </summary>
    internal static class GitHubModels
    {
        public sealed class Repo
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("full_name")]
            public string Name { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }

            [JsonPropertyName("html_url")]
            public string Url { get; set; }
        }

        public sealed class User
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("login")]
            public string Login { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("company")]
            public string Company { get; set; }

            [JsonPropertyName("html_url")]
            public string Url { get; set; }
        }

        public class Issue
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("number")]
            public int Number { get; set; }

            [JsonPropertyName("html_url")]
            public string Url { get; set; }

            [JsonPropertyName("title")]
            public string Title { get; set; }

            [JsonPropertyName("state")]
            public string State { get; set; }

            [JsonPropertyName("labels")]
            public Label[] Labels { get; set; }

            [JsonPropertyName("created_at")]
            public string WhenCreated { get; set; }

            [JsonPropertyName("closed_at")]
            public string WhenClosed { get; set; }
        }

        public sealed class IssueDetail : Issue
        {
            [JsonPropertyName("body")]
            public string Body { get; set; }
        }

        public sealed class Label
        {
            [JsonPropertyName("id")]
            public long Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("description")]
            public string Description { get; set; }
        }
    }

}


