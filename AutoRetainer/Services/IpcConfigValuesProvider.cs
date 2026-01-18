using ECommons.EzIpcManager;
using System.Linq.Expressions;
using System.Reflection;

namespace AutoRetainer.Services;

public class IpcConfigValuesProvider : IDisposable
{
    private List<Action> IpcUnregisterActions = [];
    private IpcConfigValuesProvider()
    {
        var configType = C.GetType();
        var fields = configType.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach(var field in fields)
        {
            var fieldType = field.FieldType;
            var fieldName = field.Name;
            var ipcName = $"AutoRetainer.GetConfig.{fieldName}";

            var getIpcProviderMethod = EzIPC.FindIpcProvider(1).MakeGenericMethod(fieldType);

            var ipcProvider = getIpcProviderMethod.Invoke(
                Svc.PluginInterface,
                [ipcName]
            );

            var registerFuncMethod = ipcProvider.GetType().GetMethod("RegisterFunc");

            var funcType = typeof(Func<>).MakeGenericType(fieldType);

            // Use a helper method that creates the correctly typed lambda
            var helperMethod = typeof(Utils)
                .GetMethod("CreateGetter", BindingFlags.Public | BindingFlags.Static)
                .MakeGenericMethod(fieldType);

            var lambda = (Delegate)helperMethod.Invoke(this, new object[] { field });

    registerFuncMethod.Invoke(ipcProvider, [lambda]);

            var unregisterFuncMethod = ipcProvider.GetType().GetMethod("UnregisterFunc");

            IpcUnregisterActions.Add(() =>
                unregisterFuncMethod.Invoke(ipcProvider, null)
            );
        }
    }

    public void Dispose()
    {
        foreach(var unregisterAction in IpcUnregisterActions)
        {
            unregisterAction();
        }
        IpcUnregisterActions.Clear();
    }
}
