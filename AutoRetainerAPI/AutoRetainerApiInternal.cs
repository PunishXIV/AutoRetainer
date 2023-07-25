using ECommons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRetainerAPI
{
    public partial class AutoRetainerApi
    {
        private void OnRetainerListTaskButtonsDrawAction()
        {
            if (OnRetainerListTaskButtonsDraw != null)
            {
                GenericHelpers.Safe(() => OnRetainerListTaskButtonsDraw());
            }
        }

        private void OnRetainerPostVentureTaskDrawAction(ulong cid, string retainer)
        {
            if (OnRetainerPostVentureTaskDraw != null)
            {
                GenericHelpers.Safe(() => OnRetainerPostVentureTaskDraw(cid, retainer));
            }
        }

        private void OnRetainerSettingsDrawAction(ulong cid, string retainer)
        {
            if (OnRetainerSettingsDraw != null)
            {
                GenericHelpers.Safe(() => OnRetainerSettingsDraw(cid, retainer));
            }
        }
        private void OnRetainerReadyForPostprocessIntl(string plugin, string retainer)
        {
            if (ECommonsMain.Instance.Name == plugin)
            {
                if (OnRetainerReadyToPostprocess != null)
                {
                    GenericHelpers.Safe(() => OnRetainerReadyToPostprocess(retainer));
                }
            }
        }

        void OnSendRetainerToVentureAction(string n)
        {
            if (OnSendRetainerToVenture != null)
            {
                GenericHelpers.Safe(() => OnSendRetainerToVenture(n));
            }
        }

        void OnRetainerAdditionalTask(string n)
        {
            if (OnRetainerPostprocessStep != null)
            {
                GenericHelpers.Safe(() => OnRetainerPostprocessStep(n));
            }
        }
    }
}
