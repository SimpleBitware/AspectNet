using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

public sealed class TryCatchBuilder : InstructionSetBlockBuilderBase<TryCatchBuilder>
{
    private readonly Instruction tryStartInstruction;
    private readonly Instruction catchStartInstruction;
    private readonly Instruction finallyStartInstruction;
    private readonly Instruction exitInstruction;
    
    private readonly ExceptionHandler catchHandler;
    private readonly ExceptionHandler finallyHandler;

    public TryCatchBuilder(MethodDefinition method, ILProcessor processor, ModuleCache moduleCache) : base(method, processor, moduleCache)
    {
        tryStartInstruction = processor.Create(OpCodes.Nop);
        catchStartInstruction = processor.Create(OpCodes.Nop);
        finallyStartInstruction = processor.Create(OpCodes.Nop);
        exitInstruction = processor.Create(OpCodes.Nop);

        catchHandler = new ExceptionHandler(ExceptionHandlerType.Catch)
        {
            TryStart = tryStartInstruction,
            TryEnd = catchStartInstruction,
            HandlerStart = catchStartInstruction,
            HandlerEnd = finallyStartInstruction,
            CatchType = moduleCache.ImportReference(typeof(Exception))
        };
        finallyHandler = new ExceptionHandler(ExceptionHandlerType.Finally)
        {
            TryStart = tryStartInstruction,
            TryEnd = finallyStartInstruction,
            HandlerStart = finallyStartInstruction,
            HandlerEnd = exitInstruction
        };
    }
    
    public TryCatchBuilder StartTry()
    {
        Instructions.Add(tryStartInstruction);
        return this;
    }
    
    public TryCatchBuilder EndTry()
    {
        Instructions.Add(Processor.Create(OpCodes.Leave, exitInstruction));
        return this;
    }
    
    public TryCatchBuilder StartCatch()
    {
        Instructions.Add(catchStartInstruction);
        return this;
    }
    
    public TryCatchBuilder EndCatch()
    {
        Instructions.Add(Processor.Create(OpCodes.Leave, exitInstruction));
        return this;
    }
    
    public TryCatchBuilder StartFinally()
    {
        Instructions.Add(finallyStartInstruction);
        return this;
    }
    
    public TryCatchBuilder EndFinally()
    {
        Instructions.Add(Processor.Create(OpCodes.Endfinally));
        Instructions.Add(exitInstruction);
        
        Method.Body.ExceptionHandlers.Add(catchHandler);
        Method.Body.ExceptionHandlers.Add(finallyHandler);
        
        return this;
    }
}
