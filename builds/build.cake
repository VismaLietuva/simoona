#addin "nuget:?package=Cake.FileHelpers&version=3.2.0"
#addin "nuget:?package=Cake.PinNuGetDependency&version=3.3.0"

using System.Xml;
using Path = System.IO.Path;
using IODirectory = System.IO.Directory;

var target = Argument("target", "Default");

var ossRelativePathToSlnOriginal = @"..\open-source";
var ossRelativePathToCsprojOriginal = @"..\..\open-source";

var rootPath =  Argument("rootPath", @"..\src");
var ossPathForked = Argument("ossPathForked", ossRelativePathToCsprojOriginal);

var ossRelativePathToSlnForked = MakeRelativePath(IODirectory.GetCurrentDirectory() + "\\", ossPathForked);
var ossRelativePathToCsprojForked = MakeRelativePath(IODirectory.GetCurrentDirectory() + "\\src\\", ossPathForked);

var projectPropsPath = Path.Combine(rootPath, @"Shrooms.Premium\Shrooms.Premium.props");
var testsProjectPropsPath = Path.Combine(rootPath, @"Shrooms.Premium.UnitTests\Shrooms.Premium.UnitTests.props");
var projectPath = Path.Combine(rootPath, @"Shrooms.Premium\Shrooms.Premium.csproj");
var slnPath = Path.Combine(rootPath, @"Premium.sln");
var slnPathForked = Path.Combine(rootPath, @"Premium.WithForkedOss.sln");
var nugetConfigPath = Path.Combine(rootPath, @".nuget\nuget.config");

var packagesInnerPath = @"src\api\packages";
var packagesPathOriginal = Path.Combine(ossRelativePathToCsprojOriginal, packagesInnerPath);
var packagesPathForked = Path.Combine(ossRelativePathToCsprojForked, packagesInnerPath);

Information("Current directory: {0}", IODirectory.GetCurrentDirectory());
Information("Root solution relative path: {0}", rootPath);
Information("Simoona OSS relative path to *.sln (original): {0}", ossRelativePathToSlnOriginal);
Information("Simoona OSS relative path to *.csproj: {0}", ossRelativePathToCsprojOriginal);
Information("Simoona OSS path (forked): {0}", ossPathForked);
Information("Simoona OSS relative path to *.sln (forked): {0}", ossRelativePathToSlnForked);
Information("Simoona OSS relative path to *.csproj (forked): {0}", ossRelativePathToCsprojForked);
Information("Project path (*.csproj.user): {0}", projectPropsPath);
Information("*.sln path: {0}", slnPath);
Information("*.sln path (forked): {0}", slnPathForked);
Information("nuget.config path: {0}", nugetConfigPath);
Information("Packages path: {0}", packagesPathOriginal);
Information("Packages path (forked): {0}", packagesPathForked);

Task("Default")
    .Does(() =>
{
    if (!FileExists(projectPropsPath))
    {
        CreateCsprojProps(projectPropsPath, ossRelativePathToCsprojForked);
    }
    else 
    {
        UpdateCsprojProps(projectPropsPath, ossRelativePathToCsprojForked);
    }

    if (!FileExists(testsProjectPropsPath))
    {
        CreateCsprojProps(testsProjectPropsPath, ossRelativePathToCsprojForked);
    }
    else 
    {
        UpdateCsprojProps(testsProjectPropsPath, ossRelativePathToCsprojForked);
    }

    UpdateCsprojReferences(projectPath, packagesInnerPath, packagesPathOriginal, packagesPathForked);

    if (!FileExists(nugetConfigPath))
    {
        CreateNugetConfig(nugetConfigPath, packagesPathForked);
    }

    UpdateNugetConfig(nugetConfigPath, packagesPathForked);
    UpdateCustomSln(slnPath, slnPathForked, ossRelativePathToSlnOriginal, ossRelativePathToSlnForked);
});

void CreateCsprojProps(string projectPropsPath, string simoonaCoreLocationValue)
{
    var csprojXml = string.Format(@"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <SimoonaCoreLocation>{0}</SimoonaCoreLocation>
  </PropertyGroup>
</Project>", simoonaCoreLocationValue);

    FileWriteText(projectPropsPath, csprojXml);
}

void UpdateCsprojProps(string projectPropsPath, string simoonaCoreLocationValue)
{
    var projectFile = File(projectPropsPath);

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
        xmlDoc.Load(projectPropsPath);

        var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
        namespaceManager.AddNamespace("ns", namespaceUrl);

        var rootNode = xmlDoc.SelectSingleNode("/ns:Project", namespaceManager);
        var propertyGroupNode = xmlDoc.CreateNode(XmlNodeType.Element, "PropertyGroup", namespaceUrl);
        var simoonaCoreLocationNode = xmlDoc.CreateNode(XmlNodeType.Element, "SimoonaCoreLocation", namespaceUrl);
        simoonaCoreLocationNode.InnerText = simoonaCoreLocationValue;

        propertyGroupNode.AppendChild(simoonaCoreLocationNode);
        rootNode.AppendChild(propertyGroupNode);

        xmlDoc.Save(projectPropsPath);
    }
    else
    {
        XmlPoke(projectFile, xPath, simoonaCoreLocationValue, xmlPokeSettings);
    }
}

void UpdateCsprojReferences(string projectPath, string packagesInnerPath, string packagesPathOriginal, string packagesPathForked)
{
    var namespaceUrl = "http://schemas.microsoft.com/developer/msbuild/2003";

    var xmlDoc = new XmlDocument();
    xmlDoc.Load(projectPath);

    var references = xmlDoc.GetElementsByTagName("Reference");
    foreach (var reference in references)
    {
        var xmlElement = (XmlElement)reference;

        if (xmlElement.HasChildNodes && xmlElement["HintPath"].InnerText.Contains(packagesInnerPath))
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

    // To change nuget packages references paths:
    //ReplaceTextInFiles(projectPath, packagesPathOriginal, packagesPathForked);
}

void UpdateCustomSln(string slnPath, string slnPathUser, string originalOssRelativePath, string ossRelativePath)
{
    CopyFile(slnPath, slnPathUser);
    ReplaceTextInFiles(slnPathUser, originalOssRelativePath, ossRelativePath);
}

void CreateNugetConfig(string nugetConfigPath, string packagesPath)
{
    var xmlDoc = new XmlDocument();

    var rootNode = xmlDoc.CreateNode(XmlNodeType.Element, "configuration", "");
    xmlDoc.AppendChild(rootNode);

    var xmlDeclaration = xmlDoc.CreateXmlDeclaration( "1.0", "UTF-8", null);
    xmlDoc.InsertBefore(xmlDeclaration, rootNode);

    var fileInfo = new FileInfo(nugetConfigPath);

    if (!fileInfo.Directory.Exists)
    {
        fileInfo.Directory.Create();
    }

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

private String MakeRelativePath(string fromPath, string toPath)
{
    if (string.IsNullOrEmpty(fromPath))
    {
         throw new ArgumentNullException("fromPath");
    }

    if (string.IsNullOrEmpty(toPath)) 
    {
        throw new ArgumentNullException("toPath");
    }

    var path = new DirectoryPath(toPath);

    if (path.IsRelative)
    {
        throw new ArgumentException("toPath", "Path is already relative");
    }

    Uri fromUri = new Uri(fromPath);
    Uri toUri = new Uri(toPath);

    if (fromUri.Scheme != toUri.Scheme) 
    {
         return toPath; // path can't be made relative.
    }

    var relativeUri = fromUri.MakeRelativeUri(toUri);
    var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

    if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
    {
        relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    }

    return relativePath;
}

RunTarget(target);
