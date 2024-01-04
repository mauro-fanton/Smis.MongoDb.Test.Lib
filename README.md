## Description

This is a Test library based on Mongo2Go. The aim of this library is to create an in-memory test mongoDb database, so query can be tested.

## Automatic package
The Nuget package is hold in GitHub. This is automatically packaged and published by the GitHub workflow "dotnet.hml".

The workflow use the GITHUB_TOKEN for security. The permission of the GITHUB_TOKEN are set to read and write. 

As default this is set to read. But here it set to write too otherwise the CI will not be able to access the GitHub Packages repository.

## How to set GITHUB_TOKEN permission

1.  In your repository click **Setting**.If you cannot see the "Settings" tab, select the  dropdown menu, then click **Settings**.
2.  In the left sidebar, click  **Actions**, then click **General**.
3.  Under **Workflow permissions** and select **Read and Write permission** setting.
4.  Click **Save** to apply the settings.

## Versioning

Versining is given by the GitVersion file. If you want to change the version od this package, you will need to change the version in teh GitVersion.yml file.

## packages.sh

This will build and create a packages in the local environment.
Usage:

```
./package.sh Release|Debug
```
Release or Debug are the configuration.

This will create a nuget package into the **./published** directory and then will store the package at **../package** directory
