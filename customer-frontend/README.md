# LendingPlatform
The Lending Platform customer frontend project is based on Angular 9.1.6.
### Prerequisite
Here is the list of software required to set up the customer frontend project of the Lending Platform.

**Note** : This setup is concerning the windows operating system.

| Software | Link |
| ------ | ------ |
| Git | [https://git-scm.com/download/win](https://git-scm.com/download/win) |
| Node JS (Latest version) | [https://nodejs.org/en/download/](https://nodejs.org/en/download/) |

First, you need to clone the project, You can get the customer frontend project repository in Azure DevOps. You will also require git credentials to clone the repository. That can be generated in azure DevOps itself (LendingPlatform -> Repos -> Files (make sure customer frontend project is selected) -> Clone -> Generate Credentials).

Open the command line and run these commands.
```
git clone https://JamoonLendingPlatform@dev.azure.com/JamoonLendingPlatform/LendingPlatform/_git/customer-frontend
```

If you will be committing your changes. It is best to set your email and name as it will be reflected in the commit message.

```
git config --global user.name "FIRST_NAME LAST_NAME"
git config --global user.email "MY_NAME@example.com"
```

Once you have cloned the project in the local machine you need to run these commands to checkout to the development environment.
```
cd customer-frontend
git checkout dev
```

Now, we will require to install angular-cli 9.1.6 and also install all the dependencies in the local node_modules folder. Please run these commands.
```
npm i @angular/cli@9.1.6
npm install
```

Now, everything is setup to run our customer frontend project.

### Development server

Run `npm start` for a dev server. Navigate to `http://localhost:4200/`. The app will automatically reload if you change any of the source files.

### Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

### Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory. Use the `--prod` flag for a production build.


Please follow project guidelines as documented in Azure DevOps wiki of the LendingPlatform - Jamoon ([Here is the link](https://dev.azure.com/JamoonLendingPlatform/LendingPlatform/_wiki/wikis/LendingPlatform.wiki/14/Guidelines)).
