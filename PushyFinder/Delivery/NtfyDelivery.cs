using Dalamud.Utility;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.System.Scheduler.Clip.BaseClip.Delegates;

namespace PushyFinder.Delivery;

public class NtfyDelivery : IDelivery
{
    public bool IsActive => !Plugin.Configuration.NtfyServer.IsNullOrWhitespace() &&
                            !Plugin.Configuration.NtfyTopic.IsNullOrWhitespace() && 
                            Uri.IsWellFormedUriString(Plugin.Configuration.NtfyServer, UriKind.Absolute);

    public void Deliver(string title, string text)
    {
        Task.Run(() => DeliverAsync(title, text));
    }

    private static async Task DeliverAsync(string title, string text)
    {
        // Assuming `title` is used as the main message
        // Since ntfy doesn't have a separate title field, you can prepend it to the message or just send the message
       
        IFlurlRequest request = new FlurlRequest(Plugin.Configuration.NtfyServer);
        if (!Plugin.Configuration.NtfyTopic.IsNullOrWhitespace())
            request = request.AppendPathSegment(Plugin.Configuration.NtfyTopic);
        if (!Plugin.Configuration.NtfyToken.IsNullOrWhitespace())
            request = request.WithOAuthBearerToken(Plugin.Configuration.NtfyToken);

        try
        {
            request = request.AppendPathSegment("publish");
            request.AppendQueryParam("message",$"{title}:{text}");
            await request.GetStringAsync();
            Service.PluginLog.Debug("Sent Ntfy message");
        }
        catch (FlurlHttpException e)
        {
            Service.PluginLog.Error($"Failed to make Ntfy request: '{e.Message}'");
            Service.PluginLog.Error($"{e.StackTrace}");
        }
        catch (ArgumentException e)
        {
            Service.PluginLog.Error($"{e.StackTrace}");
        }

    }
}
