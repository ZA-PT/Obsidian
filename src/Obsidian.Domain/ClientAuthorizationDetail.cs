﻿using System;
using System.Collections.Generic;

namespace Obsidian.Domain
{
    public class ClientAuthorizationDetail
    {
        public Guid ClientId { get; set; }

        public IEnumerable<string> ScopeNames { get; set; }
    }
}
