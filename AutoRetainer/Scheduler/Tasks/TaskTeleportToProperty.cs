using AutoRetainer.Modules.Voyage;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;

namespace AutoRetainer.Scheduler.Tasks;
public static class TaskTeleportToProperty
{
    public static uint[] Apartments = [Houses.Ingleside_Apartment, Houses.Kobai_Goten_Apartment, Houses.Lily_Hills_Apartment, Houses.Sultanas_Breath_Apartment, Houses.Topmast_Apartment];
    public static bool EnqueueIfNeededAndPossible(bool requireFc)
    {
        if(Player.Territory.EqualsAny(VoyageUtils.Workshops)) return false;
        var data = S.LifestreamIPC.GetHousePathData(Player.CID);
        var info = S.LifestreamIPC.GetCurrentPlotInfo();
        {
            var canPrivate = Data.GetAllowPrivateTeleportForRetainers() && data.Private != null && data.Private.PathToEntrance.Count > 0;
            var canFc = C.AllowFcTeleport && data.FC != null && data.FC.PathToEntrance.Count > 0 && (requireFc || !Data.DisableFcHouseTeleport);
            if((requireFc || !canPrivate) && canFc)
            {
                return Process(true);
            }
            if(!requireFc && canPrivate)
            {
                return Process(false);
            }
        }

        if(C.AllowSimpleTeleport)
        {
            var canFc = C.AllowFcTeleport && S.LifestreamIPC.HasFreeCompanyHouse() != false && (requireFc || !Data.DisableFcHouseTeleport);
            var canPrivate = Data.GetAllowPrivateTeleportForRetainers() && S.LifestreamIPC.HasPrivateHouse() != false;
            if((requireFc || !canPrivate) && canFc)
            {
                return ProcessSimple(true);
            }
            if(!requireFc && canPrivate)
            {
                return ProcessSimple(false);
            }
        }

        if(!requireFc && C.AllowRetireInnApartment)
        {
            //apartment logic
            if(Data.GetAllowApartmentTeleportForRetainers())
            {
                if(S.LifestreamIPC.HasApartment() == true && Apartments.Contains(Player.Territory)) return false;
                if(S.LifestreamIPC.HasApartment() != false)
                {
                    P.TaskManager.Enqueue(() => S.LifestreamIPC.EnterApartment(true));
                    P.TaskManager.Enqueue(() =>
                    {
                        if(!Svc.ClientState.IsLoggedIn)
                        {
                            PluginLog.Warning($"Logout while waiting to return to home; expecting DC travel. Aborting and waiting for relogging.");
                            return null;
                        }
                        if(Player.Interactable && S.LifestreamIPC.HasApartment() == false)
                        {
                            PluginLog.Warning("Upon returning home, apartment not found. Aborting and retrying.");
                            return null;
                        }
                        return IsScreenReady() && Player.Interactable && Apartments.Contains(Player.Territory) && !S.LifestreamIPC.IsBusy();
                    }, new(timeLimitMS:5 * 60 * 1000));
                    return true;
                }
            }
            //inn logic
            if(!Inns.List.Contains((ushort)Player.Territory))
            {
                P.TaskManager.Enqueue(() => S.LifestreamIPC.EnqueueInnShortcut(1));
                P.TaskManager.Enqueue(() =>
                {
                    if(!Svc.ClientState.IsLoggedIn)
                    {
                        PluginLog.Warning($"Logout while waiting to return to home; expecting DC travel. Aborting and waiting for relogging.");
                        return null;
                    }
                    return IsScreenReady() && Player.Interactable && Inns.List.Contains((ushort)Player.Territory) && !S.LifestreamIPC.IsBusy();
                }, new(timeLimitMS:5 * 60 * 1000));
                return true;
            }
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
                    TaskNeoHET.Enqueue(null);
                    return true; //already here
                }
            }
            P.TaskManager.Enqueue(() => S.LifestreamIPC.EnqueuePropertyShortcut(fc ? 2 : 1, 1));
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
            }, new(timeLimitMS: 5 * 60 * 1000));
            TaskNeoHET.Enqueue(null);
            return true;
        }

        bool ProcessSimple(bool fc)
        {
            var isHere = TaskNeoHET.IsInMarkerTerritory(fc?TaskNeoHET.FcMarkers:TaskNeoHET.PrivateMarkers);
            var noProperty = !(fc ? S.LifestreamIPC.HasFreeCompanyHouse() : S.LifestreamIPC.HasPrivateHouse());
            if(noProperty == true)
            {
                return false;
            }
            if(Player.Territory.EqualsAny([.. Houses.List]) && (!fc || TaskNeoHET.GetWorkshopEntrance() != null))
            {
                return false;
            }
            else if(isHere)
            {
                TaskNeoHET.Enqueue(null);
                return true; //already here
            }
            P.TaskManager.Enqueue(() => S.LifestreamIPC.EnqueuePropertyShortcut(fc ? 2 : 1, 1));
            P.TaskManager.Enqueue(() =>
            {
                if(!Svc.ClientState.IsLoggedIn)
                {
                    PluginLog.Warning($"Logout while waiting to return to home; expecting DC travel. Aborting and waiting for relogging.");
                    return null;
                }
                return Player.Interactable
                && Player.Territory.EqualsAny([..ResidentalAreas.List])
                && !S.LifestreamIPC.IsBusy();
            }, new(timeLimitMS: 5 * 60 * 1000));
            TaskNeoHET.Enqueue(null);
            return true;
        }
    }

    public static bool ShouldVoidHET()
    {
        var subsSoon = Data.WorkshopEnabled && Data.AnyEnabledVesselsAvailable() && MultiMode.EnabledSubmarines && (!C.MultiModeWorkshopConfiguration.WaitForAllLoggedIn || Data.AreAnyEnabledVesselsReturnInNext(1, true));
        var retainersSoon = MultiMode.AnyRetainersAvailable(60) && MultiMode.EnabledRetainers;
        var blockHet = subsSoon || retainersSoon;
        if(C.AllowSimpleTeleport && (Data.GetAllowFcTeleportForRetainers() || Data.GetAllowPrivateTeleportForRetainers())) return blockHet;
        var data = S.LifestreamIPC.GetHousePathData(Player.CID);
        if(Data.GetAllowFcTeleportForRetainers() && data.FC != null && data.FC.PathToEntrance.Count > 0) return blockHet;
        if(Data.GetAllowPrivateTeleportForRetainers() && data.Private != null && data.Private.PathToEntrance.Count > 0) return blockHet;
        return false;
    }
}
