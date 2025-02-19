using HitachiQA.Backend.API.Services.Executors;

namespace HitachiQA.Backend.API.Enums;

public enum ActionTypeEnum
{
    None=0,

    [ActionExecutor(typeof(ClickExecutor))]
    Click,

    [ActionExecutor(typeof(SetFieldValueExecutor))]
    SetFieldValue,
    
    [ActionExecutor(typeof(NavigateExecutor))]
    Navigate
}

[AttributeUsage(AttributeTargets.Field)] // 👈 Applied to enum fields
public class ActionExecutorAttribute : Attribute
{
    public Type ImplementationType { get; }

    public ActionExecutorAttribute(Type implementationType)
    {
        ImplementationType = implementationType;
    }
}