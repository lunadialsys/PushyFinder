using System.Collections.Generic;
using System.Linq;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace PushyFinder.Util;

public static class CrossWorldPartyListSystem
{
    public delegate void CrossWorldJoinDelegate(CrossWorldMember m);

    public delegate void CrossWorldLeaveDelegate(CrossWorldMember m);

    private static readonly List<CrossWorldMember> members = new();
    private static List<CrossWorldMember> oldMembers = new();

    public static event CrossWorldJoinDelegate? OnJoin;
    public static event CrossWorldLeaveDelegate? OnLeave;

    public static void Start()
    {
        Service.Framework.Update += Update;
    }

    public static void Stop()
    {
        Service.Framework.Update -= Update;
    }

    private static bool ListContainsMember(List<CrossWorldMember> l, CrossWorldMember m)
        => l.Any(a => a.ContentId == m.ContentId);

    private static unsafe void Update(IFramework framework)
    {
        if (!Service.ClientState.IsLoggedIn)
            return;

        if (!InfoProxyCrossRealm.IsCrossRealmParty())
            return;

        members.Clear();

        // CrossRealmGroups contains the separate groups of an alliance. The
        // old default GetGroupMember call only returned the local 8-person group.
        var proxy = InfoProxyCrossRealm.Instance();
        var groupCount = proxy == null ? 1 : proxy->GroupCount;
        groupCount = (byte)System.Math.Clamp(groupCount, 1, 6);
        var isAlliance = groupCount > 1;

        var partyCount = 0;
        for (var group = 0; group < groupCount; group++)
            partyCount += InfoProxyCrossRealm.GetGroupMemberCount(group);

        for (var group = 0; group < groupCount; group++)
        {
            var groupMemberCount = InfoProxyCrossRealm.GetGroupMemberCount(group);
            for (var i = 0u; i < groupMemberCount; i++)
            {
                var addr = InfoProxyCrossRealm.GetGroupMember(i, group);
                if (addr == null || addr->ContentId == 0)
                    continue;

                members.Add(new CrossWorldMember
                {
                    ContentId = addr->ContentId,
                    Name = addr->NameString,
                    PartyCount = partyCount,
                    IsAlliance = isAlliance,
                    Level = addr->Level,
                    JobId = addr->ClassJobId
                });
            }
        }

        // Compare the snapshots even when their counts match; an outgoing and
        // incoming member can be observed in the same update.
        foreach (var i in members)
            if (!ListContainsMember(oldMembers, i))
                OnJoin?.Invoke(i);

        foreach (var i in oldMembers)
            if (!ListContainsMember(members, i))
                OnLeave?.Invoke(i);
        
        // Fix potential broken references caused by memory semantics
        oldMembers = members.ToList();
    }

    public struct CrossWorldMember
    {
        public ulong ContentId;
        public string Name;
        public int PartyCount;
        public bool IsAlliance;
        public uint Level;
        public uint JobId;
    }
}
