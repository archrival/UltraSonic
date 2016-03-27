using System;

namespace SplitViewMenu
{
    public interface INavigationMenuItem
    {
        Type DestinationPage { get; }
        object Arguments { get; }
        string Label { get; }
    }
}