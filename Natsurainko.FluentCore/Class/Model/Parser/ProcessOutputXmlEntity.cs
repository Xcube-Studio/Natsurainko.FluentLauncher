using System;

namespace Natsurainko.FluentCore.Class.Model.Parser;

public class ProcessOutputXmlEntity
{
    public string Logger { get; set; }

    public DateTime DateTime { get; set; }

    public string Thread { get; set; }

    public string Level { get; set; }

    public string Message { get; set; }

    public string Throwable { get; set; }
}
