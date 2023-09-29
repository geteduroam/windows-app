﻿using System.CommandLine;

namespace App.MsiCreator.Commands
{
    /// <summary>
    /// Command interface.
    /// Every class implementing this interface will be added to the list of available commands
    /// by the <see cref="Engine"/>
    /// </summary>
    public interface ICommand
    {
        Command GetCommand();
    }
}
