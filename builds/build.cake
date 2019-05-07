#addin "nuget:?package=Cake.FileHelpers&version=3.2.0"
#addin "nuget:?package=Cake.PinNuGetDependency&version=3.3.0"

using System.Xml;
using Path = System.IO.Path;

var target = Argument("target", "Default");

var rootPath =  Argument("rootPath", @"..\src");
var simoonaOssPath = Argument("simoonaOssPath", @"..\..\open-source");

var projectPathUser = Path.Combine(rootPath, @"Shrooms.Premium\Shrooms.Premium.csproj.user");
var testsProjectPathUser = Path.Combine(rootPath, @"Shrooms.Premium.UnitTests\Shrooms.Premium.UnitTests.csproj.user");
var projectPath = Path.Combine(rootPath, @"Shrooms.Premium\Shrooms.Premium.csproj");
var nugetConfigPath = Path.Combine(rootPath, @".nuget\nuget.config");
var packagesPath = Path.Combine(simoonaOssPath, @"src\api\packages");

Task("Default")
    .Does(() =>
{
    Information("Root path: {0}", rootPath);
    Information("Simoona OSS path: {0}", simoonaOssPath);
    Information("Project path (user): {0}", projectPathUser);
    Information("nuget.config path: {0}", nugetConfigPath);
    Information("Packages path: {0}", packagesPath);

    if (!FileExists(projectPathUser))
    {
        CreateCsprojUser(projectPathUser, simoonaOssPath);
        CreateCsprojUser(testsProjectPathUser, simoonaOssPath);
    }
    else 
    {
        UpdateCsprojUser(projectPathUser, simoonaOssPath);
        UpdateCsprojUser(testsProjectPathUser, simoonaOssPath);
    }

    UpdateCsprojReferences(projectPath, packagesPath);

    if (!FileExists(nugetConfigPath))
    {
        CreateNugetConfig(nugetConfigPath, packagesPath);
    }

    UpdateNugetConfig(nugetConfigPath, packagesPath);
});

void CreateCsprojUser(string projectPathUser, string simoonaCoreLocation)
{
    var csprojXml = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <SimoonaCoreLocation>{0}</SimoonaCoreLocation>
  </PropertyGroup>
</Project>", simoonaCoreLocation);

    FileWriteText(projectPathUser, csprojXml);
}

void UpdateCsprojUser(string projectPathUser, string simoonaCoreLocation)
{
    var projectFile = File(projectPathUser);

    var xPath = "/ns:Project/ns:PropertyGroup/ns:SimoonaCoreLocation";
    var namespaceUrl = "http://schemas.microsoft.com/developer/msbuild/2003";
    var namespaces = new Dictionary<string, string>
    {
        { "ns", namespaceUrl }
    };

    var xmlPokeSettings = new XmlPokeSettings { Namespaces = namespaces };
    var xmlPeekSettings = new XmlPeekSettings { Namespaces = namespaces };

    var propertyExists = XmlPeek(projectFile, xPath, xmlPeekSettings);

    if (propertyExists == null)
    {
        Information("SimoonaCoreLocation property does not exist, creating new");

        var xmlDoc = new XmlDocument();
        xmlDoc.Load(projectPathUser);

        var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
        namespaceManager.AddNamespace("ns", namespaceUrl);

        var rootNode = xmlDoc.SelectSingleNode("/ns:Project", namespaceManager);
        var propertyGroupNode = xmlDoc.CreateNode(XmlNodeType.Element, "PropertyGroup", namespaceUrl);
        var simoonaCoreLocationNode = xmlDoc.CreateNode(XmlNodeType.Element, "SimoonaCoreLocation", namespaceUrl);
        simoonaCoreLocationNode.InnerText = simoonaCoreLocation;

        propertyGroupNode.AppendChild(simoonaCoreLocationNode);
        rootNode.AppendChild(propertyGroupNode);

        xmlDoc.Save(projectPathUser);
    }
    else
    {
        XmlPoke(projectFile, xPath, simoonaCoreLocation, xmlPokeSettings);
    }
}

void UpdateCsprojReferences(string projectPath, string packagesPath)
{
    var namespaceUrl = "http://schemas.microsoft.com/developer/msbuild/2003";

    var xmlDoc = new XmlDocument();
    xmlDoc.Load(projectPath);

    var references = xmlDoc.GetElementsByTagName("Reference");
    foreach (var reference in references)
    {
        var xmlElement = (XmlElement)reference;

        if (xmlElement.HasChildNodes && xmlElement["HintPath"].InnerText.Contains(packagesPath))
        {
            if (xmlElement["Private"] != null)
            {
                xmlElement["Private"].InnerText = "False";
            }
            else
            {
                var privateNode = xmlDoc.CreateNode(XmlNodeType.Element, "Private", namespaceUrl);
                privateNode.InnerText = "False";
                xmlElement.AppendChild(privateNode);
            }
        }
    }
    
    xmlDoc.Save(projectPath);
}

void CreateNugetConfig(string nugetConfigPath, string packagesPath)
{
    var xmlDoc = new XmlDocument();

    var rootNode = xmlDoc.CreateNode(XmlNodeType.Element, "configuration", "");
    xmlDoc.AppendChild(rootNode);

    var xmlDeclaration = xmlDoc.CreateXmlDeclaration( "1.0", "UTF-8", null);
    xmlDoc.InsertBefore(xmlDeclaration, rootNode);

    xmlDoc.Save(nugetConfigPath);
}

void UpdateNugetConfig(string nugetConfigPath, string packagesPath)
{
    var projectFile = File(nugetConfigPath);
    var nugetPath = Context.Tools.Resolve("nuget.exe");

    var settings = new ProcessSettings 
    {
        Arguments = new ProcessArgumentBuilder()
            .Append("config")
            .Append("-set")
            .Append("repositoryPath=" + packagesPath)
            .Append("-configfile")
            .Append(nugetConfigPath)
    };

    // Ex.: nuget config -set repositoryPath=..\packages -configfile ..\.nuget\nuget.config
    StartProcess(nugetPath, settings);
}

RunTarget(target);
