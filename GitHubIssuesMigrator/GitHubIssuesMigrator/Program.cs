namespace GitHubIssuesMigrator
{
    using System;
    using System.Net.Http.Headers;
    using Octokit;

    public class Program
    {
        public static void Main(string[] args)
        {
            var org = "WindowsAzure";
            var sourceRepo = "wacom-portal";
            var destRepo = "marker";
            var accessToken = "token-here";

            var githubClient = new GitHubClient(new ProductHeaderValue("IssuesMigrator"));
            githubClient.Credentials = new Credentials(accessToken);

            var issueRequest = new RepositoryIssueRequest();
            issueRequest.Labels.Add("marker");
            issueRequest.State = ItemState.Open;

            var issues = githubClient.Issue.GetForRepository(org, sourceRepo, issueRequest).Result;

            foreach (var issue in issues)
            {
                // save to new repo
                var resNewIssue = githubClient.Issue.Create(org, destRepo, issue.ToNewIssue()).Result;

                // close in source repo                
                var updatedIssue = issue.ToIssueUpdate();
                updatedIssue.State = ItemState.Closed;
                var resIssueComment = githubClient.Issue.Comment.Create(org, sourceRepo, issue.Number, string.Format("{{ \"body\": \"Moved to {0} repo.\" }}", destRepo)).Result;
                var resUpdatedIssue = githubClient.Issue.Update(org, sourceRepo, issue.Number, updatedIssue).Result;
            }

            Console.WriteLine(string.Format("Migrated {0} issues from {1} to {2}", issues.Count, sourceRepo, destRepo));
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
