using PushyFinder.Delivery;
using PushyFinder.Util;

namespace PushyFinder.Impl;

public static class PartyListener
{
    public static void On()
    {
        Service.PluginLog.Debug("PartyListener On");
        CrossWorldPartyListSystem.OnJoin += OnJoin;
        CrossWorldPartyListSystem.OnLeave += OnLeave;
    }

    public static void Off()
    {
        Service.PluginLog.Debug("PartyListener Off");
        CrossWorldPartyListSystem.OnJoin -= OnJoin;
        CrossWorldPartyListSystem.OnLeave -= OnLeave;
    }

    private static void OnJoin(CrossWorldPartyListSystem.CrossWorldMember m)
    {
        if (!CharacterUtil.IsClientAfk()) return;

        var jobAbbr = LuminaDataUtil.GetJobAbbreviation(m.JobId);

        if ((!m.IsAlliance && m.PartyCount == 8) || (m.IsAlliance && m.PartyCount == 24))
        {
            MasterDelivery.Deliver("Party full",
                                   $"{m.Name} (Lv{m.Level} {jobAbbr}) joins the party.\n{(m.IsAlliance ? "Alliance" : "Party")} recruitment ended. All spots have been filled.");
        }
        else
        {
            MasterDelivery.Deliver($"{m.PartyCount}/{(m.IsAlliance ? 24 : 8)}: Party join",
                                   $"{m.Name} (Lv{m.Level} {jobAbbr}) joins the party.");
        }
    }

    private static void OnLeave(CrossWorldPartyListSystem.CrossWorldMember m)
    {
        if (!CharacterUtil.IsClientAfk()) return;

        var jobAbbr = LuminaDataUtil.GetJobAbbreviation(m.JobId);

        MasterDelivery.Deliver($"{m.PartyCount - 1}/{(m.IsAlliance ? 24 : 8)}: Party leave",
                               $"{m.Name} (Lv{m.Level} {jobAbbr}) has left the party.");
    }
}
