using Nrk.FluentCore.Authentication;
using Nrk.FluentCore.GameManagement.Instances;
using System;
using System.IO;

namespace Natsurainko.FluentLauncher.Exceptions;

internal class InstanceDirectoryNotFoundException(MinecraftInstance instance, string directory) : DirectoryNotFoundException(
    $"Unable to find the directory to run Minecraft instance {instance.InstanceId} in: ${directory}, " +
    "please check if this is a valid game instance")
{
    public MinecraftInstance MinecraftInstance { get; } = instance;

    public string Directory { get; } = directory;
}

internal class NoActiveJavaRuntimeException() : Exception(
    "There is no Java runtime in the settings, or no Java runtime is enabled. Please check your settings.");

internal class JavaRuntimeFileNotFoundException(string file) : FileNotFoundException(
    $"Unable to find file path for Java runtime : ${file}, please check if this is a valid runtime", file);

internal class X86JavaRuntimeMemoryException() : Exception(
    "Using a 32-bit Java, but have allocated more than 1(or 0.5) GB of memory. " +
    "Please use a 64-bit Java, or turn off automatic memory allocation and manually allocate less than 1(or 0.5) GB of memory.");

internal class JavaRuntimeIncompatibleException(int targetJavaVersion) : Exception(
    $"No suitable version of Java found to start this game, version {targetJavaVersion} is required")
{
    public int TargetJavaVersion { get; } = targetJavaVersion;
}

internal class NoActiveAccountException() : Exception(
    "There is no Minecraft account in the settings, or no Minecraft account is enabled. Please check your settings.");

internal class AccountNotFoundException(Account account) : Exception(
    $"The Minecraft account {account.Name} specified for this instance could not be found in the settings. Please check the settings")
{
    public Account Account { get; } = account;
}