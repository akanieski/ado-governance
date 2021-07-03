# Azure DevOps Governance Tests

This tool can be used to help report on roles, groups and permissions compliance. 

## Getting Started

The tool consists of a set of tests that will iterate your collection and projects as 
configured and check for various conditions that should be met to meet compliance. 

You can start by creating a `config.json` in the root of the project folder. It will 
contain the settings that will be used to check for compliance.

The following features are supported:

- __Check an organization for a group or team that has members that have been given explicit access__
  - Used to check for Project Collection Administrators that have been given explicit access
  - Used to check a specific Project for users that have been given explicit Project Administrator access
  - **Configuration Setting:** `CheckGroupForExplicitAccess`
- __Check all project groups or teams for explicit access__
  - Used to check all projects for a given group or team scoped to that project that have been given explicit access
  - **Configuration Setting:** `CheckProjectGroupsForExplicitAccess`


```json
{
    "OrganizationUrl": "https://dev.azure.com/devops-collab",
    "PersonalAccessToken": "Your PAT",
    "CheckGroupForExplicitAccess": [
        "[Some Org]\\Project Collection Administrators",
        "[Some Project]\\Project Administrators"
    ],
    "CheckProjectGroupsForExplicitAccess": [
        "Project Administrators"
    ]
}
```

