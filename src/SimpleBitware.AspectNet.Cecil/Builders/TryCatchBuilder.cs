using Mono.Cecil;
using Mono.Cecil.Cil;
using SimpleBitware.AspectNet.Cecil.Runtime;

namespace SimpleBitware.AspectNet.Cecil.Builders;

/// <summary>
/// Builds try-catch-finally exception handling blocks in IL.
/// </summary>
/// <remarks>
/// This sealed builder provides methods to construct proper try-catch-finally structures
/// with appropriate jump targets and exception handlers. It extends <see cref="InstructionSetBlockBuilderBase{TBuilder}"/>
/// and manages the creation of exception handling instructions and handler objects.
/// </remarks>
public sealed class TryCatchBuilder : InstructionSetBlockBuilderBase<TryCatchBuilder>
{
    /// <summary>
    /// Gets the instruction marking the start of the try block.
    /// </summary>
    private readonly Instruction tryStartInstruction;
    
    /// <summary>
    /// Gets the instruction marking the start of the catch block.
    /// </summary>
    private readonly Instruction catchStartInstruction;
    
    /// <summary>
    /// Gets the instruction marking the start of the finally block.
    /// </summary>
    private readonly Instruction finallyStartInstruction;
    
    /// <summary>
    /// Gets the instruction marking the exit point after all blocks.
    /// </summary>
    private readonly Instruction exitInstruction;
    
    /// <summary>
    /// Gets the exception handler for the catch block.
    /// </summary>
    private readonly ExceptionHandler catchHandler;
    
    /// <summary>
    /// Gets the exception handler for the finally block.
    /// </summary>
    private readonly ExceptionHandler finallyHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="TryCatchBuilder"/> class.
    /// </summary>
    /// <param name="method">The method definition to build exception handlers for.</param>
    /// <param name="processor">The IL processor for creating instructions.</param>
    /// <param name="moduleCache">The module cache for importing types.</param>
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
    
    /// <summary>
    /// Marks the start of the try block.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public TryCatchBuilder StartTry()
    {
        Instructions.Add(tryStartInstruction);
        return this;
    }
    
    /// <summary>
    /// Marks the end of the try block and adds a leave instruction to the exit point.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public TryCatchBuilder EndTry()
    {
        Instructions.Add(Processor.Create(OpCodes.Leave, exitInstruction));
        return this;
    }
    
    /// <summary>
    /// Marks the start of the catch block.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public TryCatchBuilder StartCatch()
    {
        Instructions.Add(catchStartInstruction);
        return this;
    }
    
    /// <summary>
    /// Marks the end of the catch block and adds a leave instruction to the exit point.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public TryCatchBuilder EndCatch()
    {
        Instructions.Add(Processor.Create(OpCodes.Leave, exitInstruction));
        return this;
    }
    
    /// <summary>
    /// Marks the start of the finally block.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    public TryCatchBuilder StartFinally()
    {
        Instructions.Add(finallyStartInstruction);
        return this;
    }
    
    /// <summary>
    /// Marks the end of the finally block, adds an end_finally instruction, and registers both exception handlers.
    /// </summary>
    /// <returns>The current builder instance for method chaining.</returns>
    /// <remarks>
    /// This method must be called last to properly finalize the try-catch-finally structure
    /// and register the exception handlers with the method body.
    /// </remarks>
    public TryCatchBuilder EndFinally()
    {
        Instructions.Add(Processor.Create(OpCodes.Endfinally));
        Instructions.Add(exitInstruction);
        
        Method.Body.ExceptionHandlers.Add(catchHandler);
        Method.Body.ExceptionHandlers.Add(finallyHandler);
        
        return this;
    }
}
