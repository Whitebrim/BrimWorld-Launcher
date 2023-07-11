using System;

namespace Launcher.Extensions
{
    public class InvalidUsernameException : Exception { };

    public class LaunchArgumentException : Exception
    {
        public LaunchArgumentException(string message)
            : base(message) { }
    };
}
