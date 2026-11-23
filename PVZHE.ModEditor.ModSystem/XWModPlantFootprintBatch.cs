using System;
using System.Collections.Generic;
using System.Linq;

namespace PVZHE.ModEditor.ModSystem;

internal sealed class XWModPlantFootprintBatch : IDisposable
{
	internal IReadOnlyList<XWModPlantFootprintPlan> Plans { get; }

	internal long RefundTotal => Plans.Sum((XWModPlantFootprintPlan plan) => plan.RefundTotal);

	internal bool RefundAsSunDrops => Plans[0].RefundAsSunDrops;

	internal bool IsPendingValid => Plans.All((XWModPlantFootprintPlan plan) => plan.IsPendingValid);

	internal XWModPlantFootprintBatch(List<XWModPlantFootprintPlan> plans)
	{
		Plans = plans.AsReadOnly();
	}

	internal bool Reserve()
	{
		foreach (XWModPlantFootprintPlan plan in Plans)
		{
			if (!plan.Reserve())
			{
				return false;
			}
		}
		return true;
	}

	internal bool TryPrepareRefundDrops(EconomyAccountId account)
	{
		foreach (XWModPlantFootprintPlan plan in Plans)
		{
			if (!plan.TryPrepareRefundDrops(account))
			{
				return false;
			}
		}
		return true;
	}

	internal void Commit()
	{
		foreach (XWModPlantFootprintPlan plan in Plans)
		{
			plan.MarkCommitted();
		}
		foreach (XWModPlantFootprintPlan plan2 in Plans)
		{
			plan2.CleanupReplacedSupports();
		}
	}

	public void Dispose()
	{
		for (int num = Plans.Count - 1; num >= 0; num--)
		{
			Plans[num].Dispose();
		}
	}
}
