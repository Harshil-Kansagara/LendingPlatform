# LendingPlatform
The Lending Platform backend project is based on ASP .NET core with Postgresql for the database.
### Prerequisite
Here is the list of software required to set up the backend project of the Lending Platform.

**Note** : This setup is concerning the windows operating system.

| Software | Link |
| ------ | ------ |
| Git | [https://git-scm.com/download/win](https://git-scm.com/download/win) |
| Visual Studio (Community 2019) | [https://visualstudio.microsoft.com/](https://visualstudio.microsoft.com/) |
| .NET Core 3.1 | [https://dotnet.microsoft.com/download/dotnet/3.1](https://dotnet.microsoft.com/download/dotnet/3.1) |
| Postgresql (Latest  version) | [https://www.postgresql.org/download/windows/](https://www.postgresql.org/download/windows/) |

  - .Net Core 3.1 can be directly installed with Visual studio setup
  - Lastest version of Postgresql can be installed. Please remember the username and password while setting up Postgresql as it will be required later.

First, you need to clone the project, You can get the backend project repository in Azure DevOps. You will also require git credentials to clone the repository. That can be generated in azure DevOps itself (LendingPlatform -> Repos -> Files (make sure backend project is selected) -> Clone -> Generate Credentials).

Open the command line and run these commands.
```
git clone https://JamoonLendingPlatform@dev.azure.com/JamoonLendingPlatform/LendingPlatform/_git/backend
```

If you will be committing your changes. It is best to set your email and name as it will be reflected in the commit message.

```
git config --global user.name "FIRST_NAME LAST_NAME"
git config --global user.email "MY_NAME@example.com"
```

Once you have cloned the project in the local machine you need to run these commands to checkout to the development environment.
```
cd backend
git checkout dev
```
Now you will require the appsettings.development.json file which contains all the values of development environment variables.

To create an appsetting.development.json file inside **LendingPlatform.Web.Client** folder and to copy development variables values from our Lending Platform wiki ([Here is the link](https://dev.azure.com/JamoonLendingPlatform/LendingPlatform/_wiki/wikis/LendingPlatform.wiki/10/Appsettings-Web)). 

Please follow the instruction given therein comment as some of the values should be not be copied and used. 

In connection string parameter in appsettings.development.json use your given username and password while installing Postgresql. uid is the username and by default, it will be Postgres and password can set in Pwd in connection string variable value.

Now, Everything is set up to run. Just go to your repository folder backend and double click **LendingPlatform.sln** and it will open up the project in visual studio and your project will be loaded. Now you just need to start the project using IIS Express.

First, check that in the visual studio the startup project is set as LendingPlatform.Web and in the dropdown button of start select IIS Server if not already selected and just click on the button "IIS Express". It will automatically create and update the database on running and also seed the required values. Automatically browser will open "https://localhost:44311/", which is our local server API base URL.

Please follow project guidelines as documented in Azure DevOps wiki of the LendingPlatform - Jamoon ([Here is the link](https://dev.azure.com/JamoonLendingPlatform/LendingPlatform/_wiki/wikis/LendingPlatform.wiki/14/Guidelines))