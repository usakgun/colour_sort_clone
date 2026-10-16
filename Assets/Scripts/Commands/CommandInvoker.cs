using System.Collections.Generic;

public class CommandInvoker
{
    private readonly Stack<ICommand> _commandHistory = new Stack<ICommand>();

    public bool ExecuteCommand(ICommand command)
    {
        if (command.Execute())
        {
            _commandHistory.Push(command);
            return true;
        }
        return false;
    }

    public bool UndoLastCommand()
    {
        if (_commandHistory.Count > 0)
        {
            ICommand command = _commandHistory.Pop();
            command.Undo();
            return true;
        }
        return false;
    }
}