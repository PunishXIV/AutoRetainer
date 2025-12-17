using AutoRetainer.Modules.Voyage;
using ECommons.ExcelServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using ECommons.GameHelpers;

namespace AutoRetainer.Scheduler.Tasks;
public static class TaskTeleportToProperty
{
    public static uint[] Apartments = [Houses.Ingleside_Apartment, Houses.Kobai_Goten_Apartment, Houses.Lily_Hills_Apartment, Houses.Sultanas_Breath_Apartment, Houses.Topmast_Apartment];
    public static bool EnqueueIfNeededAndPossible(bool isSubmersibleOperation)
    {
        if(Player.Territory.EqualsAny(VoyageUtils.Workshops)) return false;
        if(!isSubmersibleOperation && C.NoTeleportHetWhenNextToBell && Utils.GetReachableRetainerBell(false) != null) return false;
        var fcTeleportEnabled = (Data.GetAllowFcTeleportForRetainers() && !isSubmersibleOperation) || (Data.GetAllowFcTeleportForSubs() && isSubmersibleOperation);
        var data = S.LifestreamIPC.GetHousePathData(Player.CID);
        var sharedData = S.LifestreamIPC.GetSharedHousePathData();
        var info = S.LifestreamIPC.GetCurrentPlotInfo();
        {
            var canPrivate = Data.GetAllowPrivateTeleportForRetainers() && data.Private != null && data.Private.PathToEntrance.Count > 0;
            var canShared = Data.GetAllowSharedTeleportForRetainers() && sharedData != null && sharedData.PathToEntrance.Count > 0;
            PluginLog.Information($"CanShared: {canShared}");
            var canFc = (fcTeleportEnabled && data.FC != null && data.FC.PathToEntrance.Count > 0);
            if((isSubmersibleOperation || !(canPrivate || canShared)) && canFc)
            {
                PluginLog.Information($"Teleport: fc=true, canShared={canShared}");
                return Process(true, canShared);
            }
            if(!isSubmersibleOperation && (canPrivate || canShared))
            {
                PluginLog.Information($"Teleport: fc=false, canShared={canShared}");
                return Process(false, canShared);
            }
        }

        if(C.AllowSimpleTeleport)
        {
            var canFc = fcTeleportEnabled && S.LifestreamIPC.HasFreeCompanyHouse() != false;
            var canPrivate = Data.GetAllowPrivateTeleportForRetainers() && S.LifestreamIPC.HasPrivateHouse() != false;
            var canShared = Data.GetAllowSharedTeleportForRetainers() && S.LifestreamIPC.HasSharedEstate() != false;
            if((isSubmersibleOperation || !(canPrivate || canShared)) && canFc)
            {
                PluginLog.Information($"Simple Teleport: fc=true, canShared={canShared}");
                return ProcessSimple(true, canShared);
            }
            if(!isSubmersibleOperation && (canPrivate || canShared))
            {
                PluginLog.Information($"Simple Teleport: fc=false, canShared={canShared}");
                return ProcessSimple(false, canShared);
            }
        }

        if(!isSubmersibleOperation && Data.GetIsTeleportEnabledForRetainers())
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
                    }, new(timeLimitMS: 5 * 60 * 1000));
                    return true;
                }
            }
            //inn logic
            if(!Inns.List.Contains((ushort)Player.Territory))
            {
                P.TaskManager.Enqueue(() => S.LifestreamIPC.EnqueueInnShortcut(null));
                P.TaskManager.Enqueue(() =>
                {
                    if(!Svc.ClientState.IsLoggedIn)
                    {
                        PluginLog.Warning($"Logout while waiting to return to home; expecting DC travel. Aborting and waiting for relogging.");
                        return null;
                    }
                    return IsScreenReady() && Player.Interactable && Inns.List.Contains((ushort)Player.Territory) && !S.LifestreamIPC.IsBusy();
                }, new(timeLimitMS: 5 * 60 * 1000));
                return true;
            }
        }

        //if at this point no decision was made, just invoke HET if needed, enter any house and don't care about it

        if(ExcelTerritoryHelper.Get(Player.Territory)?.TerritoryIntendedUse.RowId == (uint)TerritoryIntendedUseEnum.Residential_Area)
        {
            if(TaskNeoHET.IsInMarkerHousingPlot([.. TaskNeoHET.PrivateMarkers, .. TaskNeoHET.FcMarkers, .. (C.SharedHET ? TaskNeoHET.SharedMarkers : [])]))
            {
                TaskNeoHET.Enqueue(null);
                return true;
            }
            else if(TaskNeoHET.GetApartmentEntrance() != null && Player.DistanceTo(TaskNeoHET.GetApartmentEntrance()) < 40f)
            {
                TaskNeoHET.Enqueue(null);
                return true;
            }
        }

        return false;

        bool Process(bool fc, bool shared)
        {
            var pathData = fc ? data.FC : (shared?sharedData:data.Private);
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
            P.TaskManager.Enqueue(() => S.LifestreamIPC.EnqueuePropertyShortcut(fc ? 2 : (shared?5:1), 1));
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

        bool ProcessSimple(bool fc, bool shared)
        {
            var isHere = TaskNeoHET.IsInMarkerHousingPlot(fc ? TaskNeoHET.FcMarkers : (shared?TaskNeoHET.SharedMarkers:TaskNeoHET.PrivateMarkers));
            var noProperty = !(fc ? S.LifestreamIPC.HasFreeCompanyHouse() : (shared?S.LifestreamIPC.HasSharedEstate() != false:S.LifestreamIPC.HasPrivateHouse()));
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
            P.TaskManager.Enqueue(() => S.LifestreamIPC.EnqueuePropertyShortcut(fc ? 2 : (shared?5:1), 1));
            P.TaskManager.Enqueue(() =>
            {
                if(!Svc.ClientState.IsLoggedIn)
                {
                    PluginLog.Warning($"Logout while waiting to return to home; expecting DC travel. Aborting and waiting for relogging.");
                    return null;
                }
                return Player.Interactable
                && Player.Territory.EqualsAny([.. ResidentalAreas.List])
                && !S.LifestreamIPC.IsBusy();
            }, new(timeLimitMS: 5 * 60 * 1000));
            TaskNeoHET.Enqueue(null);
            return true;
        }
    }

    public static bool ShouldVoidHET()
    {
        if(!Player.Available) return false;
        if(Data == null) return true;
        var subsSoon = Data.WorkshopEnabled && Data.AnyEnabledVesselsAvailable() && MultiMode.EnabledSubmarines && (!Data.ShouldWaitForAllWhenLoggedIn() || Data.AreAnyEnabledVesselsReturnInNext(1, true));
        var retainersSoon = MultiMode.AnyRetainersAvailable(0) && MultiMode.EnabledRetainers;
        var blockHet = subsSoon || retainersSoon;
        if(C.AllowSimpleTeleport && (Data.GetAllowFcTeleportForRetainers() || Data.GetAllowPrivateTeleportForRetainers() || Data.GetAllowSharedTeleportForRetainers())) return blockHet;
        var data = S.LifestreamIPC.GetHousePathData(Player.CID);
        var sharedData = S.LifestreamIPC.GetSharedHousePathData();
        if(Data.GetAllowFcTeleportForRetainers() && data.FC != null && data.FC.PathToEntrance.Count > 0) return blockHet;
        if(Data.GetAllowPrivateTeleportForRetainers() && data.Private != null && data.Private.PathToEntrance.Count > 0) return blockHet;
        if(Data.GetAllowSharedTeleportForRetainers() && sharedData != null && sharedData.PathToEntrance.Count > 0) return blockHet;
        return false;
    }
}
