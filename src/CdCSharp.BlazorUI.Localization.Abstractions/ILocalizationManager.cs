using System;
using System.Collections.Generic;
using System.Text;

namespace CdCSharp.BlazorUI.Localization.Abstractions;

public interface ILocalizationManager
{
    Task SetCultureAsync(string cultureName, NavigationManager navigator)
}
