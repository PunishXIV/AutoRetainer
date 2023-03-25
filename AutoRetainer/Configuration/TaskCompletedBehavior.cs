namespace AutoRetainer.Configuration;

public enum TaskCompletedBehavior
{
    Close_retainer_list_and_disable_plugin,
    Close_retainer_list_and_keep_plugin_enabled,
    Stay_in_retainer_list_and_disable_plugin,
    Stay_in_retainer_list_and_keep_plugin_enabled,
}
