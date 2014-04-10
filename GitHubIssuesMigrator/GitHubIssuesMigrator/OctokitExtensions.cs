namespace GitHubIssuesMigrator
{
    using System.Linq;
    using Octokit;

    public static class OctokitExtensions
    {
        public static NewIssue ToNewIssue(this Issue issue)
        {
            var newIssue = new NewIssue(issue.Title);
            newIssue.Assignee = issue.Assignee != null ? issue.Assignee.Name : null;
            newIssue.Body = issue.Body;
            issue.Labels.Select(i => i.Name).ToList().ForEach(n => newIssue.Labels.Add(n));

            // issue.Comments.Select(c => c.).ToList().ForEach(n => newIssue.Labels.Add(n));
            return newIssue;
        }

        public static IssueUpdate ToIssueUpdate(this Issue issue)
        {
            var updatedIssue = new IssueUpdate();
            updatedIssue.Title = issue.Title;
            updatedIssue.Assignee = issue.Assignee != null ? issue.Assignee.Name : null;
            updatedIssue.Body = issue.Body;
            issue.Labels.Select(i => i.Name).ToList().ForEach(n => updatedIssue.Labels.Add(n));
            updatedIssue.State = issue.State;
            if (issue.Milestone != null)
            {
                updatedIssue.Milestone = issue.Milestone.Number;
            }

            return updatedIssue;
        }
    }
}
