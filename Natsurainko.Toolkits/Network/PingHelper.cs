using Natsurainko.Toolkits.Network.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Natsurainko.Toolkits.Network;

public class PingHelper
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>Such as: 192.168.2</returns>
    public static string GetLANAddress()
    {
        var localAddress = Dns.GetHostEntry(Dns.GetHostName()).AddressList.Where(x => x.ToString().StartsWith("192.168."))
            .ToArray()[0].ToString().Split('.').ToList();

        localAddress.RemoveAt(localAddress.Count - 1);
        return string.Join(".", localAddress);
    }

    public static async Task<List<string>> GetHostsAsync()
    {
        var lanAddress = GetLANAddress();
        var hosts = new List<string>();

        var transformBlock = new TransformBlock<string, string>(x => x);
        var actionBlock = new ActionBlock<string>(async address =>
        {
            try
            {
                using var ping = new Ping();
                var pingReply = await ping.SendPingAsync(IPAddress.Parse(address), 1000);

                if (pingReply.Status == IPStatus.Success)
                    hosts.Add(pingReply.Address.ToString());
            }
            catch { }
        }, new ExecutionDataflowBlockOptions
        {
            BoundedCapacity = 256,
            MaxDegreeOfParallelism = 256
        });
        using var disposable = transformBlock.LinkTo(actionBlock, new DataflowLinkOptions { PropagateCompletion = true });

        for (int i = 0; i < 256; i++)
            transformBlock.Post($"{lanAddress}.{i}");

        transformBlock.Complete();
        await actionBlock.Completion;

        GC.Collect();

        return hosts;
    }

    public static string GetLocalIpAddress()
        => Dns.GetHostAddresses(Dns.GetHostName()).Where(x => x.ToString().StartsWith("192.168.")).ToArray()[0].ToString();
}
