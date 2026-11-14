using Jla.Contracts;

namespace Jla.Tests.Unit;

public class ContractDefaultsTests
{
    [Fact]
    public void Default_limits_match_the_design_doc()
    {
        var l = new ExecutionLimits();
        Assert.Equal(2.00m, l.MaxCostUsd);
        Assert.Equal(0.20m, l.ReservationMargin);
        Assert.Equal(50, l.MaxLlmCalls);
        Assert.Equal(15, l.ApprovalExpiryMinutes);
    }

    [Fact]
    public void Approval_classes_order_so_that_Unclassified_is_the_strictest()
    {
        Assert.True(ApprovalClass.ReadOnly < ApprovalClass.ReversibleOrInternal);
        Assert.True(ApprovalClass.ReversibleOrInternal < ApprovalClass.Consequential);
        Assert.True(ApprovalClass.Consequential < ApprovalClass.Unclassified);
    }

    [Fact]
    public void Segment_trust_distinguishes_requester_intent_from_untrusted_data()
    {
        Assert.Equal(["PlatformInstruction", "RequesterIntent", "UntrustedData"], Enum.GetNames<SegmentTrust>());
    }
}
