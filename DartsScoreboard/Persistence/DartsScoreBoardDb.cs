﻿using IndexedDB.Blazor;
using Microsoft.JSInterop;
using System;

namespace DartsScoreboard;

public class DartsScoreBoardDb : IndexedDb
{
    public DartsScoreBoardDb(IJSRuntime jSRuntime, string name, int version) : base(jSRuntime, name, version) { }
    public IndexedSet<User> Users { get; set; }
}
