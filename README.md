# FileRepository Project
This simple application provides a simple file management functionality. It works in a client-server architecture and organizes data as repositories.

# Server Side Functionality
The server uses configuration file in the XML format that is passed to the application as a first argument. An example of such file may be found below:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<REPOSITORIES>
  <REPOSITORY>
    <NAME>artifacts</NAME>
    <PATH>d:\repositories\artifacts</PATH>
  </REPOSITORY>
  <REPOSITORY>
    <NAME>plugins</NAME>
    <PATH>d:\extensions\plugins</PATH>
  </REPOSITORY>
</REPOSITORIES>
```

# Client Side Functionality
The client part is used to manage files localy. It allows for downloading data from specified version of the package in the repository that has been added as a local one.

## Command Line Interface
Currently only one functional command is supported. Please take a look at the list below for details:
* _getp=\<repo\>,\<package\>,\<version\>_ - get specified package version from remote repository
* _help_                                  - print help message

## Interactive Console
When used does not specify input arguments, the client application runs in an interactive mode. The list below shows all availabe commands:
* _ADD_    - add local repository
* _DEL_    - remove local repository
* _STATUS_ - local repository status
* _GET_    - get repositories
* _GETV_   - get package versions
* _GETP_   - get package
* _EXIT_   - exit client
* _HELP_   - prints available commands list

# Remote Repository Organization
The root directory of the repository should contains packages as a separated folders. Each package should contain folders that represent single versions. There is no limitation on the number of files inside a single package version.

# NOTE
Currently, for simplicity and ease of testing, the connection is is forced to work on the localloop.
