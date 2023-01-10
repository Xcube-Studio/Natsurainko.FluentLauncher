using Natsurainko.FluentCore.Class.Model.Parser;
using Natsurainko.FluentCore.Interface;
using Natsurainko.Toolkits.Values;
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace Natsurainko.FluentCore.Class.Model;

public class XmlProcessOutput : IProcessOutput
{
    public string Raw { get; private set; }

    private static XmlReaderSettings XmlReaderSettings;

    private static XmlNamespaceManager XmlNamespaceManager;

    private static XmlParserContext XmlParserContext;

    public bool Invalid { get; private set; }

    static XmlProcessOutput()
    {
        XmlReaderSettings = new XmlReaderSettings { NameTable = new NameTable() };
        XmlNamespaceManager = new XmlNamespaceManager(XmlReaderSettings.NameTable);

        XmlNamespaceManager.AddNamespace("log4j", "http://jakarta.apache.org/log4j/");
        XmlParserContext = new XmlParserContext(null, XmlNamespaceManager, "", XmlSpace.Default);
    }

    public XmlProcessOutput(string output, bool invalid = false)
    {
        this.Raw = output;
        this.Invalid = invalid;

        if (!this.Invalid)
        {
            var index = this.Raw.IndexOf("]></log4j:Message>");
            if (this.Raw[index - 1] != ']')
                this.Raw = this.Raw.Insert(index, "]");
        }
    }

    public ProcessOutputXmlEntity GetContent()
    {
        if (this.Invalid)
            return new ProcessOutputXmlEntity { Message = this.Raw };

        var xml = new XmlDocument();

        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(this.Raw));
        using var reader = XmlReader.Create(memoryStream, XmlReaderSettings, XmlParserContext);
        xml.Load(reader);

        var entity = new ProcessOutputXmlEntity()
        {
            Logger = xml["log4j:Event"].Attributes["logger"].Value,
            Level = xml["log4j:Event"].Attributes["level"].Value,
            Thread = xml["log4j:Event"].Attributes["thread"].Value,
            DateTime = long.Parse(xml["log4j:Event"].Attributes["timestamp"].Value).ToDateTime(),
            Message = xml["log4j:Event"]["log4j:Message"].InnerText
        };

        if (xml["log4j:Event"]["log4j:Throwable"] != null)
            entity.Throwable = xml["log4j:Event"]["log4j:Throwable"].InnerText.TrimEnd('\r', '\n');

        return entity;
    }

    public string GetPrintValue()
    {
        if (this.Invalid)
            return this.Raw;

        var entity = GetContent();

        return $"[{entity.DateTime:HH:mm:ss}] [{entity.Thread}/{entity.Level}]: {entity.Message}\r\n" +
            entity.Throwable;
    }

    public void Print()
    {
        if (this.Invalid)
        {
            Console.WriteLine(this.Raw);
            return;
        }

        var entity = GetContent();

        switch (entity.Level)
        {
            case "WARN":
                Console.ForegroundColor = ConsoleColor.Yellow;
                break;
            case "ERROR":
                Console.ForegroundColor = ConsoleColor.Red;
                break;
            case "FATAL":
                Console.ForegroundColor = ConsoleColor.DarkRed;
                break;
            case "DEBUG":
                Console.ForegroundColor = ConsoleColor.Blue;
                break;
            default:
                Console.ResetColor();
                break;
        }
        Console.WriteLine($"[{entity.DateTime:HH:mm:ss}] [{entity.Thread}/{entity.Level}]: {entity.Message}");

        if (entity.Throwable != null)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(entity.Throwable);
        }

        Console.ResetColor();
    }
}
