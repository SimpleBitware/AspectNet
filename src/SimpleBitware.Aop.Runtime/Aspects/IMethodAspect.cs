using System;

namespace SimpleBitware.Aop.Runtime.Aspects;

public interface IMethodAspect
{
    void OnBefore(MethodContext context);
    void OnSuccess(MethodContext context);
    void OnException(MethodContext context, Exception exception);
}