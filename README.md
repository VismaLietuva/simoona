**Note:** *this is draft version of README, meaning that some parts need more input, suggestions and day-to-day experience working with this solution.*

# Simoona Premium

This repository contains Simoona Premium features part.

Open-source part is located in GitHub: https://github.com/VismaLietuva/simoona

## Features

|Freemium (Open-source) features |Premium features|
|--------------------------------|----------------|
| Walls                          | Events         |
| Projects                       | Books          |
| Kudos                          | Service requests  |
| External links                 | Office map        |
| Birthdays widget               | Org structure     |
| Employees list                 | Kudos loyalty bot |
| Google authentication          | Kudos shop         |
| Facebook authentication        | Books mobile app   |
| Form-based authentication      | Committees         |

## Structure

It might change, but currently it is as following:
- Front-end app is located fully (including Premium) in Open-source repository
- Back-end part in Open-source repository (Simoona "core")
- Back-end part in Premium repository (Premium features)

Due to tight coupling - front-end part for Premium features is located in Open-source repository. These features are hidden from UI by default, however unhiding them - won't make them work, because they also require back-end part.

To enable Premium features:
- Set `window.isPremium = true;` in front-end app
- Back-end part for Premium features (`Shrooms.Premium.dll`) should be copied to `Extensions` folder of Open-source project (`$\open-source\src\api\Main\PresentationLayer\Shrooms.API\Extensions\`)

## Development

Open-source repository (master branch) is referenced in this repository as Git sub-module. It means that all needed code can be checked-out automatically from both repositories,
and all projects can be included in one VS solution (`Premium.sln`).

For better development experience, especially when you need to work with both Open-source and Premium at the same time and you have forked Open-source repository there is another solution:
- Execute `build.bat` file (`$\builds\build.bat`) and provide what is asked.
- The script will update paths pointing to your forked Open-source version and also will create `Premium.WithForkedOss.sln` solution, which you can use for further development.
- When you build `Shrooms.Premium.csproj` project - it will recycle Simoona webapp (to release the lock on files) and copy needed extensions dlls to correct location automatically, thus making development experience smoother.

**Note 1:** additional attention is still needed when you plan to update/install NuGet packages in any of the projects. Make sure that you don't end commiting project files refering your local paths.

**Note 2:** if you plan to add new projects to solution, they should be added to main `Premium.sln` solution.

## CI/CD pipeline

Build and deployment are driven by Azure Pipelines:
- Build: https://dev.azure.com/vismalietuva/simoona/_build
- Deployment/releases: https://dev.azure.com/vismalietuva/simoona/_release

## Staging environment
- URL: https://simoona-staging.azurewebsites.net/test ("test" - is default company)
- Admin username: testertester@visma.com 
- Admin password: testerPass123