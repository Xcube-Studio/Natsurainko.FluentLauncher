# Set the path of the XML file
$xmlFilePath = "Natsurainko.FluentLauncher\Natsurainko.FluentLauncher.csproj"

# Load the XML file
$xmlFile = [xml](Get-Content $xmlFilePath)

# Extract the nodes named 'PackageReference'
$packageReferences = $xmlFile.SelectNodes('//PackageReference')

# Create a new XML document to store the nodes
$newXmlDocument = New-Object System.Xml.XmlDocument

# Add a root element to the new document
$rootElement = $newXmlDocument.CreateElement("root")
$newXmlDocument.AppendChild($rootElement)

# Copy each 'PackageReference' node to the new document
foreach ($node in $packageReferences) {
    $importedNode = $newXmlDocument.ImportNode($node, $true)
    $rootElement.AppendChild($importedNode)
}

# Set the path of the new XML file
$newXmlFilePath = ".package-references.xml"

# Save the new XML document to a file
$newXmlDocument.Save($newXmlFilePath)
