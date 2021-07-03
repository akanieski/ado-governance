using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.Graph;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Security.Client;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Linq;
using Microsoft.VisualStudio.Services.Common;
using System.Collections.Generic;

namespace ADO.Governance
{
    [TestClass]
    public partial class ExplicitRightsTests : BaseTest
    {
        public ExplicitRightsTests() : base() { }

        public static IEnumerable<object[]> GetProjectCheckData()
        {
            var Config = GetConfig();
            using (var conn = new VssConnection(new Uri(Config.OrganizationUrl), new VssBasicCredential(string.Empty, Config.PersonalAccessToken)))
            using (var Project = conn.GetClient<ProjectHttpClient>())
            {
                var failures = new List<UnitTestAssertException>();

                var projects = Project.GetProjects(top: 1000).SyncResult();
                foreach (var project in projects)
                {
                    foreach (var projectFilter in Config.CheckProjectGroupsForExplicitAccess ?? new string[] { })
                    {
                        yield return new object[] {
                            $"[{project.Name}]\\{projectFilter}"
                        };
                    }
                }
            }
        }

        public static IEnumerable<object[]> GetGroupCheckData()
        {
            foreach (var group in GetConfig().CheckGroupForExplicitAccess)
            {
                yield return new object[] {
                    group
                };
            }
        }

        [TestMethod]
        [DynamicData(nameof(GetProjectCheckData), DynamicDataSourceType.Method)]
        [Description("Check configured projects in the collection and make sure no single user is explicitly defined in the configured group.")]
        public async Task CheckProjectsForOneOffProjectAdmins(string projectGroup)
            => await CheckGroupForOneOffUsers(projectGroup);

        [DataTestMethod]
        [DynamicData(nameof(GetGroupCheckData), DynamicDataSourceType.Method)]
        [Description("Check configured groups and make sure no single user is explicitly defined in the given group.")]
        public async Task CheckGroupForOneOffUsers(string groupName)
        {
            using (var conn = new VssConnection(new Uri(Config.OrganizationUrl), new VssBasicCredential(string.Empty, Config.PersonalAccessToken)))
            using (var Security = await conn.GetClientAsync<SecurityHttpClient>())
            using (var Graph = await conn.GetClientAsync<GraphHttpClient>())
            {
                PagedGraphGroups paged_groups = null;
                GraphGroup groups = null;
                while (groups == null && (paged_groups == null || (paged_groups.ContinuationToken != null && paged_groups.ContinuationToken.Any())))
                {
                    paged_groups = await Graph.ListGroupsAsync(continuationToken: paged_groups == null ? null : paged_groups.ContinuationToken.FirstOrDefault());
                    groups = paged_groups.GraphGroups.FirstOrDefault(g => g.PrincipalName.Equals(groupName));
                }

                Assert.IsNotNull(groups, $"Could not locate '{groupName}'");

                var group_members = await Graph.ListMembershipsAsync(groups.Descriptor, GraphTraversalDirection.Down);

                var group_users = await Task.WhenAll((from m in group_members where m.MemberDescriptor.SubjectType == "aad" select m)
                    .Select(async m => await Graph.GetUserAsync(m.MemberDescriptor.ToString())));

                if (group_users.Any())
                {
                    Assert.IsFalse(true,
                        $"The following users should not have explicit rights to '{groupName}' and are not in compliance: {string.Join(",", group_users.Select(x => x.MailAddress))}");
                }
                else
                {
                    Assert.IsTrue(true, $"{groupName} is in compliance.");
                }
            }
        }
    }
}