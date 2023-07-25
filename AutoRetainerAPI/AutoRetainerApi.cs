using AutoRetainerAPI.Configuration;
using ECommons;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using static AutoRetainerAPI.Delegates;

namespace AutoRetainerAPI
{
    public partial class AutoRetainerApi : IDisposable
    {
        /// <summary>
        /// Event which is fired when a retainer is about to be sent to a venture.
        /// </summary>
        public event OnSendRetainerToVentureDelegate OnSendRetainerToVenture;

        /// <summary>
        /// Event which is fired when a retainer is processed and ready to receive additional postprocess tasks
        /// </summary>
        public event OnRetainerPostprocessTaskDelegate OnRetainerPostprocessStep;

        /// <summary>
        /// Event which is fired when your plugin should do additional post-process tasks. You must return game UI into the state from where you picked it up.
        /// </summary>
        public event OnRetainerReadyToPostprocessDelegate OnRetainerReadyToPostprocess;

        /// <summary>
        /// Event which is fired every time retainer's settings are displayed
        /// </summary>
        public event OnRetainerSettingsDrawDelegate OnRetainerSettingsDraw;

        /// <summary>
        /// Event which is fired every time post-venture tasks are displayed
        /// </summary>
        public event OnRetainerPostVentureTaskDrawDelegate OnRetainerPostVentureTaskDraw;

        /// <summary>
        /// Event which is fired every time task buttons are displayed in retainer list
        /// </summary>
        public event OnRetainerListTaskButtonsDrawDelegate OnRetainerListTaskButtonsDraw;

        public AutoRetainerApi()
        {
            Svc.PluginInterface.GetIpcSubscriber<string, object>(ApiConsts.OnSendRetainerToVenture).Subscribe(OnSendRetainerToVentureAction);
            Svc.PluginInterface.GetIpcSubscriber<string, object>(ApiConsts.OnRetainerAdditionalTask).Subscribe(OnRetainerAdditionalTask);
            Svc.PluginInterface.GetIpcSubscriber<string, string, object>(ApiConsts.OnRetainerReadyForPostprocess).Subscribe(OnRetainerReadyForPostprocessIntl);
            Svc.PluginInterface.GetIpcSubscriber<ulong, string, object>(ApiConsts.OnRetainerSettingsDraw).Subscribe(OnRetainerSettingsDrawAction);
            Svc.PluginInterface.GetIpcSubscriber<ulong, string, object>(ApiConsts.OnRetainerPostVentureTaskDraw).Subscribe(OnRetainerPostVentureTaskDrawAction);
            Svc.PluginInterface.GetIpcSubscriber<object>(ApiConsts.OnRetainerListTaskButtonsDraw).Subscribe(OnRetainerListTaskButtonsDrawAction);
        }

        /// <summary>
        /// Request that AutoRetainer should go through all retainers in list and execute an IPC task from current plugin. Tasks from another plugins won't be executed. Only use this method from inside <see cref="OnRetainerListTaskButtonsDraw"/> event.
        /// </summary>
        public void ProcessIPCTaskFromOverlay()
        {
            Svc.PluginInterface.GetIpcSubscriber<string, object>(ApiConsts.OnRetainerListCustomTask).InvokeAction(ECommonsMain.Instance.Name);
        }


        /// <summary>
        /// Fire inside <see cref="OnRetainerPostprocessStep"/> event to indicate that you want to do the postprocessing of a retainer.
        /// </summary>
        public void RequestPostprocess()
        {
            Svc.PluginInterface.GetIpcSubscriber<string, object>("AutoRetainer.RequestPostprocess").InvokeAction(ECommonsMain.Instance.Name);
        }

        /// <summary>
        /// Fire inside <see cref="OnRetainerReadyToPostprocess"/> to indicate that you have finished your postprocessing tasks
        /// </summary>
        public void FinishPostProcess() => Svc.PluginInterface.GetIpcSubscriber<object>("AutoRetainer.FinishPostprocessRequest").InvokeAction();


        /// <summary>
        /// Indicates whether AutoRetainer's API has been initialized and is ready to use.
        /// </summary>
        public bool Ready 
        { 
            get
            {
                try
                {
                    Svc.PluginInterface.GetIpcSubscriber<object>("AutoRetainer.Init").InvokeAction();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Allows to set suppressed state in which AutoRetainer will not perform any actions regardless of configuration.
        /// </summary>
        public bool Suppressed
        {
            get
            {
                return Svc.PluginInterface.GetIpcSubscriber<bool>("AutoRetainer.GetSuppressed").InvokeFunc();
            }
            set
            {
                Svc.PluginInterface.GetIpcSubscriber<bool, object>("AutoRetainer.SetSuppressed").InvokeAction(value);
            }
        }

        /// <summary>
        /// Disposes API. Don't forget to call it.
        /// </summary>
        public void Dispose()
        {
            Svc.PluginInterface.GetIpcSubscriber<string, object>(ApiConsts.OnSendRetainerToVenture).Unsubscribe(OnSendRetainerToVentureAction);
            Svc.PluginInterface.GetIpcSubscriber<string, object>(ApiConsts.OnRetainerAdditionalTask).Unsubscribe(OnRetainerAdditionalTask);
            Svc.PluginInterface.GetIpcSubscriber<string, string, object>(ApiConsts.OnRetainerReadyForPostprocess).Unsubscribe(OnRetainerReadyForPostprocessIntl);
            Svc.PluginInterface.GetIpcSubscriber<ulong, string, object>(ApiConsts.OnRetainerSettingsDraw).Unsubscribe(OnRetainerSettingsDrawAction);
            Svc.PluginInterface.GetIpcSubscriber<ulong, string, object>(ApiConsts.OnRetainerPostVentureTaskDraw).Unsubscribe(OnRetainerPostVentureTaskDrawAction);
            Svc.PluginInterface.GetIpcSubscriber<object>(ApiConsts.OnRetainerListTaskButtonsDraw).Unsubscribe(OnRetainerListTaskButtonsDrawAction);
        }

        /// <summary>
        /// While inside <see cref="OnSendRetainerToVenture"/>, allows you to set venture where retainer will be sent, overriding any user-defined configuration.
        /// </summary>
        /// <param name="ventureId"></param>
        public void SetVenture(uint ventureId)
        {
            Svc.PluginInterface.GetIpcSubscriber<uint, object>("AutoRetainer.SetVenture").InvokeAction(ventureId);
        }

        /// <summary>
        /// Retrieves <see cref="OfflineCharacterData"/> for specified content ID. Warning! This data must not be stored. You must access this function every time you want to read <see cref="OfflineCharacterData"/> that is up to date. 
        /// </summary>
        /// <param name="cid">Content ID of a character</param>
        /// <returns></returns>
        public OfflineCharacterData GetOfflineCharacterData(ulong cid)
        {
            return Svc.PluginInterface.GetIpcSubscriber<ulong, OfflineCharacterData>("AutoRetainer.GetOfflineCharacterData").InvokeFunc(cid);
        }

        /// <summary>
        /// Writes <see cref="OfflineCharacterData"/> to AutoRetainer. If another instance of <see cref="OfflineCharacterData"/> is already present for specified content ID, it will be replaced. Warning! You must read the data, make changes and immediately write it back within single framework update, storing <see cref="OfflineCharacterData"/> is prohibited.
        /// </summary>
        /// <param name="data"></param>
        public void WriteOfflineCharacterData(OfflineCharacterData data)
        {
            if (data.CreationFrame != Svc.PluginInterface.UiBuilder.FrameCount) throw new Exception("You must read the data, make changes and immediately write it back within single framework update, storing OfflineCharacterData is prohibited.");
            Svc.PluginInterface.GetIpcSubscriber<OfflineCharacterData, object>("AutoRetainer.WriteOfflineCharacterData").InvokeAction(data);
        }

        /// <summary>
        /// Retrieves <see cref="AdditionalRetainerData"/> for specified character and retainer. Warning! This data must not be stored. You must access this function every time you want to read <see cref="AdditionalRetainerData"/> that is up to date. 
        /// </summary>
        /// <param name="cid">Target character's content ID</param>
        /// <param name="name">Retainer's name</param>
        /// <returns></returns>
        public AdditionalRetainerData GetAdditionalRetainerData(ulong cid, string name)
        {
            return Svc.PluginInterface.GetIpcSubscriber<ulong, string, AdditionalRetainerData>("AutoRetainer.GetAdditionalRetainerData").InvokeFunc(cid, name);
        }

        /// <summary>
        /// Writes <see cref="AdditionalRetainerData"/> to AutoRetainer. Warning! You must read the data, make changes and immediately write it back within single framework update, storing <see cref="AdditionalRetainerData"/> is prohibited.
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="name"></param>
        /// <param name="data"></param>
        public void WriteAdditionalRetainerData(ulong cid, string name, AdditionalRetainerData data)
        {
            if (data.CreationFrame != Svc.PluginInterface.UiBuilder.FrameCount) throw new Exception("You must read the data, make changes and immediately write it back within single framework update, storing AdditionalRetainerData is prohibited.");
            Svc.PluginInterface.GetIpcSubscriber<ulong, string, AdditionalRetainerData, object>("AutoRetainer.WriteAdditionalRetainerData").InvokeAction(cid, name, data);
        }

        /// <summary>
        /// Returns all known characters' CIDs, excluding blacklisted and not initialized.
        /// </summary>
        /// <returns></returns>
        public List<ulong> GetRegisteredCharacters()
        {
            return Svc.PluginInterface.GetIpcSubscriber<List<ulong>>("AutoRetainer.GetRegisteredCIDs").InvokeFunc();
        }
    }
}