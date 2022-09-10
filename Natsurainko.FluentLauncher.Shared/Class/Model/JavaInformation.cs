using Newtonsoft.Json;

namespace Natsurainko.FluentLauncher.Shared.Class.Model;

public class JavaInformation
{
    [JsonProperty("JAVA_VM_VERSION")]
    public string JavaVirtualMachineVersion { get; set; }

    [JsonProperty("JAVA_VM_NAME")]
    public string JavaVirtualMachineName { get; set; }

    [JsonProperty("JAVA_VM_VENDOR")]
    public string JavaVirtualMachineVendor { get; set; }

    [JsonProperty("JAVA_VERSION")]
    public string JavaVersion { get; set; }

    [JsonProperty("JAVA_VENDOR")]
    public string JavaVendor { get; set; }

    public override int GetHashCode()
        => this.JavaVirtualMachineVersion.GetHashCode() ^ this.JavaVirtualMachineName.GetHashCode()
        ^ this.JavaVirtualMachineVendor.GetHashCode() ^ this.JavaVersion.GetHashCode() ^ this.JavaVendor.GetHashCode();

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;

        var item = (JavaInformation)obj;

        return this.JavaVirtualMachineVersion == item.JavaVirtualMachineVersion
            && this.JavaVirtualMachineName == item.JavaVirtualMachineName
            && this.JavaVirtualMachineVendor == item.JavaVirtualMachineVendor
            && this.JavaVersion == item.JavaVersion
            && this.JavaVendor == item.JavaVendor;
    }
}
