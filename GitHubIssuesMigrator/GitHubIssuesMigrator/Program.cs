namespace GitHubIssuesMigrator
{
    using System;
    using System.Linq;
    using System.Net.Http.Headers;
    using System.Web.Script.Serialization;
    using Octokit;

    public class Program
    {
        public static void Main(string[] args)
        {
            var sourceOrg = "";
            var destOrg = "";
            var sourceRepo = "";
            var destRepo = "";
            var accessToken = "";

            // label name or simply leave empty for all
            var labelToMigrate = ""; 

            var githubClient = new GitHubClient(new ProductHeaderValue("IssuesMigrator"));
            githubClient.Credentials = new Credentials(accessToken);

            var issueRequest = new RepositoryIssueRequest();
            issueRequest.State = ItemState.Open;            

            if (!string.IsNullOrWhiteSpace(labelToMigrate))
            {
                issueRequest.Labels.Add(labelToMigrate);
            }

            var issues = githubClient.Issue.GetForRepository(sourceOrg, sourceRepo, issueRequest).Result;

            foreach (var issue in issues)
            {
                // save to new repo   
                var resNewIssue = githubClient.Issue.Create(destOrg, destRepo, issue.ToNewIssue()).Result;

                // copy comments
                if (issue.Comments > 0)
                {
                    var comments = githubClient.Issue.Comment.GetForIssue(sourceOrg, sourceRepo, issue.Number).Result;
                    foreach (var comment in comments.OrderBy(c => c.CreatedAt))
                    {
                        var pstCreatedAt = TimeZoneInfo.ConvertTime(comment.CreatedAt, TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));
                        
                        // modifying the content of the comment to persist the original user and posted date                        
                        var encodedCommentBody = new JavaScriptSerializer().Serialize(string.Format("Note: this comment has been migrated from another repo by a tool.{0}It was originally posted by {1} on {2}.{0}{0}{0}{3}{0}", Environment.NewLine, comment.User.Login.ToUpperInvariant(), pstCreatedAt, comment.Body));
                        var commentBody = string.Format("{{ \"body\": {0} }}", encodedCommentBody);

                        // save to new repo
                        var resNewComment = githubClient.Issue.Comment.Create(destOrg, destRepo, resNewIssue.Number, commentBody).Result;
                    }
                }

                // close in source repo                
                var updatedIssue = issue.ToIssueUpdate();
                updatedIssue.State = ItemState.Closed;
                updatedIssue.Labels.Add("_migrated");
                var resIssueComment = githubClient.Issue.Comment.Create(sourceOrg, sourceRepo, issue.Number, string.Format("{{ \"body\": \"Moved to {0}/{1} repo.\" }}", destOrg, destRepo)).Result;
                var resUpdatedIssue = githubClient.Issue.Update(sourceOrg, sourceRepo, issue.Number, updatedIssue).Result;
            }

            Console.WriteLine(string.Format("Migrated {0} issues from {1}\\{2} to {3}\\{4}", issues.Count, sourceOrg, sourceRepo, destOrg, destRepo));
            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }
    }
}
