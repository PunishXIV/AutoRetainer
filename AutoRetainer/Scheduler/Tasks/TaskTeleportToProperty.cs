using AutoRetainer.Modules.Voyage;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;

namespace AutoRetainer.Scheduler.Tasks;
public static class TaskTeleportToProperty
{
    public static bool EnqueueIfNeededAndPossible(bool requireFc)
    {
        if(Player.Territory.EqualsAny(VoyageUtils.Workshops)) return false;
        var data = S.LifestreamIPC.GetHousePathData(Player.CID);
        var canPrivate = C.AllowPrivateTeleport && data.Private != null && data.Private.PathToEntrance.Count > 0;
        var canFc = C.AllowFcTeleport && data.FC != null && data.FC.PathToEntrance.Count > 0;
        var info = S.LifestreamIPC.GetCurrentPlotInfo();
        if((requireFc || !canPrivate) && canFc)
        {
            return Process(true);
        }
        if(!requireFc && canPrivate)
        {
            return Process(false);
        }
        return false;

        bool Process(bool fc)
        {
            var pathData = fc ? data.FC : data.Private;
            if(info != null
                && info.Value.Plot == pathData.Plot
                && info.Value.Ward == pathData.Ward
                && info.Value.Kind == pathData.ResidentialDistrict)
            {
                if(Player.Territory.EqualsAny([.. Houses.List]))
                {
                    return false;
                }
                else
                {
                    HouseEnterTask.EnqueueTask();
                    return true; //already here
                }
            }
            P.TaskManager.Enqueue(() => S.LifestreamIPC.EnqueuePropertyShortcut(fc?2:1, 1));
            P.TaskManager.Enqueue(() =>
            {
                if(!Svc.ClientState.IsLoggedIn)
                {
                    PluginLog.Warning($"Logout while waiting to return to home; expecting DC travel. Aborting and waiting for relogging.");
                    return null;
                }
                return Player.Interactable
                && S.LifestreamIPC.GetCurrentPlotInfo()?.Plot == pathData.Plot
                && S.LifestreamIPC.GetCurrentPlotInfo()?.Ward == pathData.Ward
                && S.LifestreamIPC.GetCurrentPlotInfo()?.Kind == pathData.ResidentialDistrict
                && !S.LifestreamIPC.IsBusy();
            }, 5 * 60 * 1000);
            HouseEnterTask.EnqueueTask();
            return true;
        }
    }

    public static bool HasRegisteredProperty()
    {
        var data = S.LifestreamIPC.GetHousePathData(Player.CID);
        if(data.FC != null && data.FC.PathToEntrance.Count > 0) return true;
        if(data.Private != null && data.Private.PathToEntrance.Count > 0) return true;
        return false;
    }
}
