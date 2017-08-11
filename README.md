# Conjur API for .NET

This is a *Draft* implementation of the .NET API for [Conjur](https://developer.conjur.net/).
This implementation includes an example that shows how to:

    - Authenticate
    - Check permissions to get the value of a variable
    - Get the value of a variable
    - Use a Host Factory token to create a new Host and get an apiKey to use with Conjur

## Building

This sample was built and tested with Visual Studio 2015. 

To load in Visual Studio, from the Visual Studio File menu select Open > Project/Solution > api-dotnet.sln and build the solution. This will create:

    - conjur-api.dll: the .NET version of the Conjur API.
    - ConjurTest.dll: test DLL used for automated testing of the Conjur .NET API
    - example.exe: sample application that uses the Conjur API.

Optionally, to build in a Docker container, it is recommended to use Mono and xbuild.

## Usage

To run the sample in Visual Studio, set the `example` project as the Startup Project.  To do so, in the Solution Explorer right click over `example` and select `Set as Startup Project`. 

```sh
Usage: Example  <applianceURL>
                <applianceCertificatePath>
                <username> 
                <password> 
                <variableId>
                <hostFactoryToken>
```

applianceURL: the applianceURL including /api e.g. https://conjurmaster.myorg.com/api

applianceCertificatePath: the path and name of the Conjur appliance certificate. The easiest way to get the certifiate is to use the Conjur CLI command `conjur init -h conjurmaster.myorg.com -f .conjurrc`. The certificate can be taken from any system you have run the Conjur CLI from.

username: Username of a user in Conjur. Alternatively can be a hostname.

password: Password of a user in Conjur. Alternatively can be a host apiKey.

variableId: The name of an existing variable in Conjur that has a value set and for which the `username` has execute permissions.

hostFactoryToken: A hostfactory token. The easiest way to get a host factory token for testing is to add a hostfactory to a layer using the Conjur CLI command `conjur hostfactory create` and `conjur hostfactory token create`. Take the token returned from that call and pass it as the hostFactoryToken parameter to this example.

## Example

```sh
    // Instantiate a Conjur Client object.
    //  parameter: URI - conjur appliance URI (including /api)
    //  return: Client object - if URI is incorrect errors thrown when used
    Uri uri = new Uri("https://myorg.com/api");
    Client conjurClient = new Client(uri);

    // Login with Conjur credentials like userid and password,
    // or hostid and api_key, etc
    //  parameters: username - conjur user or host id for example
    //              password - conjur user password or host api key for example
    string conjurAuthToken = conjurClient.Login(username, password);

    // Check if this user has permission to get the value of variableId
    // That requires execute permissions on the variable

    // Instantiate a Variable object
    //   parameters: client - contains authentication token and conjur URI
    //               name - the name of the variable
    Variable conjurVariable = new Variable(conjurClient, variableId);

    // Check if the current user has "execute" privilege required to get
    // the value of the variable
    //   parameters: privilege - string name of the priv to check for
    bool isAllowed = conjurVariable.Check("execute");
    if (!isAllowed)
    {
        Console.WriteLine("You do not have permissions to get the value of {0}", variableId);
    }
    else
    {
        Console.WriteLine("{0} has the value: {1}", variableId, conjurVariable.GetValue());
    }
```

## License

Copyright 2016-2017 CyberArk

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
