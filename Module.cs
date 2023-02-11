using System;
using Dalamud.Logging;

namespace Hypostasis;

public abstract class Module
{
    //public static Module Instance { get; private set; } // Can be implemented to enable access to a module's instance

    private bool? isValid;
    public bool IsValid
    {
        get
        {
            try
            {
                return isValid ?? (isValid = Validate()).Value;
            }
            catch (Exception e)
            {
                PluginLog.Error(e, $"Error validating {this}");
                return (isValid = false).Value;
            }
        }
        set
        {
            if (!value)
                Invalidate();
        }
    }

    public bool IsEnabled { get; set; }

    public virtual bool ShouldEnable => true;

    protected virtual bool Validate() => true;
    protected virtual void Enable() { }
    protected virtual void Disable() { }

    public void Toggle()
    {
        if (!IsValid) return;

        if (!IsEnabled)
        {
            Enable();
            IsEnabled = true;
        }
        else
        {
            Disable();
            IsEnabled = false;
        }
    }

    private void Invalidate()
    {
        if (!IsValid) return;
        Disable();
        isValid = false;
    }
}