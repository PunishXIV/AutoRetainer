using AutoRetainer.UiHelpers;
using ECommons.Automation.NeoTaskManager.Tasks;
using ECommons.Throttlers;
using ECommons.UIHelpers.AddonMasterImplementations;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace AutoRetainer.Scheduler.Tasks;
public unsafe class TaskRecursivelyBuyFuel
{
    private static uint Amount = 0;
    public static void Enqueue(bool ignoreMax = false)
    {
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonMaster<AddonMaster.SelectYesno>(out var m))
            {
                if(m.Text.ContainsAny(StringComparison.OrdinalIgnoreCase, "ceruleum", "青燐水バレル", "Erdseim", "céruleum"))
                {
                    if(EzThrottler.Throttle("CeruleumYesNo")) m.Yes();
                }
            }
            if(TryGetAddonByName<AtkUnitBase>("FreeCompanyCreditShop", out var a) && IsAddonReady(a))
            {
                var reader = new ReaderFreeCompanyCreditShop(a);
                if(Amount != reader.Credits)
                {
                    EzThrottler.Reset("CeruleumYesNo");
                    EzThrottler.Reset("FCBuy");
                    Amount = reader.Credits;
                }
                if(reader.Credits < 100) return true;
                var playerHas = Utils.CountItemsInInventory(10155, null, Utils.PlayerEntireInventory);
                var remaining = ignoreMax?(int)Utils.GetAmountThatCanFit(Utils.PlayerInvetories, 10155, false, out _):(C.AutoFuelPurchaseMax - playerHas);
                if(remaining <= 0)
                {
                    return true;
                }
                var amount = Math.Clamp(reader.Credits / 100, 1, Math.Min(99, remaining));
                var numeric = a->GetComponentNodeById(27)->Component->GetNodeById(5)->GetAsAtkComponentListItemRenderer()->GetNodeById(5)->GetAsAtkComponentNumericInput();
                if(numeric->Value != amount)
                {
                    numeric->SetValue((int)amount);
                }
                if(EzThrottler.Throttle("FCBuy", 2000))
                {
                    new FreeCompanyCreditShop(a).Buy(0);
                }
            }
            else
            {
                return null;
            }
            return false;
        }, new(timeLimitMS: 1000 * 60 * 10));
        P.TaskManager.Enqueue(() => Utils.TryNotify("Finished purchasing Ceruleum Tanks"));
    }

    public static void EnqueueNpcInteraction()
    {
        P.TaskManager.EnqueueTask(NeoTasks.ApproachObjectViaAutomove(() => Svc.Objects.First(x => x.IsTargetable && x.DataId == 1011274), 6.3f));
        P.TaskManager.EnqueueTask(NeoTasks.InteractWithObject(() => Svc.Objects.First(x => x.IsTargetable && x.DataId == 1011274)));
        P.TaskManager.Enqueue(() => TryGetAddonMaster<AddonMaster.SelectIconString>(out var m) && m.IsAddonReady);
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonByName<AtkUnitBase>("FreeCompanyCreditShop", out var a) && IsAddonReady(a))
            {
                return true;
            }
            if(TryGetAddonMaster<AddonMaster.SelectIconString>(out var m) && m.IsAddonReady)
            {
                if(EzThrottler.Throttle("SelectShop"))
                {
                    foreach(var x in m.Entries)
                    {
                        if(x.Text.Contains(Svc.Data.GetExcelSheet<FccShop>().GetRow(2752515).Name.GetText()))
                        {
                            x.Select();
                            return false;
                        }
                    }
                }
            }
            return false;
        });
    }

    public static void EnqueueShopClosure()
    {
        P.TaskManager.Enqueue(() =>
        {
            if(TryGetAddonByName<AtkUnitBase>("FreeCompanyCreditShop", out var a) && IsAddonReady(a))
            {
                if(EzThrottler.Throttle("CloseShop"))
                {
                    Callback.Fire(a, true, -1);
                }
                return false;
            }
            else
            {
                return true;
            }
        });
    }
}
