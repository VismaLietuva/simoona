**Note:** *this is draft version of README, meaning that some parts need more input, suggestions and day-to-day experience working with this solution.*

# Simoona Premium (draft)

This repository contains Simoona Premium features part.

Open-source part is located in GitHub: https://github.com/VismaLietuva/simoona

## Features

|Freemium (open-source) features |Premium features|
|--------------------------------|----------------|
| Wall                           | Events         |
| Projects                       | Books          |
| Kudos                          | Service requests |
| External links                 | Office map       |
| Birthdays widget               | Org structure    |
| Employees list                 | Integration with google calendar |
| Form-based authentication      | Kudos loyalty bot  |
|                                | Kudos shop         |
|                                | Books mobile app   |
|                                | Committees         |

## Structure

It might change, but currently it is as following:
- Front-end app is located fully (including Premium) in open-source repository
- Back-end part in open-source repository (Simoona "core")
- Back-end part in Premium repository (Premium features)

Due too tight coupling Premium features front-end part is located in open-source repository. These features are hidden from UI, however unhiding them - won't make them work, because they also require back-end part.

To enable Premium features:
- Set `window.isPremium = false;` in front-end app
- Compile Premium project and copy `Shrooms.Premium.dll` to `Extensions` folder of `Shrooms.API` (open-source project)

## Development

Open-source repository (master branch) is included in this repository as Git sub-module. It means that all needed code can be checked-out automatically from both repositories,
and all projects can be included in one VS solution (`Premium.sln`).
This development workflow is not yet mature and has known issues, therefore all ideas and suggestions are welcome.

Current known issues:
- To compile `Shrooms.Premium.csproj` you need either to compile Simoona open-source part manually or use `Premium.sln`.
- Using `Premium.sln` you still probably will need to open `Shrooms.sln` (open-source solution) and restore NuGet packages for it, because paths are mixed between solutions.
- If you need to work both on Premium and open-source branches - included sub-module probably won't help you, because you will want to point project to your own GitHub repository fork.
- There are tasks in a backlog to improve development - feel free to contribute and improve!

## CI/CD pipeline

Build and deployment are driven by Azure Pipelines:
- Build: https://dev.azure.com/vismalietuva/simoona/_build
- Deployment/releases: https://dev.azure.com/vismalietuva/simoona/_release

Current status and planned improvements:
- Currently CI build is not being triggered automatically on every commit (because there is build time limit on hosted agents, which might be hit during "active development").
- DB migrations are currently not being executed automatically during deployment (there is a task in backlog)
- Deployment currently works only to staging environment. Production deployment needs more investigation and preparation, since it's more critical and being used by customers. (There is also task in backlog.)

## Staging environment
- URL: https://simoona-staging.azurewebsites.net/test ("test" - is default company)
- Admin username: testertester@visma.com 
- Admin password: testerPass123